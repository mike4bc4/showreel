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
        struct DataWrapper<T>
        {
            public T value;

            public DataWrapper(T value)
            {
                this.value = value;
            }
        }

        [Serializable]
        public struct DataEntry
        {
            public string key;
            public string value;
        }

        [SerializeField] List<DataEntry> m_Data;

        public SaveObject()
        {
            m_Data = new List<DataEntry>();
        }

        public void SetValue<T>(string key, T value)
        {
            var keyInvariant = key.ToLowerInvariant();
            for (int i = 0; i < m_Data.Count; i++)
            {
                DataEntry entry = m_Data[i];
                if (entry.key == keyInvariant)
                {
                    entry.value = JsonUtility.ToJson(new DataWrapper<T>(value));
                    return;
                }
            }

            m_Data.Add(new DataEntry()
            {
                key = keyInvariant,
                value = JsonUtility.ToJson(new DataWrapper<T>(value)),
            });
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var keyInvariant = key.ToLowerInvariant();
            foreach (var entry in m_Data)
            {
                if (entry.key == keyInvariant)
                {
                    try
                    {
                        value = JsonUtility.FromJson<DataWrapper<T>>(entry.value).value;
                        return true;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            value = default;
            return false;
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
