using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class Timer
    {
        public event Action onTimeout;

        float m_Interval;
        double m_TimeSinceStartup;
        float m_ElapsedTime;
        bool m_Repeat;
        bool m_Started;

        public float interval
        {
            get => m_Interval;
            set => m_Interval = Mathf.Max(0f, value);
        }

        public float elapsedTime
        {
            get => m_ElapsedTime;
        }

        public bool repeat
        {
            get => m_Repeat;
            set => m_Repeat = value;
        }

        public Timer(float interval, bool autoStart = true)
        {
            m_Interval = Mathf.Max(0f, interval);
            if (autoStart)
            {
                Start();
            }
        }

        public void Start()
        {
            if (!m_Started)
            {
                m_TimeSinceStartup = EditorApplication.timeSinceStartup;
                EditorApplication.update += Update;
                m_Started = true;
            }
        }

        public void Stop()
        {
            EditorApplication.update -= Update;
            m_Started = false;
        }

        void Update()
        {
            float delta = (float)(EditorApplication.timeSinceStartup - m_TimeSinceStartup);
            m_TimeSinceStartup = EditorApplication.timeSinceStartup;

            m_ElapsedTime += delta;
            if (m_ElapsedTime < m_Interval)
            {
                return;
            }

            m_ElapsedTime = 0f;
            onTimeout?.Invoke();
            if (!m_Repeat)
            {
                EditorApplication.update -= Update;
                m_Started = false;
            }
        }
    }
}
