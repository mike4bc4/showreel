using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskChain
{
    struct LinkTask
    {
        Action m_Task;
        Func<UniTask> m_AsyncTask;

        public LinkTask(Action task)
        {
            m_Task = task;
            m_AsyncTask = null;
        }

        public LinkTask(Func<UniTask> asyncTask)
        {
            m_Task = null;
            m_AsyncTask = asyncTask;
        }

        public void Execute(out UniTask? task)
        {
            task = null;
            if (m_Task != null)
            {
                m_Task();
            }
            else if (m_AsyncTask != null)
            {
                task = m_AsyncTask();
            }
        }
    }

    List<(LinkTask, LinkTask)> m_Links;
    int m_Index;

    public int index
    {
        get => m_Index;
        set => m_Index = Mathf.Clamp(value, 0, m_Links.Count - 1);
    }

    public TaskChain()
    {
        m_Links = new List<(LinkTask to, LinkTask from)>();
    }

    public void AddLink(Func<UniTask> to, Action from)
    {
        m_Links.Add((new LinkTask(to), new LinkTask(from)));
    }

    public void AddLink(Action to, Func<UniTask> from)
    {
        m_Links.Add((new LinkTask(to), new LinkTask(from)));
    }

    public void AddLink(Func<UniTask> to, Func<UniTask> from)
    {
        m_Links.Add((new LinkTask(to), new LinkTask(from)));
    }

    public void AddLink(Action to, Action from)
    {
        m_Links.Add((new LinkTask(to), new LinkTask(from)));
    }

    public async UniTask GoForward()
    {
        var links = m_Links.GetRange(m_Index, m_Links.Count - m_Index);
        foreach ((LinkTask to, LinkTask from) link in links)
        {
            link.to.Execute(out var task);
            if (task != null)
            {
                await (UniTask)task;
            }

            m_Index++;
        }

        m_Index = links.Count - 1;
    }

    public async UniTask GoBackward()
    {
        var links = m_Links.GetRange(0, m_Index + 1);
        links.Reverse();
        foreach ((LinkTask to, LinkTask from) link in links)
        {
            link.from.Execute(out var task);
            if (task != null)
            {
                await (UniTask)task;
            }

            m_Index--;
        }

        m_Index = 0;
    }
}
