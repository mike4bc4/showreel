using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Scheduler : MonoBehaviour
    {
        public static event Action onUpdate
        {
            add => Instance.m_OnUpdate += value;
            remove => Instance.m_OnUpdate -= value;
        }

        public static event Action onLateUpdate
        {
            add => Instance.m_OnLateUpdate += value;
            remove => Instance.m_OnLateUpdate -= value;
        }

        static Scheduler s_Instance;

        Action m_OnUpdate;
        Action m_OnLateUpdate;

        static Scheduler Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("Scheduler");
                    s_Instance = go.AddComponent<Scheduler>();
                    go.hideFlags |= HideFlags.NotEditable;
                    DontDestroyOnLoad(go);
                }

                return s_Instance;
            }
        }

        void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                Destroy(this);
            }
        }

        void Update()
        {
            m_OnUpdate?.Invoke();
        }

        void LateUpdate()
        {
            m_OnLateUpdate?.Invoke();
        }
    }
}
