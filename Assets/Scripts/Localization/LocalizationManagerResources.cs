using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationManagerResources ", menuName = "Scriptable Objects/Localization/Resources")]
    class LocalizationManagerResources : ScriptableSingleton<LocalizationManagerResources>
    {
        [SerializeField] List<string> m_Locales;
        [SerializeField] List<Table> m_Tables;

        public IReadOnlyList<string> locales
        {
            get => m_Locales.AsReadOnly();
        }

        public IReadOnlyList<Table> tables
        {
            get => m_Tables.AsReadOnly();
        }

        public static Table GetTable(string name)
        {
            foreach (var table in Instance.m_Tables)
            {
                if (table.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return table;
                }
            }

            return null;
        }

        public static int GetLocaleIndex(string locale)
        {
            for (int i = 0; i < Instance.m_Locales.Count; i++)
            {
                string loc = Instance.m_Locales[i];
                if (loc.Equals(locale, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string GetLocaleName(int localeIndex)
        {
            if (0 <= localeIndex && localeIndex < Instance.m_Locales.Count)
            {
                return Instance.m_Locales[localeIndex];
            }

            return null;
        }
    }
}
