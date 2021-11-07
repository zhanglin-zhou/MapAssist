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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MapAssist.Helpers;
using MapAssist.Interfaces;
using MapAssist.Structs;

namespace MapAssist.Types
{
    public class UnitAny : IUpdatable<UnitAny>
    {
        private readonly IntPtr _pUnit;
        private Structs.UnitAny _unitAny;
        private Act _act;
        private Path _path;
        private Inventory _inventory;
        private MonsterData _monsterData;
        private string _name;

        public UnitAny(IntPtr pUnit)
        {
            _pUnit = pUnit;
            Update();
        }

        public UnitAny Update()
        {
            if (IsValid())
            {
                using (var processContext = GameManager.GetProcessContext())
                {
                    _unitAny = processContext.Read<Structs.UnitAny>(_pUnit);
                    _path = new Path(_unitAny.pPath);

                    if (IsPlayer())
                    {
                        _name = Encoding.ASCII.GetString(processContext.Read<byte>(_unitAny.pUnitData, 16)).TrimEnd((char)0);
                        _inventory = processContext.Read<Inventory>(_unitAny.pInventory);
                        _act = new Act(_unitAny.pAct);
                    }

                    if (IsMonster())
                    {
                        _monsterData = processContext.Read<MonsterData>(_unitAny.pUnitData);
                        // var temp = ReadStruct<Structs.SubName>(monsterData.pMonStats);
                        // var unitClassName = new string((sbyte*)temp.ClassName, 0, 16).Trim().Replace("\0", "");// Encoding.UTF8.GetString(temp.Name);
                    }
                }
            }
            return this;
        }

        public string Name { get => _name;  }
        public UnitType UnitType { get => _unitAny.UnitType; }
        public uint TxtFileNo { get => _unitAny.TxtFileNo; }
        public uint UnitId { get => _unitAny.UnitId; }
        public uint Mode { get => _unitAny.Mode; }
        public IntPtr UnitDataPtr { get => _unitAny.pUnitData; }
        public MonsterData MonsterData { get => _monsterData; }
        public Act Act { get => _act; }
        public Path Path { get => _path; }
        public IntPtr StatsListExPtr { get => _unitAny.pStatsListEx; }
        public Inventory Inventory { get => _inventory; }
        public uint OwnerType { get => _unitAny.OwnerType; }
        public ushort X { get => IsMovable() ? _path.DynamicX : _path.StaticX; }
        public ushort Y { get => IsMovable() ? _path.DynamicY : _path.StaticY; }
        public Point Position { get => new Point(X, Y); }
        public UnitAny ListNext { get => new UnitAny(_unitAny.pListNext); }
        public UnitAny RoomNext { get => new UnitAny(_unitAny.pRoomNext); }

        public bool IsMovable()
        {
            return !(UnitType == UnitType.Object || UnitType == UnitType.Item);
        }

        public bool IsValid()
        {
            return _pUnit != IntPtr.Zero;
        }

        public bool IsPlayer()
        {
            return UnitType == UnitType.Player;
        }

        public bool IsPlayerUnit()
        {
            return IsPlayer() && _unitAny.pInventory != IntPtr.Zero && Inventory.pUnk1 != IntPtr.Zero;
        }

        public bool IsMonster()
        {
            if (_unitAny.UnitType != UnitType.Monster) return false;
            if (_unitAny.Mode == 0 || _unitAny.Mode == 12) return false;
            if ((_unitAny.TxtFileNo >= 110 && _unitAny.TxtFileNo <= 113) || (_unitAny.TxtFileNo == 608 && _unitAny.Mode == 8)) return false;
            if (_unitAny.TxtFileNo == 68 && _unitAny.Mode == 14) return false;
            if ((_unitAny.TxtFileNo == 258 || _unitAny.TxtFileNo == 261) && (_unitAny.Mode == 14)) return false;
            // if (D2COMMON_get_unit_stat(unit, 172, 0) == 2) return 0;
            var falsePositives = new List<uint> { 227, 283, 326, 327, 328, 329, 330, 410, 411, 412, 413, 414, 415, 416, 366, 406, 351, 352, 353, 266, 408, 516, 517, 518, 519, 522, 523, 543, 543, 545 };
            if (falsePositives.Contains(_unitAny.TxtFileNo)) return false;
            // ms_wchar_t* wname = D2CLIENT_get_unit_name(unit);
            // if ((strcmp(name, "an evil force") == 0) || (strcmp(name, "dummy") == 0))
            return true;
        }

        public bool IsElite()
        {
            return _monsterData.MonsterType > 0;
        }

        public override bool Equals(object obj) => obj is UnitAny other && Equals(other);

        public bool Equals(UnitAny unit) => UnitId == unit.UnitId;

        public override int GetHashCode() => UnitId.GetHashCode();

        public static bool operator ==(UnitAny unit1, UnitAny unit2) => unit1.Equals(unit2);

        public static bool operator !=(UnitAny unit1, UnitAny unit2) => !(unit1 == unit2);
    }
}
