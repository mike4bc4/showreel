using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

class TaskScheduler
{
    CancellationTokenSource m_Cts;
    int m_ScheduledTaskCount;
    bool m_TaskScheduled;
    bool m_TaskCompleted;

    public bool isReady
    {
        get => m_TaskCompleted && !m_TaskScheduled;
    }

    public CancellationToken token
    {
        get => m_Cts.Token;
    }

    public TaskScheduler()
    {
        m_TaskCompleted = true;
    }

    void Stop()
    {
        if (m_Cts != null)
        {
            m_Cts.Cancel();
            m_Cts.Dispose();
            m_Cts = null;
        }
    }

    /// <summary>
    /// Sends cancel signal to currently executed action (if any) and schedules new action for execution. 
    /// When no other action have been scheduled, new action is executed immediately, otherwise it will 
    /// start when previously scheduled action throws OperationCancelledException or completes successfully.
    /// </summary>
    public UniTask Schedule(Action action, CancellationToken cancellationToken = default)
    {
        return ScheduleInternal(action, cancellationToken);
    }

    /// <summary>
    /// Sends cancel signal to currently executed action (if any) and schedules new action for execution. 
    /// When no other action have been scheduled, new action is executed immediately, otherwise it will 
    /// start when previously scheduled action throws OperationCancelledException or completes successfully.
    /// </summary>
    public UniTask Schedule(Func<UniTask> action, CancellationToken cancellationToken = default)
    {
        return ScheduleInternal(action, cancellationToken);
    }

    public UniTask Schedule(Func<CancellationToken, UniTask> action, CancellationToken cancellationToken = default)
    {
        Stop();
        m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
        return UniTask.Create(async () =>
        {
            m_TaskScheduled = true;
            if (!m_TaskCompleted)
            {
                try
                {
                    await UniTask.WaitUntil(() => m_TaskCompleted, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    m_TaskScheduled = false;
                }
            }

            m_TaskCompleted = false;
            m_TaskScheduled = false;

            try
            {
                await action(token);
            }
            finally
            {
                m_TaskCompleted = true;
            }
        });
    }

    UniTask ScheduleInternal(object action, CancellationToken cancellationToken = default)
    {
        Stop();
        m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
        return UniTask.Create(async () =>
        {
            m_TaskScheduled = true;
            if (!m_TaskCompleted)
            {
                try
                {
                    await UniTask.WaitUntil(() => m_TaskCompleted, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    m_TaskScheduled = false;
                }
            }

            m_TaskCompleted = false;
            m_TaskScheduled = false;

            if (action is Action a)
            {
                a();
                m_TaskCompleted = true;
            }
            else if (action is Func<UniTask> func)
            {
                try
                {
                    await func();
                }
                finally
                {
                    m_TaskCompleted = true;
                }
            }
            else
            {
                m_TaskCompleted = true;
                throw new ArgumentException();
            }
        });
    }
}