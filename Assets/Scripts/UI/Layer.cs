using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class Layer : MonoBehaviour
    {
        public static readonly LayerComparer Comparer = new LayerComparer();

        public class LayerComparer : IComparer<Layer>
        {
            public int Compare(Layer x, Layer y)
            {
                if (x.gameObject.activeSelf != y.gameObject.activeSelf)
                {
                    return x.gameObject.activeSelf.CompareTo(y.gameObject.activeSelf);
                }

                if (x.displayOrder == y.displayOrder)
                {
                    return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
                }
                else
                {
                    return x.displayOrder.CompareTo(y.displayOrder);
                }
            }
        }

        [SerializeField] int m_DisplayOrder;

        UIDocument m_UIDocument;
        RawImage m_RawImage;
        IFilter m_Filter;

        public int displayOrder
        {
            get => m_DisplayOrder;
            set
            {
                m_DisplayOrder = value;
                LayerManager.SortLayers();
            }
        }

        public float panelSortingOrder
        {
            get => m_UIDocument.panelSettings.sortingOrder;
            set => m_UIDocument.panelSettings.sortingOrder = value;
        }

        public Texture texture
        {
            get => m_RawImage.texture;
        }

        public IFilter filter
        {
            get => m_Filter;
            set
            {
                m_Filter = value;
                m_RawImage.material = m_Filter != null ? m_Filter.material : null;
            }
        }

        public Color color
        {
            get => m_RawImage.color;
            set => m_RawImage.color = value;
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

        public VisualTreeAsset visualTreeAsset
        {
            get => m_UIDocument.visualTreeAsset;
            set => m_UIDocument.visualTreeAsset = value;
        }

        public VisualElement rootVisualElement
        {
            get => m_UIDocument.rootVisualElement;
        }

        public int position
        {
            get => transform.GetSiblingIndex();
            set => transform.SetSiblingIndex(Mathf.Clamp(value, 0, transform.parent.childCount - 1));
        }

        public void Init()
        {
            m_RawImage = GetComponent<RawImage>();
            m_UIDocument = GetComponent<UIDocument>();
        }

        public void Clear()
        {
            filter = null;
            color = Color.white;
            alpha = 1f;
            visualTreeAsset = null;
        }

        public void MoveInFrontOf(Layer layer)
        {
            if (layer == this)
            {
                return;
            }

            if (position < layer.position)
            {
                position = layer.position;
            }
            else
            {
                position = layer.position + 1;
            }
        }

        public void MoveBehind(Layer layer)
        {
            if (layer == this)
            {
                return;
            }

            if (position > layer.position)
            {
                position = layer.position;
            }
            else
            {
                position = layer.position - 1;
            }
        }
    }
}
