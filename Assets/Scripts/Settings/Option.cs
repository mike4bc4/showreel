using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public class Option<T>
    {
        [SerializeField] string m_Name;
        [SerializeField] T m_Value;

        public string name => m_Name;
        public T value => m_Value;

        public Option(string name, T value)
        {
            m_Name = name;
            m_Value = value;
        }
    }

    [Serializable]
    public class OptionSet<T>
    {
        [SerializeField] int m_DefaultOptionIndex;
        [SerializeField] List<Option<T>> m_Options;

        public int defaultOptionIndex => Mathf.Clamp(m_DefaultOptionIndex, 0, m_Options.Count - 1);
        public IReadOnlyList<Option<T>> options => m_Options.AsReadOnly();

        public OptionSet(int defaultOptionIndex, List<Option<T>> options)
        {
            m_DefaultOptionIndex = defaultOptionIndex;
            m_Options = options;
        }
    }
}
