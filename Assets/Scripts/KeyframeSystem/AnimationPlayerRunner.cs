using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    class AnimationPlayerRunner : MonoBehaviour
    {
        static AnimationPlayerRunner s_Instance;

        event Action m_OnUpdate;

        public static event Action onUpdate
        {
            add => instance.m_OnUpdate += value;
            remove => instance.m_OnUpdate -= value;
        }

        public static AnimationPlayerRunner instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("AnimationPlayerRunner");
                    s_Instance = go.AddComponent<AnimationPlayerRunner>();
                    go.hideFlags |= HideFlags.NotEditable;
                    DontDestroyOnLoad(go);
                }

                return s_Instance;
            }
        }

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(this.gameObject);
            }

            s_Instance = this;
        }

        void Update()
        {
            m_OnUpdate?.Invoke();
        }
    }
}
