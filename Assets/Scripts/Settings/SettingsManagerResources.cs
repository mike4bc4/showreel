using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace Settings
{
    [CreateAssetMenu(fileName = "SettingsManagerResources ", menuName = "Scriptable Objects/Settings Manager Resources")]
    public class SettingsManagerResources : ScriptableSingleton<SettingsManagerResources>
    {
        [SerializeField] OptionSet<WindowMode> m_WindowModeOptions;
        [SerializeField] OptionSet<bool> m_VerticalSyncOptions;
        [SerializeField] OptionSet<float> m_BlurQualityOptions;
        [SerializeField] OptionSet<bool> m_ShowWelcomeWindowOptions;
        [SerializeField] OptionSet<Theme> m_ThemeOptions;
        [SerializeField] OptionSet<string> m_LocaleOptions;

        public OptionSet<WindowMode> windowModeOptions => m_WindowModeOptions;
        public OptionSet<bool> verticalSyncOptions => m_VerticalSyncOptions;
        public OptionSet<float> blurQualityOptions => m_BlurQualityOptions;
        public OptionSet<bool> showWelcomeWindowOptions => m_ShowWelcomeWindowOptions;
        public OptionSet<Theme> themeOptions => m_ThemeOptions;
        public OptionSet<string> localeOptions => m_LocaleOptions;

        public OptionSet<Vector2Int> resolutionOptions
        {
            get
            {
                var options = new List<Option<Vector2Int>>();
                foreach (var resolution in Screen.resolutions)
                {
                    options.Add(new Option<Vector2Int>(resolution.width + "x" + resolution.height, resolution.GetSize()));
                }

                options = options.Distinct().ToList();
                var defaultResolution = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
                var defaultOptionIndex = 0;
                for (int i = 1; i < options.Count; i++)
                {
                    var opt = options[i];
                    if (Vector2Int.Distance(opt.value, defaultResolution) < Vector2Int.Distance(options[defaultOptionIndex].value, defaultResolution))
                    {
                        defaultOptionIndex = i;
                    }
                }

                return new OptionSet<Vector2Int>(defaultOptionIndex, options);
            }
        }

        public OptionSet<float> GetRefreshRateOptions(Vector2Int resolution)
        {
            var options = new List<Option<float>>();
            foreach (var res in Screen.resolutions)
            {
                if (res.width == resolution.x && res.height == resolution.y)
                {
                    options.Add(new Option<float>(string.Format("{0:0.##}Hz", res.refreshRateRatio.value), (float)res.refreshRateRatio.value));
                }
            }

            options.Add(new Option<float>("Table:Unlimited", -1f));
            options = options.Distinct().ToList();

            return new OptionSet<float>(options.Count - 1, options);
        }
    }
}
