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

        public static event Action delayCall
        {
            add => Instance.m_DelayedCalls.Add(value);
            remove => Instance.m_DelayedCalls.Remove(value);
        }

        static Scheduler s_Instance;

        Action m_OnUpdate;
        Action m_OnLateUpdate;
        List<Action> m_DelayedCalls;

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

        public static T AddComponent<T>() where T : Component
        {
            return Instance.gameObject.AddComponent<T>();
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
                return;
            }

            m_DelayedCalls = new List<Action>();
        }

        void Update()
        {
            m_OnUpdate?.Invoke();
        }

        void LateUpdate()
        {
            m_OnLateUpdate?.Invoke();
            
            var delayedCalls = new List<Action>(m_DelayedCalls);
            m_DelayedCalls.Clear();
            foreach (var delayedCall in delayedCalls)
            {
                delayedCall?.Invoke();
            }
        }
    }
}
