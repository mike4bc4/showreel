using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Layers
{
    public abstract class Layer : BaseLayer
    {
        public const float DefaultBlurSize = 6f;

        const string k_TintPropertyName = "_Tint";
        const string k_CropRectPropertyName = "_CropRect";
        const string k_BlurSizePropertyName = "_BlurSize";
        const string k_BlurQualityPropertyName = "_BlurQuality";
        const string k_OverscanPropertyName = "_Overscan";

        const string k_UseCropRectKeyword = "_USE_CROP_RECT";
        const string k_BlurEnabledKeyword = "_BLUR_ENABLED";

        Material m_Material;
        Color m_Tint;
        float m_Alpha;
        VisualElement m_MaskElement;
        float m_BlurSize;
        float m_BlurQuality;
        Overscan m_Overscan;

        public Material material
        {
            get => m_Material;
        }

        public Color tint
        {
            get => m_Tint;
            set
            {
                m_Tint = value;
                m_Tint.a = alpha;
                material.SetColor(k_TintPropertyName, value);
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
                material.SetColor(k_TintPropertyName, color);
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
                material.SetFloat(k_BlurSizePropertyName, value);
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

        public float blurQuality
        {
            get => m_BlurQuality;
            set
            {
                m_BlurQuality = value;
                material.SetFloat(k_BlurQualityPropertyName, m_BlurQuality);
            }
        }

        public Overscan overscan
        {
            get => m_Overscan;
            set
            {
                m_Overscan = value;
                material.SetVector(k_OverscanPropertyName, m_Overscan);
            }
        }

        protected void Init(Material material)
        {
            base.Init();
            m_Material = material;
            blurSize = 0f;
            blurQuality = SettingsManager.BlurQuality.value;
            tint = Color.white;
            alpha = 1f;
        }

        void Update()
        {
            if (visible)
            {
                UpdateTrackedElement();
            }
        }

        void UpdateTrackedElement()
        {
            // Added panel check as list boards are swapping visual tree assets, and may leave a layer
            // with mask element from detached tree. Although we could make sure that every layer drops
            // unused mask element to avoid exception, such solution feels like limiting layer's functionality
            // as attaching and detaching visual elements from tree is quite a common practice and
            // layer should respect that.
            if (m_MaskElement != null && m_MaskElement.panel != null)
            {
                var rootSize = m_MaskElement.panel.visualTree.worldBound.size;
                var rect = m_MaskElement.worldBound;
                var x = rect.x / rootSize.x;
                var y = 1f - rect.yMax / rootSize.y;
                var w = rect.width / rootSize.x;
                var h = rect.height / rootSize.y;

                material.SetVector(k_CropRectPropertyName, new Vector4(x, y, w, h));
            }
        }

        void OnDestroy()
        {
            Destroy(m_Material);
        }
    }
}
