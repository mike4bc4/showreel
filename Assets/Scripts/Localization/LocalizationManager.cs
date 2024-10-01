using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
    [InitializeOnLoad]
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

        static List<ILocalizedElement> localizedElements => Instance.m_LocalizedElements;

        LocalizationManager()
        {
            m_LocalizedElements = new List<ILocalizedElement>();
        }

        internal static void RegisterElement(ILocalizedElement textElement)
        {
            if (!localizedElements.Contains(textElement))
            {
                localizedElements.Add(textElement);
            }
        }

        public static void Localize(ILocalizedElement localizedElement, string locale)
        {
            LocalizeInternal(new List<ILocalizedElement>() { localizedElement }, locale);
        }

        public static void LocalizeVisualTree(VisualElement root, string locale)
        {
            var labels = root.Query<LocalizedLabel>().ToList().Cast<ILocalizedElement>();
            LocalizeInternal(localizedElements, locale);
        }

        public static void Localize(string locale)
        {
            LocalizeInternal(localizedElements, locale);
        }

        static void LocalizeInternal(List<ILocalizedElement> localizedElements, string locale)
        {
            var localeIndex = LocalizationManagerResources.GetLocaleIndex(locale);
            if (localeIndex < 0)
            {
                Debug.LogWarning($"Cannot find '{locale}' locale.");
                return;
            }

            foreach (var element in localizedElements)
            {
                var table = LocalizationManagerResources.GetTable(element.table);
                if (table != null)
                {
                    var translation = table.GetTranslation(element.key, localeIndex);
                    if (translation != null)
                    {
                        element.text = translation;
                    }
                    else
                    {
                        element.text = $"Cannot find translation for '{element.key}' in '{element.table}' for '{locale}' locale.";
                    }
                }
                else
                {
                    element.text = $"Cannot find '{element.table}' table.";
                }
            }
        }
    }
}
