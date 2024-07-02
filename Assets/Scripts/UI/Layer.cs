using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class Layer : LayerBase
    {
        UIDocument m_UIDocument;
        IFilter m_Filter;

        public float panelSortingOrder
        {
            get => m_UIDocument.panelSettings.sortingOrder;
            set => m_UIDocument.panelSettings.sortingOrder = value;
        }

        public IFilter filter
        {
            get => m_Filter;
            set
            {
                m_Filter = value;
                m_RawImage.material = m_Filter != null ? m_Filter.material : null;
            }
        }

        public VisualTreeAsset visualTreeAsset
        {
            get => m_UIDocument.visualTreeAsset;
            set => m_UIDocument.visualTreeAsset = value;
        }

        public VisualElement rootVisualElement
        {
            get => m_UIDocument.rootVisualElement;
        }

        public override void Init()
        {
            base.Init();
            m_UIDocument = GetComponent<UIDocument>();
            if (m_UIDocument == null)
            {
                m_UIDocument = gameObject.AddComponent<UIDocument>();
                m_UIDocument.panelSettings = Instantiate(LayerManager.TemplatePanelSettings);
            }

            var renderTexture = new RenderTexture(LayerManager.TemplateRenderTexture);
            m_UIDocument.panelSettings.targetTexture = renderTexture;
            m_RawImage.texture = renderTexture;
        }

        public override void Clear()
        {
            base.Clear();
            filter = null;
            visualTreeAsset = null;
        }
    }
}
