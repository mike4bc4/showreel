using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    public static class SettingsExtensions
    {
        public static FullScreenMode ToFullScreenMode(this WindowMode windowMode)
        {
            if (windowMode == WindowMode.Fullscreen)
            {
                return FullScreenMode.FullScreenWindow;
            }
            else
            {
                return FullScreenMode.Windowed;
            }
        }

        public static WindowMode ToWindowMode(this FullScreenMode fullScreenMode)
        {
            if (fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                return WindowMode.Fullscreen;
            }
            else
            {
                return WindowMode.Windowed;
            }
        }

        public static Vector2Int GetSize(this Resolution resolution)
        {
            return new Vector2Int(resolution.width, resolution.height);
        }
    }
}
