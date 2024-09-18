using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Utility
{
    [CreateAssetMenu(fileName = "SettingsManagerResources ", menuName = "Scriptable Objects/Settings Manager Resources")]
    public class SettingsManagerResources : ScriptableSingleton<SettingsManagerResources>
    {
        [SerializeField] OptionList<WindowMode> m_WindowModeOptions;
        [SerializeField] OptionList<bool> m_VerticalSyncOptions;
        [SerializeField] OptionList<Quality, float> m_BlurQualityOptions;
        [SerializeField] OptionList<bool> m_ShowWelcomeWindowOptions;

        public OptionList<WindowMode> windowModeOptions => m_WindowModeOptions;
        public OptionList<bool> verticalSyncOptions => m_VerticalSyncOptions;
        public OptionList<Quality, float> blurQualityOptions => m_BlurQualityOptions;
        public OptionList<bool> showWelcomeWindowOptions => m_ShowWelcomeWindowOptions;

        public OptionList<Vector2Int> resolutionOptions
        {
            get
            {
                var options = new List<Option<Vector2Int>>();
                foreach (var resolution in Screen.resolutions)
                {
                    options.Add(new Option<Vector2Int>(resolution.width + "x" + resolution.height, new Vector2Int(resolution.width, resolution.height)));
                }

                options = options.Distinct().ToList();
                var defaultResolution = new Vector2Int(Screen.width, Screen.height);
                var defaultOptionIndex = 0;
                for (int i = 1; i < options.Count; i++)
                {
                    var opt = options[i];
                    if (Vector2Int.Distance(opt.value, defaultResolution) < Vector2Int.Distance(options[defaultOptionIndex].value, defaultResolution))
                    {
                        defaultOptionIndex = i;
                    }
                }

                return new OptionList<Vector2Int>(options, defaultOptionIndex);
            }
        }

        public OptionList<float> GetRefreshRateOptions(Vector2Int resolution)
        {
            var options = new List<Option<float>>();
            foreach (var res in Screen.resolutions)
            {
                if (res.width == resolution.x && res.height == resolution.y)
                {
                    options.Add(new Option<float>(string.Format("{0:0.##}Hz", res.refreshRateRatio.value), (float)res.refreshRateRatio.value));
                }
            }

            options.Add(new Option<float>("Unlimited", -1f));
            options = options.Distinct().ToList();
            var defaultRefreshRate = 60f;
            var defaultOptionIndex = 0;
            for (int i = 1; i < options.Count; i++)
            {
                var opt = options[i];
                if (Mathf.Abs(opt.value - defaultRefreshRate) < Mathf.Abs(options[defaultOptionIndex].value - defaultRefreshRate))
                {
                    defaultOptionIndex = i;
                }
            }

            return new OptionList<float>(options, defaultOptionIndex);
        }
    }
}
