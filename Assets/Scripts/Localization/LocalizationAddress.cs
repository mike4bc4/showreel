using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Localization
{
    public struct LocalizationAddress
    {
        string m_Table;
        string m_Key;

        public string table
        {
            get => m_Table;
        }

        public string key
        {
            get => m_Key;
        }

        public bool isEmpty
        {
            get => m_Table == null && m_Key == null;
        }

        public LocalizationAddress(string address) : this()
        {
            if (address == null)
            {
                return;
            }

            var words = address.Split(":", System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 2)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].Trim();
                }

                m_Table = words[0];
                m_Key = words[1];
            }
        }

        public static LocalizationAddress FromString(string address)
        {
            var localizationAddress = new LocalizationAddress();
            if (address == null)
            {
                return localizationAddress;
            }

            var words = address.Split(":", System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 2)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].Trim();
                }

                localizationAddress.m_Table = words[1];
                localizationAddress.m_Key = words[1];
            }

            return localizationAddress;
        }

        public static bool IsAddress(string address)
        {
            var localizationAddress = FromString(address);
            return !localizationAddress.isEmpty;
        }

        public LocalizationAddress(string table, string key)
        {
            m_Table = table;
            m_Key = key;
        }

        public override string ToString()
        {
            if (isEmpty)
            {
                return "Null";
            }

            return (m_Table == null ? "Null" : m_Table) + ":" + (m_Key == null ? "Null" : m_Key);
        }

        public static implicit operator LocalizationAddress(string address) => new LocalizationAddress(address);
    }

    public struct LocalizationAddressCollection
    {
        List<LocalizationAddress> m_Addresses;

        public IReadOnlyCollection<LocalizationAddress> addresses
        {
            get
            {
                if (m_Addresses == null)
                {
                    m_Addresses = new List<LocalizationAddress>();
                }

                return m_Addresses.AsReadOnly();
            }
        }

        public LocalizationAddressCollection(string addresses) : this()
        {
            if (addresses == null)
            {
                return;
            }

            var ads = addresses.Split(",", System.StringSplitOptions.RemoveEmptyEntries);
            m_Addresses = new List<LocalizationAddress>();
            foreach (var address in ads)
            {
                m_Addresses.Add(address);
            }
        }

        public override string ToString()
        {
            if (addresses.Count == 0)
            {
                return "Null";
            }
            else if (addresses.Count == 1)
            {
                return addresses.ElementAt(0).ToString();
            }
            else
            {
                var str = addresses.ElementAt(0).ToString();
                for (int i = 1; i < addresses.Count; i++)
                {
                    str += "," + addresses.ElementAt(i).ToString();
                }

                return str;
            }
        }

        public static implicit operator LocalizationAddressCollection(string addresses) => new LocalizationAddressCollection(addresses);
    }
}
