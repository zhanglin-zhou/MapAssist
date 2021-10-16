/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/D2RAssist/
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

using System.Drawing;

namespace D2RAssist.Types
{
    public enum MapPosition
    {
        TopLeft,
        TopRight
    }

    public static class Settings
    {
        public static class Map
        {
            public static class Colors
            {
                public static readonly Color DoorNext = Color.FromArgb(237, 107, 0);
                public static readonly Color DoorPrevious = Color.FromArgb(255, 0, 149);
                public static readonly Color Waypoint = Color.FromArgb(16, 140, 235);
                public static readonly Color Player = Color.FromArgb(255, 255, 0);
                public static readonly Color SuperChest = Color.FromArgb(17, 255, 0);
                public static readonly Color TextColor = Color.Chartreuse;
                public static readonly Color PointOfInterestLine = Color.Chartreuse;
            }

            public static readonly Font Font = new Font("Arial", 8);
            public const double Opacity = 0.70;
            public const bool AlwaysOnTop = true;
            public const bool HideInTown = true;
            public const int Size = 450;
            public const MapPosition Position = MapPosition.TopRight;
            public const int UpdateTime = 100;
            public const bool Rotate = true;
            public const char ShowHideKey = '\\';

            public static readonly Area[] PrefetchAreas =
            {
                Area.CatacombsLevel2,
                Area.DuranceOfHateLevel2,
            };

            public const bool ClearPrefetchedOnAreaChange = false;
        }

        public static class Api
        {
            public static string Endpoint = "http://localhost:8080/";
        }
    }
}
