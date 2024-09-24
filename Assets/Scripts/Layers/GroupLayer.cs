using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Layers
{
    public class GroupLayer : BaseLayer
    {
        List<BaseLayer> m_Layers;
        bool m_Dirty;
        bool m_IsQuitting;

        public bool dirty
        {
            get => m_Dirty;
        }

        public int count
        {
            get => m_Layers.Count;
        }

        public BaseLayer this[int index]
        {
            get
            {
                if (0 <= index && index < count)
                {
                    return m_Layers[index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        public List<BaseLayer> descendants
        {
            get
            {
                var descendants = new List<BaseLayer>();
                foreach (var layer in m_Layers)
                {
                    descendants.Add(layer);
                    if (layer is GroupLayer groupLayer)
                    {
                        descendants.AddRange(groupLayer.descendants);
                    }
                }

                return descendants;
            }
        }

        public new void Init()
        {
            base.Init();
            m_Layers = new List<BaseLayer>();
            Application.quitting += () => m_IsQuitting = true;
        }

        public void SetDirty()
        {
            m_Dirty = true;
        }

        public void Add(BaseLayer layer)
        {
            if (!m_Layers.Contains(layer))
            {
                if (layer.group != null)
                {
                    layer.group.Remove(layer);
                }

                m_Layers.Add(layer);
                layer.group = this;
                layer.transform.SetParent(transform);
                SetDirty();
            }
        }

        public void Remove(BaseLayer layer)
        {
            if (m_Layers.Remove(layer))
            {
                // Layer is a MonoBehaviour, which can be removed from the list even when its equality
                // operator returns null, but it cannot be modified (as it was destroyed).
                if (layer != null)
                {
                    layer.group = null;
                    layer.transform.SetParent(LayerManager.Instance.transform);
                }

                SetDirty();
            }
        }

        internal void Sort()
        {
            m_Layers.Sort(Comparer);
            for (int i = 0; i < m_Layers.Count; i++)
            {
                BaseLayer layer = m_Layers[i];
                layer.transform.SetSiblingIndex(i);
            }

            m_Dirty = false;
        }

        void OnDestroy()
        {
            if (m_IsQuitting)
            {
                return;
            }

            for (int i = 0; i < m_Layers.Count; i++)
            {
                BaseLayer layer = m_Layers[i];
                Remove(layer);
                i--;
            }
        }
    }
}
