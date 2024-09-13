using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utility
{
    [Serializable]
    public class SaveObject
    {
        [Serializable]
        public struct DataEntry
        {
            public string key;
            public string value;
            public string type;
        }

        public List<DataEntry> m_Data;

        public object this[string key]
        {
            get
            {
                string keyInvariant = key.ToLowerInvariant();
                foreach (var entry in m_Data)
                {
                    if (entry.key == keyInvariant)
                    {
                        if (string.IsNullOrEmpty(entry.value) || string.IsNullOrEmpty(entry.type))
                        {
                            break;
                        }

                        try
                        {
                            return Convert.ChangeType(entry.value, Type.GetType(entry.type));
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning($"Failed to convert '{entry.value}' into '{entry.type}'.");
                            return entry.value;
                        }
                    }
                }

                return null;
            }
            set
            {
                string keyInvariant = key.ToLowerInvariant();
                for (int i = 0; i < m_Data.Count; i++)
                {
                    DataEntry entry = m_Data[i];
                    if (entry.key == keyInvariant)
                    {
                        entry.value = value?.ToString();
                        entry.type = value?.GetType().FullName;
                        return;
                    }
                }

                m_Data.Add(new DataEntry()
                {
                    key = keyInvariant,
                    value = value?.ToString(),
                    type = value?.GetType().FullName,
                });
            }
        }

        public SaveObject()
        {
            m_Data = new List<DataEntry>();
        }

        public void Write(string path)
        {
            string fullPath = Application.persistentDataPath + "/" + path + ".json";
            var fileInfo = new FileInfo(fullPath);
            fileInfo.Directory.Create();    // Does not create directory if it already exist.

            string data = JsonUtility.ToJson(this);
            File.WriteAllText(fullPath, data);
        }

        public static SaveObject Read(string path)
        {
            string fullPath = Application.persistentDataPath + "/" + path + ".json";
            if (!File.Exists(fullPath))
            {
                return null;
            }

            string data = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<SaveObject>(data);
        }
    }
}
