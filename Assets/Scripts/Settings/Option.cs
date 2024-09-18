using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public abstract class Option
    {
        [SerializeField] string m_Name;

        public string name => m_Name;

        public Option(string name)
        {
            m_Name = name;
        }
    }

    [Serializable]
    public class Option<T> : Option
    {
        [SerializeField] T m_value;

        public T value => m_value;

        public Option(string name, T primaryValue) : base(name)
        {
            m_value = primaryValue;
        }
    }

    [Serializable]
    public class Option<T1, T2> : Option
    {
        [SerializeField] T1 m_PrimaryValue;
        [SerializeField] T2 m_SecondaryValue;

        public T1 primaryValue => m_PrimaryValue;
        public T2 secondaryValue => m_SecondaryValue;

        public Option(string name, T1 primaryValue, T2 secondaryValue) : base(name)
        {
            m_PrimaryValue = primaryValue;
            m_SecondaryValue = secondaryValue;
        }
    }
}
