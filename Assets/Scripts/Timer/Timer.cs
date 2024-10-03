using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimerUtility
{
    public class Timer
    {
        static TimerRunner s_Runner;

        internal static TimerRunner Runner => s_Runner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var gameObject = new GameObject("TimerRunner");
            s_Runner = gameObject.AddComponent<TimerRunner>();
            GameObject.DontDestroyOnLoad(gameObject);
        }

        public static ITimerHandle NextFrame(Action action)
        {
            if (Application.isPlaying)
            {
                Action nextFrameAction = null;
                s_Runner.update += nextFrameAction = () =>
                {
                    action?.Invoke();
                    s_Runner.update -= nextFrameAction;
                };

                return new TimerHandle(nextFrameAction);
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.CallbackFunction nextFrameAction = null;
                UnityEditor.EditorApplication.update += nextFrameAction = () =>
                {
                    action?.Invoke();
                    UnityEditor.EditorApplication.update -= nextFrameAction;
                };

                return new EditorTimerHandle(nextFrameAction);
#else
                return null;
#endif
            }
        }

        public static ITimerHandle When(Func<bool> predicate, Action action)
        {
            if (Application.isPlaying)
            {
                Action whenAction = null;
                s_Runner.update += whenAction = () =>
                {
                    if (predicate.Invoke())
                    {
                        action?.Invoke();
                        s_Runner.update -= whenAction;
                    }
                };

                return new TimerHandle(whenAction);
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.CallbackFunction whenAction = null;
                UnityEditor.EditorApplication.update += whenAction = () =>
                {
                    if (predicate.Invoke())
                    {
                        action?.Invoke();
                        UnityEditor.EditorApplication.update -= whenAction;
                    }
                };

                return new EditorTimerHandle(whenAction);
#else
                return null;
#endif
            }
        }
    }
}
