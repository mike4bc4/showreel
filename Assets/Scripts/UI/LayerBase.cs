using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class LayerBase : MonoBehaviour
    {
        public static readonly LayerComparer Comparer = new LayerComparer();

        public class LayerComparer : IComparer<LayerBase>
        {
            public int Compare(LayerBase x, LayerBase y)
            {
                if (x.gameObject.activeSelf != y.gameObject.activeSelf)
                {
                    return x.gameObject.activeSelf.CompareTo(y.gameObject.activeSelf);
                }

                if (x.displayOrder == y.displayOrder)
                {
                    return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
                }

                return x.displayOrder.CompareTo(y.displayOrder);
            }
        }

        [SerializeField] protected int m_DisplayOrder;

        protected RawImage m_RawImage;

        public int displayOrder
        {
            get => m_DisplayOrder;
            set
            {
                m_DisplayOrder = value;
                LayerManager.SortLayers();
            }
        }

        public Texture texture
        {
            get => m_RawImage.texture;
        }

        public Color color
        {
            get
            {
                var color = m_RawImage.color;
                color.a = 1f;
                return color;
            }
            set
            {
                var color = value;
                color.a = m_RawImage.color.a;
                m_RawImage.color = color;
            }
        }

        public float alpha
        {
            get => m_RawImage.color.a;
            set
            {
                var color = m_RawImage.color;
                color.a = Mathf.Clamp01(value);
                m_RawImage.color = color;
            }
        }

        public virtual void Init()
        {
            m_RawImage = GetComponent<RawImage>();
            if (m_RawImage == null)
            {
                m_RawImage = gameObject.AddComponent<RawImage>();
            }
        }

        public virtual void Clear()
        {
            color = Color.white;
            alpha = 1f;
        }
    }
}
