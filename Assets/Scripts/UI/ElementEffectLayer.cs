using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class ElementEffectLayer : MonoBehaviour
    {
        const string k_ElementRectPropertyName = "_ElementRect";
        const string k_OverscanPropertyName = "_Overscan";
        const string k_BlurSizePropertyName = "_BlurSize";
        const string k_BlurEnabledKeyword = "_BLUR_ENABLED";

        RawImage m_RawImage;
        RectTransform m_RectTransform;
        VisualElement m_TrackedElement;
        Texture m_Texture;
        Overscan m_Overscan;
        float m_BlurSize;
        LocalKeyword m_BlurEnabledKeyword;

        public float blurSize
        {
            get => m_BlurSize;
            set
            {
                m_BlurSize = Mathf.Max(0f, value);
                if (m_RawImage?.material != null)
                {
                    m_RawImage.material.SetFloat(k_BlurSizePropertyName, m_BlurSize);
                    m_RawImage.material.SetKeyword(m_BlurEnabledKeyword, m_BlurSize > 0f);
                }
            }
        }

        public Overscan overscan
        {
            get => m_Overscan;
            set
            {
                m_Overscan = value;
                if (m_RawImage?.material != null)
                {
                    m_RawImage.material.SetVector(k_OverscanPropertyName, m_Overscan);
                }
            }
        }

        public Texture texture
        {
            get => m_Texture;
            set
            {
                m_Texture = value;
                if (m_RawImage != null)
                {
                    m_RawImage.texture = m_Texture;
                }
            }
        }

        public VisualElement trackedElement
        {
            get => m_TrackedElement;
            set => m_TrackedElement = value;
        }

        public static ElementEffectLayer Create(string name)
        {
            var gameObject = new GameObject(name);
            var effectLayer = gameObject.AddComponent<ElementEffectLayer>();
            return effectLayer;
        }

        public void Init()
        {
            m_RectTransform = GetComponent<RectTransform>();
            if (m_RectTransform == null)
            {
                m_RectTransform = gameObject.AddComponent<RectTransform>();
            }

            m_RectTransform.anchorMin = Vector2.zero;
            m_RectTransform.anchorMax = Vector2.one;
            m_RectTransform.offsetMin = Vector2.zero;
            m_RectTransform.offsetMax = Vector2.zero;

            m_RawImage = GetComponent<RawImage>();
            m_RawImage ??= gameObject.AddComponent<RawImage>();
            if (m_RawImage.material == null || m_RawImage.material.shader != LayerManager.EffectLayerShader)
            {
                m_RawImage.material = new Material(LayerManager.EffectLayerShader);
            }

            m_BlurEnabledKeyword = new LocalKeyword(m_RawImage.material.shader, k_BlurEnabledKeyword);

            texture = m_Texture;
            overscan = m_Overscan;
            blurSize = m_BlurSize;
        }

        public void SetBlurSize(float blurSize)
        {
            this.blurSize = blurSize;
        }

        void Update()
        {
            UpdateElementRect();
        }

        void UpdateElementRect()
        {
            if (m_TrackedElement != null)
            {
                var rect = m_TrackedElement.worldBound;
                m_RawImage?.material?.SetVector(k_ElementRectPropertyName, new Vector4(rect.x, Screen.height - rect.yMax, rect.width, rect.height));
            }
        }

        void OnDestroy()
        {
            if (m_RawImage?.material != null)
            {
                Destroy(m_RawImage.material);
            }
        }
    }
}
