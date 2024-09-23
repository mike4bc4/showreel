using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Scheduler : MonoBehaviour
    {
        class Interval
        {
            public Guid id { get; }
            public Action func { get; }
            public int delay { get; }
            public float delta { get; set; }

            public Interval(Action func, int delay)
            {
                this.id = System.Guid.NewGuid();
                this.func = func;
                this.delay = delay;
                this.delta = 0;
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
        Dictionary<Guid, Interval> m_Intervals;

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
            m_Intervals = new Dictionary<Guid, Interval>();
        }

        void Update()
        {
            m_OnUpdate?.Invoke();
            UpdateIntervals();
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

        void UpdateIntervals()
        {
            foreach (var kv in m_Intervals)
            {
                var interval = kv.Value;
                interval.delta += Time.deltaTime;
                if (interval.delta * 1000f >= interval.delay)
                {
                    interval.delta = 0;
                    interval.func?.Invoke();
                }
            }
        }

        public static Guid SetInterval(Action func, int delay)
        {
            var interval = new Interval(func, delay);
            Instance.m_Intervals.Add(interval.id, interval);
            return interval.id;
        }

        public static void ClearInterval(Guid id)
        {
            Instance.m_Intervals.Remove(id);
        }
    }
}
