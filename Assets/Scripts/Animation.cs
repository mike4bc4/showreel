using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class Animation2
{
    public event Action onFinished;

    object m_Object;
    string m_Property;
    UniTask m_Task;
    float m_Time;
    TimingFunction m_TimingFunction;
    float m_ElapsedTime;
    Reference<CancellationTokenSource> m_Cst;
    Reference<bool> m_Finished;

    public bool finished
    {
        get => m_Finished;
    }

    public object obj
    {
        get => m_Object;
    }

    public string property
    {
        get => m_Property;
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

    public Animation2(object obj, string property, UniTask task, Reference<CancellationTokenSource> cst, Reference<bool> finished)
    {
        m_Object = obj;
        m_Property = property;
        m_Time = 1f;
        m_Task = task;
        m_Cst = cst;
        m_Finished = finished;
    }

    public void InvokeFinished()
    {
        onFinished?.Invoke();
    }

    public UniTask AsTask(CancellationToken ct = default)
    {
        SetTaskCancellationToken(ct);
        return m_Task;
    }

    public void SetTaskCancellationToken(CancellationToken ct)
    {
        if (ct != default)
        {
            m_Cst.Value = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }
    }
}
