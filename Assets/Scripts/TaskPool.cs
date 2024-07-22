using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskPool
{
    Dictionary<int, Func<UniTask>> m_AsyncTasks;
    Dictionary<int, Action> m_Tasks;
    int m_Length;

    public int length
    {
        get => m_Length;
    }

    public object this[int i]
    {
        get
        {
            if (i < 0 || i >= m_Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (m_AsyncTasks.TryGetValue(i, out var func))
            {
                return func;
            }
            else
            {
                return m_Tasks[i];
            }
        }
    }

    public TaskPool()
    {
        m_AsyncTasks = new Dictionary<int, Func<UniTask>>();
        m_Tasks = new Dictionary<int, Action>();
    }

    public void Add(Func<UniTask> task)
    {
        m_AsyncTasks.Add(m_Length, task);
        m_Length += 1;
    }

    public void Add(Action task)
    {
        m_Tasks.Add(m_Length, task);
        m_Length += 1;
    }

    public TaskPool GetRange(int index, int count)
    {
        if ((index < 0 || index >= length) || (count < 0 || index + count > length))
        {
            throw new ArgumentOutOfRangeException();
        }

        var taskPool = new TaskPool();
        for (int i = index; i < index + count; i++)
        {
            switch (this[i])
            {
                case Func<UniTask> func:
                    taskPool.Add(func);
                    break;
                case Action func:
                    taskPool.Add(func);
                    break;
            }
        }

        return taskPool;
    }

    public void Reverse()
    {
        var asyncTasks = new Dictionary<int, Func<UniTask>>();
        foreach (var entry in m_AsyncTasks)
        {
            asyncTasks.Add(length - entry.Key - 1, entry.Value);
        }

        m_AsyncTasks = asyncTasks;

        var tasks = new Dictionary<int, Action>();
        foreach (var entry in m_Tasks)
        {
            tasks.Add(length - entry.Key - 1, entry.Value);
        }

        m_Tasks = tasks;
    }
}
