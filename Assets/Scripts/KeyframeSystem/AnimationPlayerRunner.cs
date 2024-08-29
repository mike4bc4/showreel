using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    // This class is not a MonoBehavior as we want to keep safe access to onUpdate callback.
    // If this class would represent component, said component would be destroyed while application
    // is closing, accessing its instance in that period of time would result in creation of new
    // component and cause leak exception. Wrapping actual component with outer class allows to 
    // control whether new instances can be created, and even when it can't be done, 'dummy' onUpdate 
    // delegate still might be used, though it will not be invoked anymore.
    class AnimationPlayerRunner
    {
        static AnimationPlayerRunner s_Instance;

        class Component : MonoBehaviour
        {
            public event Action onUpdate;

            void Update()
            {
                onUpdate?.Invoke();
            }
        }

        Component m_Component;
        bool m_IsQuitting;
        List<Action> m_Callbacks;

        public static AnimationPlayerRunner Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new AnimationPlayerRunner();
                }

                return s_Instance;
            }
        }

        public static event Action OnUpdate
        {
            add => Instance.m_Callbacks.Add(value);
            remove => Instance.m_Callbacks.Remove(value);
        }

        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            Instance.Awake();
        }

        AnimationPlayerRunner()
        {
            m_Callbacks = new List<Action>();
        }

        void Awake()
        {
            Application.quitting += () => m_IsQuitting = true;
            m_Component = CreateComponent();
            m_Component.onUpdate += Update;
        }

        Component CreateComponent()
        {
            var go = new GameObject("AnimationPlayerRunner");
            var component = go.AddComponent<Component>();
            go.hideFlags |= HideFlags.NotEditable;
            GameObject.DontDestroyOnLoad(go);
            return component;
        }

        void Update()
        {
            if (m_IsQuitting)
            {
                return;
            }

            for (int i = 0; i < m_Callbacks.Count; i++)
            {
                if (m_Callbacks[i] != null)
                {
                    m_Callbacks[i].Invoke();
                }
                else
                {
                    m_Callbacks.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
