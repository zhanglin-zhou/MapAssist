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
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using D2RAssist.Types;
using D2RAssist.Helpers;

namespace D2RAssist
{
    public partial class frmOverlay : Form
    {
        // Move to windows external
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private Screen _screen;
        private IntPtr _d2RWindowPointer;

        public frmOverlay()
        {
            InitializeComponent();
        }

        private void frmOverlay_Load(object sender, EventArgs e)
        {
            // We should select screen either from config or find the actual position of the D2R window/screen.
            var d2rProcess = Process.GetProcessesByName("D2R")[0];
            _d2RWindowPointer = d2rProcess.MainWindowHandle;
            UpdateLocation();

            this.Opacity = Settings.Map.Opacity;

            Timer MapUpdateTimer = new Timer();
            MapUpdateTimer.Interval = Settings.Map.UpdateTime;
            MapUpdateTimer.Tick += new EventHandler(MapUpdateTimer_Tick);
            MapUpdateTimer.Start();

            if (Settings.Map.AlwaysOnTop)
            {
                uint initialStyle = (uint)WindowsExternal.GetWindowLongPtr(this.Handle, -20);
                WindowsExternal.SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
                WindowsExternal.SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }

            mapOverlay.Location = new Point(0, 0);
            mapOverlay.Width = this.Width;
            mapOverlay.Height = this.Height;
            mapOverlay.BackColor = Color.Transparent;
        }

        private async void MapUpdateTimer_Tick(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();

            Globals.CurrentGameData = GameMemory.GetGameData();

            if (Globals.CurrentGameData != null)
            {
                bool shouldUpdateMap = (Globals.CurrentGameData.MapSeed != 0 && Globals.MapData == null) ||
                    (Globals.CurrentGameData.AreaId != Globals.LastGameData?.AreaId &&
                    Globals.CurrentGameData.AreaId != 0 ||
                    Globals.CurrentGameData.Difficulty != Globals.LastGameData?.Difficulty);

                if (Globals.LastGameData?.MapSeed != Globals.CurrentGameData.MapSeed && Globals.CurrentGameData.MapSeed != 0)
                {
                    await MapApi.CreateNewSession();            
                }

                if (shouldUpdateMap)
                {
                    Globals.MapData = await MapApi.GetMapData(Globals.CurrentGameData.AreaId);
                }

                Globals.LastGameData = Globals.CurrentGameData;

                if (ShouldHideMap())
                {
                    mapOverlay.Hide();
                }
                else
                {
                    mapOverlay.Show();
                    mapOverlay.Refresh();
                }

            }
            timer.Start();
        }

        private bool ShouldHideMap()
        {
            return Globals.CurrentGameData.MapSeed == 0 || 
                 !D2RProcessInForeground() ||
                (Settings.Map.HideInTown == true &&
                Globals.CurrentGameData.AreaId.IsTown());
        }

        private bool D2RProcessInForeground()
        {
            IntPtr activeWindowHandle = WindowsExternal.GetForegroundWindow();
            return activeWindowHandle == _d2RWindowPointer;
        }

        private void mapOverlay_Paint(object sender, PaintEventArgs e)
        {
            if (Globals.MapData == null)
            {
                return;
            }
            
            UpdateLocation();

            Bitmap gameMap = MapRenderer.FromMapData(Globals.MapData);
            Point anchor = new Point(0, 0);
            switch (Settings.Map.Position) {
                case MapPosition.TopRight:
                    anchor = new Point(_screen.WorkingArea.Width - gameMap.Width, 0);

                    break;
                case MapPosition.TopLeft:
                    anchor = new Point(0, 0);
                    break;
            }
            e.Graphics.DrawImage(gameMap, anchor);
        }

        /// <summary>
        /// Update the location and size of the form relative to the D2R window location.
        /// </summary>
        private void UpdateLocation()
        {
            this.WindowState = FormWindowState.Normal;
            _screen = Screen.FromHandle(_d2RWindowPointer);
            this.Location = new Point(_screen.WorkingArea.X, _screen.WorkingArea.Y);
            this.Size = new Size(_screen.WorkingArea.Width, _screen.WorkingArea.Height);
        }
    }
}
