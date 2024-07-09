using System;
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

        public bool receivesInput
        {
            get => rootVisualElement.enabledSelf;
            set => rootVisualElement.SetEnabled(value);
        }

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

            var renderTexture = m_UIDocument.panelSettings.targetTexture;
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(LayerManager.TemplateRenderTexture);
                m_UIDocument.panelSettings.targetTexture = renderTexture;
            }

            m_RawImage.texture = renderTexture;
        }

        public override void Clear()
        {
            base.Clear();
            filter = null;
            visualTreeAsset = null;
        }

        public VisualElement MakeSnapshot(VisualElement ve)
        {
            if (ve == null)
            {
                throw new ArgumentNullException();
            }

            var rect = ve.worldBound;
            var snapshotVe = new VisualElement();
            snapshotVe.name = (string.IsNullOrEmpty(ve.name) ? ve.GetType().Name.ToLower() : ve.name) + "-snapshot";
            snapshotVe.style.width = rect.width;
            snapshotVe.style.height = rect.height;
            snapshotVe.style.SetPosition(new StylePosition() { position = Position.Absolute, left = rect.x, top = rect.y });

            var texture = TextureEditor.Crop(this.texture, rect);
            snapshotVe.style.backgroundImage = texture;
            return snapshotVe;
        }
    }
}
