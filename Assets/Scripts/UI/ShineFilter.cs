using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class ShineFilter : IFilter
    {
        public const float DefaultOffset = 0.5f;
        public const float DefaultWidth = 0.25f;
        public static readonly Color DefaultColor = Color.white;

        const string k_OffsetPropertyName = "_ShineOffset";
        const string k_WidthPropertyName = "_ShineWidth";
        const string k_ColorPropertyName = "_ShineColor";

        Material m_Material;

        public Material material
        {
            get => m_Material;
        }

        public float offset
        {
            get => m_Material.GetFloat(k_OffsetPropertyName);
            set => m_Material.SetFloat(k_OffsetPropertyName, value);
        }

        public float width
        {
            get => m_Material.GetFloat(k_WidthPropertyName);
            set => m_Material.SetFloat(k_WidthPropertyName, value);
        }

        public Color color
        {
            get => m_Material.GetColor(k_ColorPropertyName);
            set => m_Material.SetColor(k_ColorPropertyName, value);
        }

        public ShineFilter()
        {
            m_Material = GameObject.Instantiate(LayerManager.ShineMaterial);
            offset = DefaultOffset;
            width = DefaultWidth;
            color = DefaultColor;
        }
    }
}
