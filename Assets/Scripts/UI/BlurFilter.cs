using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class BlurFilter : IFilter
    {
        public const float DefaultSize = 8f;

        const string k_QualityPropertyName = "_Quality";
        const string k_SizePropertyName = "_Size";

        Material m_Material;

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
            set => m_Material.SetFloat(k_SizePropertyName, value);
        }

        public BlurFilter()
        {
            m_Material = GameObject.Instantiate(LayerManager.BlurMaterial);
        }

        public BlurFilter(float size) : this()
        {
            this.size = size;
        }
    }
}
