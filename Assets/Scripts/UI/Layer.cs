using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;

namespace UI
{
    public class Layer : LayerBase
    {
        const string k_MaskChannelsNoneKeyword = "_MASK_CHANNELS_NONE";
        const string k_MaskChannelsX8Keyword = "_MASK_CHANNELS_X8";
        const string k_MaskChannelsX16Keyword = "_MASK_CHANNELS_X16";
        const string k_MaskCoordsPropertyName = "_MaskCoords";
        const int k_MaskCoordsPropertyCount = 16;
        const string k_InvertMaskPropertyName = "_InvertMask";
        const string k_AlphaTexPropertyName = "_AlphaTex";
        const float k_BlurDisabledEpsilon = 0.1f;

        UIDocument m_UIDocument;
        List<VisualElement> m_MaskedElements;
        bool m_BlocksRaycast;
        bool m_Interactable;
        List<ElementEffectLayer> m_ElementEffectLayers;

        LocalKeyword m_MaskChannelsNoneKeyword;
        LocalKeyword m_MaskChannelsX8Keyword;
        LocalKeyword m_MaskChannelsX16Keyword;

        public bool interactable
        {
            get => m_Interactable;
            set
            {
                m_Interactable = value;
                if (active)
                {
                    rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>().enabled = m_Interactable;
                }
            }
        }

        // This actually is kind of workaround, as normally we would have to change picking mode for
        // every element in Visual Tree Asset hierarchy. It would introduce a serious flaw as
        // not only previous picking mode setting would be lost but also each new object with 
        // default picking mode would still block raycast. That's why it's way more reliable to
        // enable or disable PanelRaycaster component  This comes at cost of using reflection,
        // as RuntimePanel object which contains Visual Element hierarchy is inaccessible 
        // and hidden behind IPanel interface.
        public bool blocksRaycasts
        {
            get => m_BlocksRaycast;
            set
            {
                m_BlocksRaycast = value;
                if (active)
                {
                    rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>().enabled = m_BlocksRaycast;
                }
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            blocksRaycasts = m_BlocksRaycast;
            interactable = m_Interactable;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            var eventHandler = rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>();
            var raycaster = rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>();
            m_Interactable = eventHandler.isActiveAndEnabled;
            m_BlocksRaycast = raycaster.isActiveAndEnabled;
            eventHandler.enabled = false;
            raycaster.enabled = false;
        }

        public float inputSortOrder
        {
            get => m_UIDocument.panelSettings.sortingOrder;
            set => m_UIDocument.panelSettings.sortingOrder = value;
        }

        public VisualTreeAsset visualTreeAsset
        {
            get => m_UIDocument.visualTreeAsset;
            set
            {
                if (m_UIDocument.visualTreeAsset != value)
                {
                    m_UIDocument.visualTreeAsset = value;
                }
            }
        }

        public VisualElement rootVisualElement
        {
            get => m_UIDocument.rootVisualElement;
        }

        public bool invertMask
        {
            get => m_RawImage.material.GetInteger(k_InvertMaskPropertyName) == 1;
            set => m_RawImage.material.SetInteger(k_InvertMaskPropertyName, value ? 1 : 0);
        }

        public override void Init()
        {
            base.Init();
            m_MaskedElements = new List<VisualElement>();
            m_ElementEffectLayers = new List<ElementEffectLayer>();

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
            m_RawImage.material = Instantiate(LayerManager.BlurMaterial);
            blur = 0f;
            invertMask = false;

            m_MaskChannelsNoneKeyword = new LocalKeyword(m_RawImage.material.shader, k_MaskChannelsNoneKeyword);
            m_MaskChannelsX8Keyword = new LocalKeyword(m_RawImage.material.shader, k_MaskChannelsX8Keyword);
            m_MaskChannelsX16Keyword = new LocalKeyword(m_RawImage.material.shader, k_MaskChannelsX16Keyword);
        }

        public ElementEffectLayer AddElementEffectLayer(VisualElement ve)
        {
            if (!rootVisualElement.IsMyDescendant(ve))
            {
                throw new Exception($"'{ve}' is not descendant of '{this}'");
            }

            var effectLayer = ElementEffectLayer.Create((string.IsNullOrEmpty(ve.name) ? ve.GetType() : ve.name) + "(EffectLayer)");
            effectLayer.transform.SetParent(transform);
            effectLayer.texture = texture;
            effectLayer.trackedElement = ve;
            effectLayer.Init();
            m_ElementEffectLayers.Add(effectLayer);
            return effectLayer;
        }

        public void RemoveElementEffectLayer(VisualElement ve)
        {
            var effectLayer = GetElementEffectLayer(ve);
            if (effectLayer != null)
            {
                RemoveElementEffectLayer(effectLayer);
            }
        }

        public void RemoveElementEffectLayer(ElementEffectLayer effectLayer)
        {
            m_ElementEffectLayers.Remove(effectLayer);
            Destroy(effectLayer.gameObject);
        }

        public ElementEffectLayer GetElementEffectLayer(VisualElement ve)
        {
            foreach (var effectLayer in m_ElementEffectLayers)
            {
                if (effectLayer.trackedElement == ve)
                {
                    return effectLayer;
                }
            }

            return null;
        }

        public override void ResetLayer()
        {
            base.ResetLayer();
            invertMask = false;
            foreach (var layer in m_ElementEffectLayers)
            {
                RemoveElementEffectLayer(layer);
            }

            // It's important to unmask before UIDocument is modified, otherwise
            // panel becomes dirty and unmask fails because of unknown panel size
            // which returns NaN vector during recalculation phase.
            UnmaskElements();

            visualTreeAsset = null;
            rootVisualElement.Clear();
        }

        public Layer CreateSnapshotLayer(VisualElement ve, string layerName)
        {
            var rect = ve.worldBound;
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height))
            {
                throw new Exception($"'{ve.GetType()} {ve.name}' world bound rect dimensions are unknown, probably its layout still have to be recalculated.");
            }

