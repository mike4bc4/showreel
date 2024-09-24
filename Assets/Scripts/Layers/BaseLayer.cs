using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Layers
{
    public abstract class BaseLayer : MonoBehaviour
    {
        public static readonly IComparer<BaseLayer> Comparer = new LayerComparer();

        class LayerComparer : IComparer<BaseLayer>
        {
            public int Compare(BaseLayer x, BaseLayer y)
            {
                return x.displaySortOrder.CompareTo(y.displaySortOrder);
            }
        }

        int m_DisplaySortOrder;
        bool m_Visible;
        string m_LayerName;
        GroupLayer m_Group;

        public new string name
        {
            get => m_LayerName;
            set
            {
                m_LayerName = value;
                gameObject.name = $"{GetType().Name}({m_LayerName})";
            }
        }

        public virtual bool visible
        {
            get => m_Visible;
            set
            {
                if (value != m_Visible)
                {
                    m_Visible = value;
                    LayerManager.MarkCommandBufferDirty();
                }
            }
        }

        public virtual int displaySortOrder
        {
            get => m_DisplaySortOrder;
            set
            {
                if (value != m_DisplaySortOrder)
                {
                    m_DisplaySortOrder = value;
                    LayerManager.MarkCommandBufferDirty();
                    if (m_Group != null)
                    {
                        m_Group.SetDirty();
                    }
                }
            }
        }

        public GroupLayer group
        {
            get => m_Group;
            internal set => m_Group = value;
        }

        protected void Init()
        {
            m_Visible = true;
        }

        void OnDestroy()
        {
            if (m_Group != null)
            {
                m_Group.Remove(this);
            }
        }
    }
}

