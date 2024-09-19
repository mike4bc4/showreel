using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Scheduler : MonoBehaviour
    {
        class CallbackObject
        {
            Action m_Action;
            float m_Interval;
            float m_Delta;

            public Action action => m_Action;
            public float interval
            {
                get => m_Interval;
                set => m_Interval = Mathf.Max(0f, value);
            }

            public float delta
            {
                get => m_Delta;
                set => m_Delta = value;
            }

            public CallbackObject(Action action, float interval)
            {
                m_Action = action;
                m_Interval = interval;
            }
        }

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
        List<CallbackObject> m_CallbackObjects;

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
            m_CallbackObjects = new List<CallbackObject>();
        }

        void Update()
        {
            m_OnUpdate?.Invoke();
            UpdateCallbackObjects();
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

        void UpdateCallbackObjects()
        {
            for (int i = 0; i < m_CallbackObjects.Count; i++)
            {
                CallbackObject callbackObject = m_CallbackObjects[i];
                if (callbackObject.action == null)
                {
                    m_CallbackObjects.RemoveAt(i);
                }
                else
                {
                    callbackObject.delta += Time.deltaTime;
                    if (callbackObject.delta >= callbackObject.interval)
                    {
                        callbackObject.delta = 0f;
                        callbackObject.action?.Invoke();
                    }
                }
            }
        }

        public static void RegisterCallbackEvery(Action action, float interval)
        {
            var callbackObjectIndex = GetCallbackObjectIndex(action);
            if (callbackObjectIndex < 0)
            {
                var callbackObject = new CallbackObject(action, interval);
                Instance.m_CallbackObjects.Add(callbackObject);
            }
            else
            {
                Instance.m_CallbackObjects[callbackObjectIndex].interval = interval;
            }
        }

        public static void UnregisterCallbackEvery(Action action)
        {
            var callbackObjectIndex = GetCallbackObjectIndex(action);
            if (callbackObjectIndex >= 0)
            {
                Instance.m_CallbackObjects.RemoveAt(callbackObjectIndex);
            }
        }

        static int GetCallbackObjectIndex(Action action)
        {
            for (int i = 0; i < Instance.m_CallbackObjects.Count; i++)
            {
                CallbackObject callbackObject = Instance.m_CallbackObjects[i];
                if (callbackObject.action == action)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
