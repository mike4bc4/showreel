using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Extensions;
using Settings;

namespace Localization
{
    public class LocalizationManager
    {
        static LocalizationManager s_Instance;

        public static LocalizationManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new LocalizationManager();
                }

                return s_Instance;
            }
        }

        List<ILocalizedElement> m_LocalizedElements;
        string m_SelectedLocale;

        public static string SelectedLocale
        {
            get => Instance.m_SelectedLocale;
            set => Instance.m_SelectedLocale = value;
        }

        static List<ILocalizedElement> localizedElements => Instance.m_LocalizedElements;

        LocalizationManager()
        {
            m_LocalizedElements = new List<ILocalizedElement>();
        }

        // We are registering callback even before SettingsManager awake is called, this way we can
        // be sure that OnSettingsApplied will be called when settings are loaded.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            SettingsManager.OnSettingsApplied += OnSettingsApplied;
        }

        static void OnSettingsApplied()
        {
            if (SelectedLocale != SettingsManager.Locale.value)
            {
                SelectedLocale = SettingsManager.Locale.value;
                Localize();
            }
        }

        internal static void RegisterElement(ILocalizedElement textElement)
        {
            if (!localizedElements.Contains(textElement))
            {
                localizedElements.Add(textElement);
            }
        }

        public static void Localize(ILocalizedElement localizedElement, string locale = null)
        {
            LocalizeInternal(new List<ILocalizedElement>() { localizedElement }, locale);
        }

        public static void LocalizeVisualTree(VisualElement root, string locale = null)
        {
            LocalizeInternal(root.GetDescendants<ILocalizedElement>(), locale);
        }

        public static void Localize(string locale = null)
        {
            LocalizeInternal(localizedElements, locale);
        }

        static void LocalizeInternal(List<ILocalizedElement> localizedElements, string locale = null)
        {
            locale ??= SelectedLocale;
            if (locale == null)
            {
                Debug.LogWarning($"Default locale not specified.");
                return;
            }

            var localeIndex = LocalizationManagerResources.GetLocaleIndex(locale);
            if (localeIndex < 0)
            {
                Debug.LogWarning($"Cannot find '{locale}' locale.");
                return;
            }

            foreach (var element in localizedElements)
            {
                if (element.localizationAddress.isEmpty)
                {
                    continue;
                }

                var table = LocalizationManagerResources.GetTable(element.localizationAddress.table);
                if (table != null)
                {
                    var translation = table.GetTranslation(element.localizationAddress.key, localeIndex);
                    if (translation != null)
                    {
                        element.text = translation;
                    }
                    else
                    {
                        element.text = $"Cannot find translation for '{element.localizationAddress.key}' in '{element.localizationAddress.table}' for '{locale}' locale.";
                    }
                }
                else
                {
                    element.text = $"Cannot find '{element.localizationAddress.table}' table.";
                }
            }
        }
    }
}
