using System.Drawing;
using D2RAssist.Helpers;

namespace D2RAssist.Types
{
    public static class Icons
    {
        public static readonly Bitmap DoorNext = ImageUtils.CreateFilledRectangle(Settings.Map.Colors.DoorNext, 10, 10);

        public static readonly Bitmap DoorPrevious =
            ImageUtils.CreateFilledRectangle(Settings.Map.Colors.DoorPrevious, 10, 10);

        public static readonly Bitmap Waypoint = ImageUtils.CreateFilledRectangle(Settings.Map.Colors.Waypoint, 10, 10);
        public static readonly Bitmap Player = ImageUtils.CreateFilledRectangle(Settings.Map.Colors.Player, 5, 5);

        public static readonly Bitmap SuperChest =
            ImageUtils.CreateFilledEllipse(Settings.Map.Colors.SuperChest, 10, 10);
    }
}
