using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.IO;

namespace Editor
{
    [InitializeOnLoad]
    public class Refresher
    {
        const string k_SettingsDirectoryPath = "Assets/Refresher";
        const string k_SettingsPath = k_SettingsDirectoryPath + "/Settings.asset";

        static Refresher s_Instance;

        Timer m_Timer;
        bool m_Enabled;
        RefresherSettings m_Settings;

        public static float Interval
        {
            get => s_Instance.m_Timer.interval;
            set => s_Instance.m_Timer.interval = value;
        }

        public static bool Enabled
        {
            get => s_Instance.enabled;
            set => s_Instance.enabled = value;
        }

        bool enabled
        {
            get => m_Enabled;
            set
            {
                if (value != m_Enabled)
                {
                    m_Enabled = value;
                    if (m_Enabled)
                    {
                        m_Timer.onTimeout += Refresh;
                    }
                    else
                    {
                        m_Timer.onTimeout -= Refresh;
                    }
                }
            }
        }

        RefresherSettings settings
        {
            get
            {
                if (m_Settings == null)
                {
                    if (!Directory.Exists(k_SettingsDirectoryPath))
                    {
                        Directory.CreateDirectory(k_SettingsDirectoryPath);
                    }

                    m_Settings = AssetDatabase.LoadAssetAtPath<RefresherSettings>(k_SettingsPath);
                    if (m_Settings == null)
                    {
                        m_Settings = RefresherSettings.CreateInstance();
                        AssetDatabase.CreateAsset(m_Settings, k_SettingsPath);
                    }
                }

                return m_Settings;
            }
        }

        static Refresher()
        {
            if (s_Instance == null)
            {
                s_Instance = new Refresher();
            }
        }

        Refresher()
        {
            m_Timer = new Timer(settings.interval) { repeat = true };
            enabled = settings.enabled;
        }

        void Refresh()
        {
            if (Application.isPlaying)
            {
                return;
            }

            // Although finding all ui documents every refresh might seem expensive, it's rather insignificant in comparison to
            // re-enabling each component, and allows to avoid brittle component tracking.
            var uiDocuments = GameObject.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            foreach (var uiDocument in uiDocuments)
            {
                bool enabled = uiDocument.enabled;
                uiDocument.enabled = !enabled;
                uiDocument.enabled = enabled;
            }
        }
    }
}
