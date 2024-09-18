using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Utility
{
    public enum WindowMode
    {
        Fullscreen = 0,
        Windowed = 1,
    }

    public enum Quality
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    public sealed class SettingsManager
    {
        public static event Action OnSettingsApplied;

        const string k_WindowModeKey = "WindowMode";
        const string k_ResolutionKey = "Resolution";
        const string k_RefreshRateKey = "RefreshRate";
        const string k_VerticalSyncKey = "VerticalSync";
        const string k_BlurQualityKey = "BlurQuality";
        const string k_ShowWelcomeWindowKey = "ShowWelcomeWindow";
        const string k_SavePath = "Settings";
        static SettingsManager s_Instance;


        Setting<WindowMode> m_WindowMode;
        Setting<Vector2Int> m_Resolution;
        Setting<float> m_RefreshRate;
        Setting<bool> m_VerticalSync;
        Setting<Quality, float> m_BlurQuality;
        Setting<bool> m_ShowWelcomeWindow;
        SaveObject m_SaveObject;

        public static SettingsManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new SettingsManager();
                    s_Instance.Init();
                }

                return s_Instance;
            }
        }

        public static Setting<WindowMode> WindowMode => Instance.m_WindowMode;
        public static Setting<Vector2Int> Resolution => Instance.m_Resolution;
        public static Setting<float> RefreshRate => Instance.m_RefreshRate;
        public static Setting<bool> VerticalSync => Instance.m_VerticalSync;
        public static Setting<Quality, float> BlurQuality => Instance.m_BlurQuality;
        public static Setting<bool> ShowWelcomeWindow => Instance.m_ShowWelcomeWindow;

        static SaveObject saveObject
        {
            get => Instance.m_SaveObject;
            set => Instance.m_SaveObject = value;
        }

        static List<Setting> settings
        {
            get => new List<Setting>()
            {
                WindowMode,
                Resolution,
                RefreshRate,
                VerticalSync,
                BlurQuality,
                ShowWelcomeWindow,
            };
        }

        SettingsManager()
        {
            m_WindowMode = new Setting<WindowMode>(SettingsManagerResources.Instance.windowModeOptions);
            m_Resolution = new Setting<Vector2Int>(() => SettingsManagerResources.Instance.resolutionOptions);
            m_Resolution.onChanged += OnResolutionChanged;

            m_RefreshRate = new Setting<float>(() => SettingsManagerResources.Instance.GetRefreshRateOptions(m_Resolution.option.value));
            m_VerticalSync = new Setting<bool>(SettingsManagerResources.Instance.verticalSyncOptions);
            m_BlurQuality = new Setting<Quality, float>(SettingsManagerResources.Instance.blurQualityOptions);
            m_ShowWelcomeWindow = new Setting<bool>(SettingsManagerResources.Instance.showWelcomeWindowOptions);
        }

        void Init()
        {
            Read();
            if (m_SaveObject == null)
            {
                Write();
            }
        }

        void OnResolutionChanged()
        {
            var nearestRefreshRate = m_RefreshRate.options[0].value;
            for (int i = 1; i < m_RefreshRate.options.Count; i++)
            {
                var refreshRate = m_RefreshRate.options[i].value;
                if (Mathf.Abs(m_RefreshRate.option.value - refreshRate) < Mathf.Abs(nearestRefreshRate - refreshRate))
                {
                    nearestRefreshRate = refreshRate;
                }
            }

            m_RefreshRate.SetOption(nearestRefreshRate);
        }

        public static void Reset()
        {
            foreach (var setting in settings)
            {
                setting.Reset();
            }
        }

        public static void Apply()
        {
            FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
            switch (WindowMode.option.value)
            {
                case Utility.WindowMode.Fullscreen:
                    fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case Utility.WindowMode.Windowed:
                    fullScreenMode = FullScreenMode.Windowed;
                    break;
            }

            Screen.SetResolution(Resolution.option.value.x, Resolution.option.value.y, fullScreenMode);
            Application.targetFrameRate = Mathf.RoundToInt(RefreshRate.option.value);

            QualitySettings.vSyncCount = VerticalSync.option.value ? 1 : 0;
            OnSettingsApplied?.Invoke();
        }

        public static void Restore()
        {
            if (saveObject == null)
            {
                return;
            }

            WindowMode.SetOption(saveObject.TryGetValue(k_WindowModeKey, out WindowMode windowMode) ? windowMode : WindowMode.options.defaultOption.value);
            Resolution.SetOption(saveObject.TryGetValue(k_ResolutionKey, out Vector2Int resolution) ? resolution : Resolution.options.defaultOption.value);
            RefreshRate.SetOption(saveObject.TryGetValue(k_RefreshRateKey, out float refreshRate) ? refreshRate : RefreshRate.options.defaultOption.value);
            VerticalSync.SetOption(saveObject.TryGetValue(k_VerticalSyncKey, out bool verticalSync) ? verticalSync : VerticalSync.options.defaultOption.value);
            BlurQuality.SetOption(saveObject.TryGetValue(k_BlurQualityKey, out Quality blurQuality) ? blurQuality : BlurQuality.options.defaultOption.primaryValue);
            ShowWelcomeWindow.SetOption(saveObject.TryGetValue(k_ShowWelcomeWindowKey, out bool showWelcomeWindow) ? showWelcomeWindow : ShowWelcomeWindow.options.defaultOption.value);
        }

        public static void Write()
        {
            saveObject = new SaveObject();
            saveObject.SetValue(k_WindowModeKey, WindowMode.option.value);
            saveObject.SetValue(k_ResolutionKey, Resolution.option.value);
            saveObject.SetValue(k_RefreshRateKey, RefreshRate.option.value);
            saveObject.SetValue(k_VerticalSyncKey, VerticalSync.option.value);
            saveObject.SetValue(k_BlurQualityKey, BlurQuality.option.primaryValue);
            saveObject.SetValue(k_ShowWelcomeWindowKey, ShowWelcomeWindow.option.value);
            saveObject.Write(k_SavePath);
        }

        public static void Read()
        {
            saveObject = SaveObject.Read(k_SavePath);
            Restore();
        }
    }
}
