using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class LayerBase : MonoBehaviour
    {
        public static readonly LayerComparer Comparer = new LayerComparer();
        public const float DefaultBlur = 8f;

        const string k_BlurSizePropertyName = "_BlurSize";
        const string k_BlurOnKeyword = "BLUR_ON";
        const float k_BlurDisabledEpsilon = 0.1f;

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

        public float blur
        {
            get => m_RawImage.material.GetFloat(k_BlurSizePropertyName);
            set
            {
                m_RawImage.material.SetFloat(k_BlurSizePropertyName, value);
                if (value > k_BlurDisabledEpsilon)
                {
                    m_RawImage.material.EnableKeyword(k_BlurOnKeyword);
                }
                else
                {
                    m_RawImage.material.DisableKeyword(k_BlurOnKeyword);
                }
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

        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
        }

        public void SetBlur(float blur)
        {
            this.blur = blur;
        }

        public void Clear()
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = (RenderTexture)texture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        public virtual void ResetLayer()
        {
            color = Color.white;
            alpha = 1f;
            blur = 0f;
            Clear();
        }
    }
}