            var snapshot = new VisualElement();
            snapshot.name = "snapshot";
            snapshot.style.width = rect.width;
            snapshot.style.height = rect.height;
            snapshot.style.SetPosition(new StylePosition() { position = Position.Absolute, left = rect.x, top = rect.y });
            snapshot.style.backgroundImage = TextureEditor.Crop(this.texture, ve.worldBound);

            var layer = LayerManager.CreateLayer(layerName);
            layer.rootVisualElement.Add(snapshot);
            layer.displaySortOrder = displaySortOrder + 1;
            return layer;
        }

        public void UnmaskElements()
        {
            var dirty = m_MaskedElements.Count > 0;
            m_MaskedElements.Clear();
            if (dirty)
            {
                UpdateMask();
            }
        }

        public void UnmaskElements(params VisualElement[] visualElements)
        {
            bool dirty = false;
            foreach (var ve in visualElements)
            {
                if (m_MaskedElements.Remove(ve))
                {
                    dirty = true;
                }
            }

            if (dirty)
            {
                UpdateMask();
            }
        }

        public void MaskElements(params VisualElement[] visualElements)
        {
            var dirty = false;
            foreach (var ve in visualElements)
            {
                if (!m_MaskedElements.Contains(ve))
                {
                    m_MaskedElements.Add(ve);
                    dirty = true;
                }
            }

            if (dirty)
            {
                UpdateMask();
            }
        }

        void UpdateMask()
        {
            if (rootVisualElement.layout.size.IsNan())
            {
                throw new Exception("Layer dimensions are unknown, probably its layout still have to be recalculated.");
            }

            var rects = new List<Rect>();
            for (int i = 0; i < m_MaskedElements.Count; i++)
            {
                var ve = m_MaskedElements[i];
                if (ve == null || ve.panel == null)
                {
                    m_MaskedElements.RemoveAt(i);
                    i--;
                }
                else
                {
                    rects.Add(ve.worldBound);
                }
            }

            var w = rootVisualElement.layout.width;
            var h = rootVisualElement.layout.height;
            for (int i = 0; i < k_MaskCoordsPropertyCount; i++)
            {
                var maskCoords = i < rects.Count
                    ? new Vector4(rects[i].xMin / w, 1 - rects[i].yMax / h, rects[i].xMax / w, 1 - rects[i].yMin / h)
                    : new Vector4(-1f, -1f, -1f, -1f);
                m_RawImage.material.SetVector(k_MaskCoordsPropertyName + i, maskCoords);
            }

            m_RawImage.material.SetKeyword(m_MaskChannelsNoneKeyword, rects.Count == 0);
            m_RawImage.material.SetKeyword(m_MaskChannelsX8Keyword, rects.Count > 0 && rects.Count <= 8);
            m_RawImage.material.SetKeyword(m_MaskChannelsX16Keyword, rects.Count > 8);
        }
    }
}
