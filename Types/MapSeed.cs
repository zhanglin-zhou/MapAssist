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

using MapAssist.Helpers;
using MapAssist.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapAssist.Types
{
    public class MapSeed : IUpdatable<MapSeed>
    {
        private readonly IntPtr _pMapSeed;
        public uint Seed { get; private set; }

        public MapSeed(IntPtr pMapSeed)
        {
            _pMapSeed = pMapSeed;
            Update();
        }

        public MapSeed Update()
        {
            using (var processContext = GameManager.GetProcessContext())
            {
                try
                {
                    var pMapSeedData = processContext.Read<IntPtr>(_pMapSeed);
                    var mapSeedData = processContext.Read<Structs.MapSeed>(pMapSeedData);

                    uint check = 0;

                    switch (GameManager.OffsetCheck)
                    {
                        case "Default":
                            Seed = mapSeedData.mapSeed2;
                            if (Seed == 0) Seed = mapSeedData.mapSeed1;

                            break;

                        case "110":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed1 : mapSeedData.mapSeed2;
                            break;

                        case "124":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed2 : mapSeedData.mapSeed1;
                            break;

                        case "830":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed1 : mapSeedData.mapSeed2;
                            break;

                        case "870":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed1 : mapSeedData.mapSeed2;
                            break;

                        case "990":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed2 : mapSeedData.mapSeed1;
                            break;

                        case "BE0":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed2 : mapSeedData.mapSeed1;
                            break;

                        case "F60":
                            check = processContext.Read<uint>(IntPtr.Add(pMapSeedData, Convert.ToInt32(GameManager.OffsetCheck, 16)));
                            Seed = check > 0 ? mapSeedData.mapSeed2 : mapSeedData.mapSeed1;
                            break;
                    }
                }
                catch (Exception) { }
            }
            return this;
        }
    }
}
