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
        static Component s_Component;
        static bool s_Quitting;

        class Component : MonoBehaviour
        {
            public event Action onUpdate;

            void Update()
            {
                onUpdate?.Invoke();
            }
        }

        public static event Action onUpdate
        {
            add
            {
                if (!s_Quitting)
                {
                    component.onUpdate += value;
                }
            }
            remove
            {
                if (!s_Quitting)
                {
                    component.onUpdate -= value;
                }
            }
        }

        static Component component
        {
            get
            {
                if (s_Component == null)
                {
                    var go = new GameObject("AnimationPlayerRunner");
                    s_Component = go.AddComponent<Component>();
                    go.hideFlags |= HideFlags.NotEditable;
                    GameObject.DontDestroyOnLoad(go);
                }

                return s_Component;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            Application.quitting += () => s_Quitting = true;
        }
    }
}
