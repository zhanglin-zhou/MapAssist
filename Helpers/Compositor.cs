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

using D2RAssist.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace D2RAssist.Helpers
{
    public class Compositor
    {
        private readonly AreaData _areaData;
        private readonly Bitmap _background;
        private readonly IReadOnlyList<PointOfInterest> _pointsOfInterest;

        public Compositor(AreaData areaData, IReadOnlyList<PointOfInterest> pointOfInterest)
        {
            _areaData = areaData;
            _pointsOfInterest = pointOfInterest;
            _background = DrawBackground(areaData, pointOfInterest);
        }

        private static Bitmap DrawBackground(AreaData areaData, IReadOnlyList<PointOfInterest> pointOfInterest)
        {
            var background = new Bitmap(areaData.CollisionGrid[0].Length, areaData.CollisionGrid.Length,
                PixelFormat.Format32bppArgb);
            using (var backgroundGraphics = Graphics.FromImage(background))
            {
                backgroundGraphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0,
                    areaData.CollisionGrid[0].Length,
                    areaData.CollisionGrid.Length);
                backgroundGraphics.CompositingQuality = CompositingQuality.HighQuality;
                backgroundGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                backgroundGraphics.SmoothingMode = SmoothingMode.HighQuality;
                backgroundGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                for (var y = 0; y < areaData.CollisionGrid.Length; y++)
                {
                    for (var x = 0; x < areaData.CollisionGrid[y].Length; x++)
                    {
                        var type = areaData.CollisionGrid[y][x];
                        switch (type)
                        {
                            case 1:
                                //background.SetPixel(x, y, Color.FromArgb(70, 51, 41));
                                break;
                            case -1:
                                // uncroppedBackground.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                                break;
                            case 0:
                                background.SetPixel(x, y, Color.FromArgb(50, 50, 50));
                                break;
                            case 16:
                                background.SetPixel(x, y, Color.FromArgb(168, 56, 50));
                                break;
                            case 7:
                                background.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                                break;
                            case 5:
                                // uncroppedBackground.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                                break;
                            case 33:
                                background.SetPixel(x, y, Color.FromArgb(0, 0, 255));
                                break;
                            case 23:
                                background.SetPixel(x, y, Color.FromArgb(0, 0, 255));
                                break;
                            case 4:
                                background.SetPixel(x, y, Color.FromArgb(0, 255, 255));
                                break;
                            case 21:
                                background.SetPixel(x, y, Color.FromArgb(255, 0, 255));
                                break;
                            case 20:
                                background.SetPixel(x, y, Color.FromArgb(70, 51, 41));
                                break;
                            case 17:
                                background.SetPixel(x, y, Color.FromArgb(255, 51, 255));
                                break;
                            case 3:
                                background.SetPixel(x, y, Color.FromArgb(255, 0, 255));
                                break;
                            case 19:
                                background.SetPixel(x, y, Color.FromArgb(0, 51, 255));
                                break;
                            case 2:
                                background.SetPixel(x, y, Color.FromArgb(10, 51, 23));
                                break;
                            case 37:
                                background.SetPixel(x, y, Color.FromArgb(50, 51, 23));
                                break;
                            case 6:
                                background.SetPixel(x, y, Color.FromArgb(80, 51, 33));
                                break;
                            case 39:
                                background.SetPixel(x, y, Color.FromArgb(20, 11, 33));
                                break;
                            case 53:
                                background.SetPixel(x, y, Color.FromArgb(10, 11, 43));
                                break;
                            default:
                                background.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                                break;
                        }
                    }
                }

                foreach (var poi in pointOfInterest)
                {
                    if (poi.DrawIcon != null && poi.DrawIcon.Size != Size.Empty)
                    {
                        backgroundGraphics.DrawImage(poi.DrawIcon, poi.Position.OffsetFrom(areaData.Origin));
                    }

                    if (poi.DrawLabel && !string.IsNullOrWhiteSpace(poi.Label))
                    {
                        backgroundGraphics.DrawString(poi.Label, Settings.Map.Font,
                            new SolidBrush(Settings.Map.Colors.TextColor), poi.Position.OffsetFrom(areaData.Origin));
                    }
                }

                return background;
            }
        }

        public Bitmap Compose(GameData gameData)
        {
            if (gameData.Area != _areaData.Area)
            {
                throw new ApplicationException("Asked to compose an image for a different area." +
                                               $"Compositor area: {_areaData.Area}, Game data: {gameData.Area}");
            }

            var image = (Bitmap)_background.Clone();
            using (var imageGraphics = Graphics.FromImage(image))
            {
                imageGraphics.CompositingQuality = CompositingQuality.HighQuality;
                imageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                imageGraphics.SmoothingMode = SmoothingMode.HighQuality;
                imageGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var localPlayerPosition = gameData.PlayerPosition.OffsetFrom(_areaData.Origin);

                imageGraphics.DrawImage(Icons.Player, localPlayerPosition);

                // The lines are dynamic, and follow the player, so have to be drawn here.
                // The rest can be done in DrawBackground.
                foreach (var poi in _pointsOfInterest)
                {
                    if (poi.DrawLine)
                    {
                        imageGraphics.DrawLine(new Pen(Settings.Map.Colors.POILine), localPlayerPosition,
                            poi.Position.OffsetFrom(_areaData.Origin));
                    }
                }
            }

            image = ImageUtils.CropBitmap(image);

            double biggestDimension = Math.Max(image.Width, image.Height);

            var multiplier = Settings.Map.Size / biggestDimension;

            if (multiplier == 0)
            {
                multiplier = 1;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (multiplier != 1)
            {
                image = ImageUtils.ResizeImage(image, (int)(image.Width * multiplier),
                    (int)(image.Height * multiplier));
            }


            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Settings.Map.Rotate)
            {
                image = ImageUtils.RotateImage(image, 53, true, false, Color.Transparent);
            }

            return image;
        }
    }
}
