using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimerUtility
{
    public interface ITimerHandle
    {
        public void Cancel();
    }

    class TimerHandle : ITimerHandle
    {
        Action m_Callback;

        public TimerHandle(Action callback)
        {
            m_Callback = callback;
        }

        public void Cancel()
        {
            if (m_Callback != null)
            {
                Timer.Runner.update -= m_Callback;
            }
        }
    }

#if UNITY_EDITOR
    class EditorTimerHandle : ITimerHandle
    {
        UnityEditor.EditorApplication.CallbackFunction m_EditorCallback;

        public EditorTimerHandle(UnityEditor.EditorApplication.CallbackFunction editorCallback)
        {
            m_EditorCallback = editorCallback;
        }

        public void Cancel()
        {
            if (m_EditorCallback != null)
            {
                UnityEditor.EditorApplication.update -= m_EditorCallback;
            }
        }
    }
#endif
}
