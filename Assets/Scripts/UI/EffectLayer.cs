using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EffectLayer : LayerBase
    {
        public const float DefaultBlurSize = 8f;

        // const string k_AlphaTexPropertyName = "_AlphaTex";
        const string k_QualityPropertyName = "_Quality";
        const string k_BlurSizePropertyName = "_Size";
        const string k_BlurOnKeyword = "BLUR_ON";
        const float k_BlurDisabledEpsilon = 0.1f;

        bool m_BlurEnabled;

        public float blurSize
        {
            get => m_RawImage.material.GetFloat(k_BlurSizePropertyName);
            set
            {
                m_RawImage.material.SetFloat(k_BlurSizePropertyName, value);
                var blurEnabled = value > k_BlurDisabledEpsilon;
                if (blurEnabled != m_BlurEnabled)
                {
                    m_BlurEnabled = blurEnabled;
                    if (m_BlurEnabled)
                    {
                        m_RawImage.material.EnableKeyword(k_BlurOnKeyword);
                    }
                    else
                    {
                        m_RawImage.material.DisableKeyword(k_BlurOnKeyword);
                    }
                }
            }
        }

        public override void Init()
        {
            base.Init();
            m_RawImage.material = Instantiate(LayerManager.BlurEffectMaterial);
            blurSize = 0f;
        }

        public override void ResetLayer()
        {
            base.ResetLayer();
            blurSize = 0f;
        }
    }
}
