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

using GameOverlay.Drawing;
using MapAssist.Helpers;
using MapAssist.Structs;
using System;
using System.Collections.Generic;

namespace MapAssist.Types
{
    public class UnitAny
    {
        public IntPtr pUnit { get; private set; }
        public Structs.UnitAny Struct { get; private set; }
        public UnitType UnitType => Struct.UnitType;
        public uint UnitId => Struct.UnitId;
        public uint TxtFileNo => Struct.TxtFileNo;
        public Area Area { get; private set; }
        public Point Position => new Point(X, Y);
        public ushort X => IsMovable ? Path.DynamicX : Path.StaticX;
        public ushort Y => IsMovable ? Path.DynamicY : Path.StaticY;
        public Dictionary<Stat, Dictionary<ushort, int>> StatLayers { get; private set; }
        public Dictionary<Stat, int> Stats { get; private set; }
        protected uint[] StateFlags { get; set; }
        public bool IsHovered { get; set; } = false;
        public bool IsCached { get; private set; } = false;
        private Path Path { get; set; }
        public bool IsCorpse => Struct.isCorpse && UnitId != GameMemory.PlayerUnit.UnitId && Area != Area.None;

        public UnitAny(IntPtr pUnit)
        {
            this.pUnit = pUnit;

            if (IsValidPointer)
            {
                using (var processContext = GameManager.GetProcessContext())
                {
                    Struct = processContext.Read<Structs.UnitAny>(this.pUnit);
                }
            }
        }

        public void UpdateStruct(IntPtr pUnit, Structs.UnitAny Struct)
        {
            this.pUnit = pUnit;
            this.Struct = Struct;
        }

        protected bool Update()
        {
            if (IsValidPointer)
            {
                using (var processContext = GameManager.GetProcessContext())
                {
                    Struct = processContext.Read<Structs.UnitAny>(pUnit);

                    if (IsCached) return false;

                    if (IsValidUnit)
                    {
                        Path = new Path(Struct.pPath);
                        Area = Path.Room.RoomEx.Level.LevelId;

                        if (Struct.pStatsListEx != IntPtr.Zero)
                        {
                            var stats = new Dictionary<Stat, int>();
                            var statLayers = new Dictionary<Stat, Dictionary<ushort, int>>();

                            var statListStruct = processContext.Read<StatListStruct>(Struct.pStatsListEx);
                            StateFlags = statListStruct.StateFlags;

                            var statValues = processContext.Read<StatValue>(statListStruct.Stats.pFirstStat, Convert.ToInt32(statListStruct.Stats.Size));
                            foreach (var stat in statValues)
                            {
                                if (statLayers.ContainsKey(stat.Stat))
                                {
                                    if (stat.Layer == 0) continue;
                                    if (!statLayers[stat.Stat].ContainsKey(stat.Layer))
                                    {
                                        statLayers[stat.Stat].Add(stat.Layer, stat.Value);
                                    }
                                }
                                else
                                {
                                    stats.Add(stat.Stat, stat.Value);
                                    statLayers.Add(stat.Stat, new Dictionary<ushort, int>() { { stat.Layer, stat.Value } });
                                }
                            }

                            Stats = stats;
                            StatLayers = statLayers;
                        }

                        if (GameMemory.cache.ContainsKey(UnitId)) IsCached = true;

                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMovable
        {
            get => !(Struct.UnitType == UnitType.Object || Struct.UnitType == UnitType.Item);
        }

        public bool IsValidPointer
        {
            get => pUnit != IntPtr.Zero;
        }

        public bool IsValidUnit
        {
            get => Struct.pUnitData != IntPtr.Zero && Struct.pPath != IntPtr.Zero && Struct.UnitType <= UnitType.Tile;
        }

        public bool IsPlayer
        {
            get => Struct.UnitType == UnitType.Player && Struct.pAct != IntPtr.Zero;
        }

        public bool IsPlayerOwned
        {
            get => IsMerc && Stats.ContainsKey(Stat.Strength); // This is ugly, but seems to work.
        }

        public bool IsMonster
        {
            get
            {
                if (Struct.UnitType != UnitType.Monster) return false;
                if (Struct.Mode == 0 || Struct.Mode == 12) return false;
                if (NPC.Dummies.ContainsKey(TxtFileNo)) { return false; }

                return true;
            }
        }

        public bool IsMerc
        {
            get => new List<Npc> { Npc.Rogue2, Npc.Guard, Npc.IronWolf, Npc.Act5Hireling2Hand }.Contains((Npc)TxtFileNo);
        }

        public double DistanceTo(UnitAny other)
        {
            return Math.Sqrt((Math.Pow(other.X - Position.X, 2) + Math.Pow(other.Y - Position.Y, 2)));
        }

        public override bool Equals(object obj) => obj is UnitAny other && Equals(other);

        public bool Equals(UnitAny unit) => !(unit is null) && UnitId == unit.UnitId;

        public override int GetHashCode() => UnitId.GetHashCode();

        public static bool operator ==(UnitAny unit1, UnitAny unit2) => (unit1 is null && unit2 is null) || (!(unit1 is null) && unit1.Equals(unit2));

        public static bool operator !=(UnitAny unit1, UnitAny unit2) => !(unit1 == unit2);
    }
}
