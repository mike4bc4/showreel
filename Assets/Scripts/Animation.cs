using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation
{
    public event Action onFinished;

    object m_Object;
    string m_Property;
    Coroutine m_Coroutine;
    float m_Time;
    TimingFunction m_TimingFunction;
    float m_ElapsedTime;

    public object obj
    {
        get => m_Object;
    }

    public string property
    {
        get => m_Property;
    }

    public Coroutine coroutine
    {
        get => m_Coroutine;
    }

    public float time
    {
        get => m_Time;
        set => m_Time = Mathf.Max(value, 0.001f);
    }

    public TimingFunction timingFunction
    {
        get => m_TimingFunction;
        set => m_TimingFunction = value;
    }

    public float elapsedTime
    {
        get => m_ElapsedTime;
        set => m_ElapsedTime = value;
    }

    public Animation(object obj, string property, Coroutine coroutine)
    {
        m_Object = obj;
        m_Property = property;
        m_Coroutine = coroutine;
        m_Time = 1f;
    }

    public void InvokeFinished()
    {
        onFinished?.Invoke();
    }
}
