using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DynamicMask
{
    public event Action dirtied;

    List<VisualElement> m_VisualElements;
    int m_Width;
    int m_Height;
    bool m_Invert;
    bool m_Dirty;
    Texture m_Texture;

    public Vector2 size
    {
        get => new Vector2(m_Width, m_Height);
        set
        {
            width = m_Width;
            height = m_Height;
        }
    }

    public int width
    {
        get => m_Width;
        set
        {
            var width = Mathf.Max(1, value);
            if (m_Width != width)
            {
                m_Width = width;
                dirty = true;
            }
        }
    }

    public int height
    {
        get => m_Height;
        set
        {
            var height = Mathf.Max(1, value);
            if (m_Height != height)
            {
                m_Height = height;
                dirty = true;
            }
        }
    }

    public bool invert
    {
        get => m_Invert;
        set
        {
            if (m_Invert != value)
            {
                m_Invert = value;
                dirty = true;
            }
        }
    }

    public Texture texture
    {
        get
        {
            if (dirty)
            {
                m_Texture = CreateTexture();
                dirty = false;
            }

            return m_Texture;
        }
    }

    bool dirty
    {
        get => m_Dirty;
        set
        {
            if (m_Dirty != value)
            {
                m_Dirty = value;
                dirtied?.Invoke();
            }
        }
    }

    public void AddElements(params VisualElement[] visualElements)
    {
        foreach (var ve in visualElements)
        {
            if (!m_VisualElements.Contains(ve))
            {
                m_VisualElements.Add(ve);
                dirty = true;
            }
        }
    }

    public void RemoveElements(params VisualElement[] visualElements)
    {
        foreach (var ve in visualElements)
        {
            if (m_VisualElements.Remove(ve))
            {
                dirty = true;
            }
        }
    }

    Texture2D CreateTexture()
    {
        var rects = new List<Rect>();
        foreach (var ve in m_VisualElements)
        {
            rects.Add(ve.worldBound);
        }

        return TextureEditor.CreateMask(size, invert, rects.ToArray());
    }

    public DynamicMask(int width, int height, bool invert)
    {
        m_VisualElements = new List<VisualElement>();
        m_Width = width;
        m_Height = height;
        m_Invert = invert;
        m_Texture = CreateTexture();
    }

    public DynamicMask(Vector2 size, bool invert) : this((int)size.x, (int)size.y, invert) { }
}
