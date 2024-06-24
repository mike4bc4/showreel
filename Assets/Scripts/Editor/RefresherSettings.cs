using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public class RefresherSettings : ScriptableObject
    {
        [SerializeField] private bool m_IsEnabled;  // m_Enabled is already serialized
        [SerializeField] private float m_Interval;

        public bool enabled
        {
            get => m_IsEnabled;
        }

        public float interval
        {
            get => m_Interval;
        }

        public static RefresherSettings CreateInstance()
        {
            var settings = CreateInstance<RefresherSettings>();
            settings.m_IsEnabled = false;
            settings.m_Interval = 0.5f;
            return settings;
        }
    }
}
