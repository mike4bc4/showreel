using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Utility;

namespace Settings
{
    public enum WindowMode
    {
        Fullscreen = 0,
        Windowed = 1,
    }

    public enum Theme
    {
        Light = 0,
        Dark = 1,
    }

    public sealed class SettingsManager : MonoBehaviour
    {
        public static event Action OnSettingsApplied;

        const string k_WindowModeKey = "WindowMode";
        const string k_ResolutionKey = "Resolution";
        const string k_RefreshRateKey = "RefreshRate";
        const string k_VerticalSyncKey = "VerticalSync";
        const string k_BlurQualityKey = "BlurQuality";
        const string k_ShowWelcomeWindowKey = "ShowWelcomeWindow";
        const string k_ThemeKey = "Theme";
        const string k_LocaleKey = "Locale";
        const string k_SavePath = "Settings";
        static SettingsManager s_Instance;

        Setting<WindowMode> m_WindowMode;
        Setting<Vector2Int> m_Resolution;
        Setting<float> m_RefreshRate;
        Setting<bool> m_VerticalSync;
        Setting<float> m_BlurQuality;
        Setting<bool> m_ShowWelcomeWindow;
        Setting<Theme> m_Theme;
        Setting<string> m_Locale;
        SaveObject m_SaveObject;

        public static SettingsManager Instance
        {
            get => s_Instance;
        }

        public static Setting<WindowMode> WindowMode => s_Instance.m_WindowMode;
        public static Setting<Vector2Int> Resolution => s_Instance.m_Resolution;
        public static Setting<float> RefreshRate => s_Instance.m_RefreshRate;
        public static Setting<bool> VerticalSync => s_Instance.m_VerticalSync;
        public static Setting<float> BlurQuality => s_Instance.m_BlurQuality;
        public static Setting<bool> ShowWelcomeWindow => s_Instance.m_ShowWelcomeWindow;
        public static Setting<Theme> Theme => s_Instance.m_Theme;
        public static Setting<string> Locale => s_Instance.m_Locale;

        static SaveObject saveObject
        {
            get => s_Instance.m_SaveObject;
            set => s_Instance.m_SaveObject = value;
        }


        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);

            m_WindowMode = new Setting<WindowMode>(SettingsManagerResources.Instance.windowModeOptions);
            m_Resolution = new Setting<Vector2Int>(() => SettingsManagerResources.Instance.resolutionOptions);
            m_Resolution.onChanged += OnResolutionChanged;

            m_RefreshRate = new Setting<float>(() => SettingsManagerResources.Instance.GetRefreshRateOptions(m_Resolution.value));
            m_VerticalSync = new Setting<bool>(SettingsManagerResources.Instance.verticalSyncOptions);
            m_BlurQuality = new Setting<float>(SettingsManagerResources.Instance.blurQualityOptions);
            m_ShowWelcomeWindow = new Setting<bool>(SettingsManagerResources.Instance.showWelcomeWindowOptions);
            m_Theme = new Setting<Theme>(SettingsManagerResources.Instance.themeOptions);
            m_Locale = new Setting<string>(SettingsManagerResources.Instance.localeOptions);

            Read();
            Apply();
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
                if (Mathf.Abs(m_RefreshRate.value - refreshRate) < Mathf.Abs(nearestRefreshRate - refreshRate))
                {
                    nearestRefreshRate = refreshRate;
                }
            }

            m_RefreshRate.SetValue(nearestRefreshRate);
        }

        public static void Apply()
        {
            Screen.SetResolution(Resolution.value.x, Resolution.value.y, WindowMode.value.ToFullScreenMode());
            Application.targetFrameRate = Mathf.RoundToInt(RefreshRate.value);

            QualitySettings.vSyncCount = VerticalSync.value ? 1 : 0;
            OnSettingsApplied?.Invoke();
        }

        public static void Restore()
        {
            if (saveObject == null)
            {
                return;
            }

            WindowMode.SetValue(saveObject.TryGetValue(k_WindowModeKey, out WindowMode windowMode) ? windowMode : WindowMode.defaultOption.value);
            Resolution.SetValue(saveObject.TryGetValue(k_ResolutionKey, out Vector2Int resolution) ? resolution : Resolution.defaultOption.value);
            RefreshRate.SetValue(saveObject.TryGetValue(k_RefreshRateKey, out float refreshRate) ? refreshRate : RefreshRate.defaultOption.value);
            VerticalSync.SetValue(saveObject.TryGetValue(k_VerticalSyncKey, out bool verticalSync) ? verticalSync : VerticalSync.defaultOption.value);
            BlurQuality.SetValue(saveObject.TryGetValue(k_BlurQualityKey, out float blurQuality) ? blurQuality : BlurQuality.defaultOption.value);
            ShowWelcomeWindow.SetValue(saveObject.TryGetValue(k_ShowWelcomeWindowKey, out bool showWelcomeWindow) ? showWelcomeWindow : ShowWelcomeWindow.defaultOption.value);
            Theme.SetValue(saveObject.TryGetValue(k_ThemeKey, out Theme theme) ? theme : Theme.defaultOption.value);
            Locale.SetValue(saveObject.TryGetValue(k_LocaleKey, out string locale) ? locale : Locale.defaultOption.value);
        }

        public static void Write()
        {
            saveObject = new SaveObject();
            saveObject.SetValue(k_WindowModeKey, WindowMode.value);
            saveObject.SetValue(k_ResolutionKey, Resolution.value);
            saveObject.SetValue(k_RefreshRateKey, RefreshRate.value);
            saveObject.SetValue(k_VerticalSyncKey, VerticalSync.value);
            saveObject.SetValue(k_BlurQualityKey, BlurQuality.value);
            saveObject.SetValue(k_ShowWelcomeWindowKey, ShowWelcomeWindow.value);
            saveObject.SetValue(k_ThemeKey, Theme.value);
            saveObject.SetValue(k_LocaleKey, Locale.value);
            saveObject.Write(k_SavePath);
        }

        public static void Read()
        {
            saveObject = SaveObject.Read(k_SavePath);
            Restore();
        }
    }
}
