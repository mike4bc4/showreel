using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaitForTime : IEnumerator
{
    float m_Time;
    float m_StartTime;

    public object Current
    {
        get
        {
            return null;
        }
    }

    public bool MoveNext()
    {
        if (Application.isPlaying)
        {
            return Time.time < m_StartTime + m_Time;
        }
#if UNITY_EDITOR
        else
        {
            return EditorApplication.timeSinceStartup < m_StartTime + m_Time;
        }
#endif
    }

    public void Reset()
    {

    }

    public WaitForTime(float time)
    {
        m_Time = time;
        if (Application.isPlaying)
        {
            m_StartTime = Time.time;
        }
#if UNITY_EDITOR
        else
        {
            m_StartTime = (float)EditorApplication.timeSinceStartup;
        }
#endif
    }
}
