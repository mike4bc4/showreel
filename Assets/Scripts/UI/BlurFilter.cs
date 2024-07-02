using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UI
{
    public class BlurFilter : IFilter
    {
        public const float DefaultQuality = 0.5f;
        public const float DefaultSize = 8f;

        const float k_BlurDisabledEpsilon = 0.1f;

        const string k_QualityPropertyName = "_Quality";
        const string k_SizePropertyName = "_Size";

        Material m_Material;
        bool m_BlurEnabled;

        public Material material
        {
            get => m_Material;
        }

        public float quality
        {
            get => m_Material.GetFloat(k_QualityPropertyName);
            set => m_Material.SetFloat(k_QualityPropertyName, value);
        }

        public float size
        {
            get => m_Material.GetFloat(k_SizePropertyName);
            set
            {
                m_Material.SetFloat(k_SizePropertyName, value);
                var blurEnabled = value > k_BlurDisabledEpsilon;
                if (blurEnabled != m_BlurEnabled)
                {
                    m_BlurEnabled = blurEnabled;
                    if (m_BlurEnabled)
                    {
                        m_Material.EnableKeyword("BLUR_ON");
                    }
                    else
                    {
                        m_Material.DisableKeyword("BLUR_ON");
                    }
                }
            }
        }

        public BlurFilter()
        {
            m_Material = GameObject.Instantiate(LayerManager.BlurMaterial);
            size = DefaultSize;
            quality = DefaultQuality;
        }
    }
}
