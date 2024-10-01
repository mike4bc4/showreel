using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: InternalsVisibleToAttribute("Localization.Editor")]

namespace Localization
{
    [CreateAssetMenu(fileName = "Table ", menuName = "Scriptable Objects/Localization/Table")]
    class Table : ScriptableObject
    {
        [Serializable]
        class Entry
        {
            [SerializeField] public string key;
            [SerializeField] public List<string> translations;

            public string this[int localeIndex]
            {
                get
                {
                    if (0 <= localeIndex && localeIndex < translations.Count)
                    {
                        return translations[localeIndex];
                    }

                    return null;
                }
            }
        }

        [SerializeField] List<Entry> m_Entries;
        [SerializeField] TextAsset m_CsvFile;
        Dictionary<string, Entry> m_EntryLookup;

        public TextAsset csvFile
        {
            get => m_CsvFile;
        }

        public string GetTranslation(string key, int localeIndex)
        {
            var entry = GetEntry(key);
            if (entry != null)
            {
                return entry[localeIndex];
            }

            return null;
        }

        Entry GetEntry(string key)
        {
            if (m_EntryLookup == null)
            {
                RebuildLookup();
            }

            if (m_EntryLookup.TryGetValue(key.ToLowerInvariant(), out var entry))
            {
                return entry;
            }

            return null;
        }

        public void ReadCsv()
        {
            if (m_CsvFile == null)
            {
                return;
            }

            var lines = m_CsvFile.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex("(?:,|\\n|^)(\"(?:(?:\"\")*[^\"]*)*\"|[^\",\\n]*|(?:\\n|$))");

            foreach (var line in lines)
            {
                var matches = regex.Matches(line);
                if (matches.Count == 0)
                {
                    continue;
                }

                var key = matches[0].Groups[1].Value;
                var translations = new List<string>();
                for (int i = 1; i < matches.Count; i++)
                {
                    translations.Add(matches[i].Groups[1].Value.Trim('"'));
                }

                var entry = GetEntry(key);
                if (entry != null)
                {
                    entry.translations = translations;
                }
                else
                {
                    entry = new Entry()
                    {
                        key = key,
                        translations = translations,
                    };

                    m_Entries.Add(entry);
                }
            }

            RebuildLookup();
        }

        void RebuildLookup()
        {
            m_EntryLookup = new Dictionary<string, Entry>();
            foreach (var entry in m_Entries)
            {
                m_EntryLookup[entry.key.ToLowerInvariant()] = entry;
            }
        }
    }
}
