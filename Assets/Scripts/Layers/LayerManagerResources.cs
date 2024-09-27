using System;
using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Layers
{
    [CreateAssetMenu(fileName = "LayerManagerResources ", menuName = "Scriptable Objects/Layer Manager Resources")]
    public class LayerManagerResources : ScriptableSingleton<LayerManagerResources>
    {
        [Serializable]
        struct ThemeEntry
        {
            [SerializeField] Theme m_Theme;
            [SerializeField] ThemeStyleSheet m_ThemeStyleSheet;

            public Theme theme => m_Theme;
            public ThemeStyleSheet themeStyleSheet => m_ThemeStyleSheet;
        }

        [SerializeField] List<ThemeEntry> m_Themes;

        public static ThemeStyleSheet GetThemeStyleSheet(Theme theme)
        {
            foreach (var entry in Instance.m_Themes)
            {
                if (entry.theme == theme)
                {
                    return entry.themeStyleSheet;
                }
            }

            return null;
        }

        public static bool TryGetLinkedTheme(ThemeStyleSheet themeStyleSheet, out Theme theme)
        {
            foreach (var entry in Instance.m_Themes)
            {
                if (entry.themeStyleSheet == themeStyleSheet)
                {
                    theme = entry.theme;
                    return true;
                }
            }

            theme = default;
            return false;
        }
    }
}
