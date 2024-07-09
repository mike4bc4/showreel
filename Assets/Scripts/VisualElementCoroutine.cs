using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementCoroutine
{
    IVisualElementScheduledItem m_Item;

    public VisualElementCoroutine(IVisualElementScheduledItem item)
    {
        m_Item = item;
    }

    public void Stop()
    {
        if (m_Item != null)
        {
            m_Item.Pause();
            m_Item = null;
        }
    }
}

