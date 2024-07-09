using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class MaskFilter : IFilter
    {
        const string k_AlphaTexPropertyName = "_AlphaTex";
        const string k_UseAlphaMaskKeyword = "USE_ALPHA_MASK";
        const string k_InvertAlphaMaskPropertyName = "_InvertAlphaMask";

        Material m_Material;
        bool m_UseAlphaMask;

        public Material material
        {
            get => m_Material;
        }

        public bool invertMask
        {
            get => m_Material.GetFloat(k_InvertAlphaMaskPropertyName) != 0;
            set => m_Material.SetFloat(k_InvertAlphaMaskPropertyName, value ? 1f : 0f);
        }

        public Texture alphaTexture
        {
            get => m_Material.GetTexture(k_AlphaTexPropertyName);
            set
            {
                m_Material.SetTexture(k_AlphaTexPropertyName, value);
                var useAlphaMask = value != null;
                if (m_UseAlphaMask != useAlphaMask)
                {
                    m_UseAlphaMask = useAlphaMask;
                    if (useAlphaMask)
                    {
                        m_Material.EnableKeyword(k_UseAlphaMaskKeyword);
                    }
                    else
                    {
                        m_Material.DisableKeyword(k_UseAlphaMaskKeyword);
                    }
                }
            }
        }

        public MaskFilter()
        {
            m_Material = GameObject.Instantiate(LayerManager.MaskMaterial);
            alphaTexture = null;
            m_Material.DisableKeyword(k_UseAlphaMaskKeyword);
        }
    }
}
