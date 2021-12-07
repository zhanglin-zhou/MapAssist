/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using MapAssist.Properties;
using MapAssist.Settings;
using MapAssist.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

#pragma warning disable 649

namespace MapAssist.Helpers
{
    public class MapApi
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private static Process _pipeClient;
        private static Thread _pipeReaderThread;
        private static object _pipeRequestLock = new object();
        private static BlockingCollection<(uint, string)> collection = new BlockingCollection<(uint, string)>();
        
        private readonly ConcurrentDictionary<Area, AreaData> _cache;
        private Difficulty _difficulty;
        private uint _mapSeed;

        public static bool StartPipedChild()
        {
            File.WriteAllBytes("piped.exe", Resources.piped);

            _pipeClient = new Process();
            _pipeClient.StartInfo.FileName = "piped.exe";
            _pipeClient.StartInfo.UseShellExecute = false;
            _pipeClient.StartInfo.RedirectStandardOutput = true;
            _pipeClient.StartInfo.RedirectStandardInput = true;
            _pipeClient.StartInfo.RedirectStandardError = true;
            
            try
            {
                _pipeClient.Start();
            }
            catch (Exception ex)
            {
                return false;
            }

            var streamReader = _pipeClient.StandardOutput;

            _pipeReaderThread = new Thread(() =>
            {
                Func<int, byte[]> ReadBytes = (length) =>
                {
                    var data = new byte[0];
                    while (data.Length < length)
                    {
                        var tryReadLength = length - data.Length;
                        var chunk = new byte[tryReadLength];
                        var readLength = streamReader.BaseStream.Read(chunk, 0, tryReadLength);

                        data = Combine(data, chunk.Take(readLength).ToArray());
                    }
                    return data;
                };

                while (true)
                {
                    var length = BitConverter.ToUInt32(ReadBytes(4), 0);

                    if (length == 0)
                    {
                        collection.Add((0, null));
                        continue;
                    }

                    var json = Encoding.UTF8.GetString(ReadBytes((int)length));
                    var jsonObj = JObject.Parse(json);

                    if (jsonObj.ContainsKey("error"))
                    {
                        _log.Error(jsonObj["error"].ToString());
                        continue;
                    }

                    collection.Add((length, json));
                }
            });
            _pipeReaderThread.Start();

            var (startupLength, _) = collection.Take();
            return startupLength == 0;
        }
        
        public MapApi(Difficulty difficulty, uint mapSeed)
        {
            _difficulty = difficulty;
            _mapSeed = mapSeed;

            // Cache for pre-fetching maps for the surrounding areas.
            _cache = new ConcurrentDictionary<Area, AreaData>();

            Prefetch(MapAssistConfiguration.Loaded.PrefetchAreas);
        }

        public AreaData GetMapData(Area area)
        {
            if (!_cache.TryGetValue(area, out AreaData areaData))
            {
                // Not in the cache, block.
                _log.Info($"Cache miss on {area}");
                areaData = GetMapDataInternal(area);
            }

            Area[] adjacentAreas = areaData.AdjacentLevels.Keys.ToArray();
            if (adjacentAreas.Any())
            {
                Prefetch(adjacentAreas);
            }

            return areaData;
        }

        private void Prefetch(params Area[] areas)
        {
            var prefetchBackgroundWorker = new BackgroundWorker();
            prefetchBackgroundWorker.DoWork += (sender, args) =>
            {
                if (MapAssistConfiguration.Loaded.ClearPrefetchedOnAreaChange)
                {
                    _cache.Clear();
                }

                // Special value telling us to exit.
                if (areas.Length == 0)
                {
                    _log.Info("Prefetch worker terminating");
                    return;
                }

                foreach (Area area in areas)
                {
                    if (_cache.ContainsKey(area)) continue;

                    _cache[area] = GetMapDataInternal(area);
                    _log.Info($"Prefetched {area}");
                }
            };
            prefetchBackgroundWorker.RunWorkerAsync();
            prefetchBackgroundWorker.Dispose();
        }

        private AreaData GetMapDataInternal(Area area)
        {
            var req = new Req();
            req.seed = _mapSeed;
            req.difficulty = (uint)_difficulty;
            req.levelId = (uint)area;

            var writer = _pipeClient.StandardInput;

            var data = ToBytes(req);
            lock (_pipeRequestLock)
            {
                writer.BaseStream.Write(data, 0, data.Length);
                writer.BaseStream.Flush();

                var (length, json) = collection.Take();
                var rawAreaData = JsonConvert.DeserializeObject<RawAreaData>(json);

                return rawAreaData.ToInternal(area);
            }
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            var ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private static byte[] ToBytes(Req req)
        {
            var size = Marshal.SizeOf(req);
            var arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(req, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Req
        {
            public uint seed;
            public uint difficulty;
            public uint levelId;
        }

        public static void Dispose()
        {
            _pipeReaderThread.Abort();
        }
    }
}
