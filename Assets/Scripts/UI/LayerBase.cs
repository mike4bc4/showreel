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
                if (x.active != y.active)
                {
                    return x.active.CompareTo(y.active);
                }

                return x.displaySortOrder.CompareTo(y.displaySortOrder);
            }
        }

        [SerializeField] int m_DisplaySortOrder;
        [SerializeField] bool m_Active;

        protected RawImage m_RawImage;
        float m_Alpha;

        public bool active
        {
            get => m_Active;
            set
            {
                if (m_Active != value)
                {
                    m_Active = value;
                    if (m_Active)
                    {
                        OnActivate();
                    }
                    else
                    {
                        OnDeactivate();
                    }

                    LayerManager.SortLayers();
                }
            }
        }

        public int displaySortOrder
        {
            get => m_DisplaySortOrder;
            set
            {
                m_DisplaySortOrder = value;
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
            get => m_Alpha;
            set
            {
                m_Alpha = Mathf.Clamp01(value);
                if (active)
                {
                    var color = m_RawImage.color;
                    color.a = m_Alpha;
                    m_RawImage.color = color;
                }
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
            m_RawImage ??= gameObject.AddComponent<RawImage>();
        }

        void Awake()
        {
            m_Active = true;
        }

        protected virtual void OnActivate()
        {
            alpha = m_Alpha;
        }

        protected virtual void OnDeactivate()
        {
            m_Alpha = m_RawImage.color.a;
            var color = m_RawImage.color;
            color.a = 0f;
            m_RawImage.color = color;
        }

        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
        }

        public void SetBlur(float blur)
        {
            this.blur = blur;
        }

        public void SetColor(Color color)
        {
            this.color = color;
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
