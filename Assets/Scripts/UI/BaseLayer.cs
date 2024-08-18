using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public abstract class BaseLayer : MonoBehaviour
    {
        public static readonly IComparer<BaseLayer> Comparer = new LayerComparer();
        public const float DefaultBlurSize = 8f;

        const string k_BlurEnabledKeyword = "_BLUR_ENABLED";
        const string k_BlurSizeProperty = "_BlurSize";
        const string k_TintProperty = "_Tint";
        const string k_CropRectProperty = "_CropRect";
        const string k_UseCropRectKeyword = "_USE_CROP_RECT";
        const string k_OverscanProperty = "_Overscan";

        class LayerComparer : IComparer<BaseLayer>
        {
            public int Compare(BaseLayer x, BaseLayer y)
            {
                return x.displaySortOrder.CompareTo(y.displaySortOrder);
            }
        }

        protected bool m_Initialized;
        protected Material m_Material;

        int m_DisplaySortOrder;
        Color m_Tint;
        float m_Alpha;
        VisualElement m_MaskElement;
        float m_BlurSize;
        Overscan m_Overscan;
        bool m_Visible;
        bool m_Active;

        public bool active
        {
            get => m_Active;
            set => m_Active = value;
        }

        public bool visible
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

        public Material material
        {
            get => m_Material;
        }

        public int displaySortOrder
        {
            get => m_DisplaySortOrder;
            set
            {
                if (value != m_DisplaySortOrder)
                {
                    m_DisplaySortOrder = value;
                    LayerManager.MarkCommandBufferDirty();
                }
            }
        }

        public Color tint
        {
            get => m_Tint;
            set
            {
                m_Tint = value;
                m_Tint.a = alpha;
                material?.SetColor(k_TintProperty, value);
            }
        }

        public float alpha
        {
            get => m_Alpha;
            set
            {
                m_Alpha = Mathf.Clamp01(value);
                var color = m_Tint;
                color.a = m_Alpha;
                material.SetColor(k_TintProperty, color);
            }
        }

        public VisualElement maskElement
        {
            get => m_MaskElement;
            set
            {
                m_MaskElement = value;
                if (m_MaskElement != null)
                {
                    material.EnableKeyword(k_UseCropRectKeyword);
                    UpdateTrackedElement();
                }
                else
                {
                    material.DisableKeyword(k_UseCropRectKeyword);
                }
            }
        }

        public float blurSize
        {
            get => m_BlurSize;
            set
            {
                m_BlurSize = value;
                material.SetFloat(k_BlurSizeProperty, value);
                if (m_BlurSize > 0)
                {
                    material.EnableKeyword(k_BlurEnabledKeyword);
                }
                else
                {
                    material.DisableKeyword(k_BlurEnabledKeyword);
                }
            }
        }

        public Overscan overscan
        {
            get => m_Overscan;
            set
            {
                m_Overscan = value;
                material.SetVector(k_OverscanProperty, m_Overscan);
            }
        }

        protected void Init(Material material)
        {
            if (m_Initialized == true)
            {
                throw new Exception("Cannot initialize multiple times.");
            }

            m_Active = true;
            m_Visible = true;
            m_Material = material;
            blurSize = 0;
            tint = Color.white;
            alpha = 1f;

            m_Initialized = true;
        }

        void Update()
        {
            if (!m_Active)
            {
                return;
            }

            UpdateTrackedElement();
        }

        void OnDestroy()
        {
            Destroy(m_Material);
        }

        void UpdateTrackedElement()
        {
            if (m_MaskElement != null)
            {
                var rect = m_MaskElement.worldBound;
                material.SetVector(k_CropRectProperty, new Vector4(rect.x, Camera.main.pixelHeight - rect.yMax, rect.width, rect.height));
            }
        }
    }
}

