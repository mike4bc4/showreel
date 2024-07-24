using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;

namespace UI
{
    public class Layer : LayerBase
    {
        public const float DefaultBlurSize = 8f;

        const string k_AlphaTexPropertyName = "_AlphaTex";
        const string k_QualityPropertyName = "_Quality";
        const string k_BlurSizePropertyName = "_Size";
        const string k_BlurOnKeyword = "BLUR_ON";
        const float k_BlurDisabledEpsilon = 0.1f;

        UIDocument m_UIDocument;
        List<VisualElement> m_MaskedElements;
        bool m_BlurEnabled;
        bool m_BlocksInput;
        Dictionary<VisualElement, PickingMode> m_ElementPickingModes;

        public bool interactable
        {
            get => rootVisualElement.enabledSelf;
            set => rootVisualElement.SetEnabled(value);
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
            get
            {
                var panelRaycaster = rootVisualElement?.GetSelectableGameObject()?.GetComponent<PanelRaycaster>();
                if (panelRaycaster == null)
                {
                    throw new Exception($"Unable to acquire '{typeof(PanelRaycaster)}' component.");
                }

                return panelRaycaster.isActiveAndEnabled;
            }
            set
            {
                var panelRaycaster = rootVisualElement?.GetSelectableGameObject()?.GetComponent<PanelRaycaster>();
                if (panelRaycaster == null)
                {
                    throw new Exception($"Unable to acquire '{typeof(PanelRaycaster)}' component.");
                }

                panelRaycaster.enabled = value;
            }
        }

        public float panelSortingOrder
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
            m_MaskedElements = new List<VisualElement>();
            m_ElementPickingModes = new Dictionary<VisualElement, PickingMode>();

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
            blurSize = 0f;
        }

        public override void ResetLayer()
        {
            base.ResetLayer();
            visualTreeAsset = null;
            blurSize = 0f;
            Unmask();
        }

        public Layer CreateSnapshotLayer(VisualElement ve, string layerName)
        {
            var rect = ve.worldBound;
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height))
            {
                throw new Exception($"'{ve}' world bound rect dimensions are unknown, probably its layout still have to be recalculated.");
            }

            var snapshot = new VisualElement();
            snapshot.style.width = rect.width;
            snapshot.style.height = rect.height;
            snapshot.style.SetPosition(new StylePosition() { position = Position.Absolute, left = rect.x, top = rect.y });

            // Texture will be assigned at the end of frame, when UI frame has already been generated.
            new Func<UniTask>(async () =>
            {
                await UniTask.WaitForEndOfFrame(this);
                if (ve == null || snapshot == null)
                {
                    return;
                }

                snapshot.style.backgroundImage = TextureEditor.Crop(this.texture, ve.worldBound);
            })();

            var layer = LayerManager.CreateLayer(layerName);
            layer.rootVisualElement.Add(snapshot);
            return layer;
        }

        public async UniTask<Layer> CreateSnapshotLayerAsync(VisualElement ve, string layerName, CancellationToken ct)
        {
            var rect = ve.worldBound;
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height))
            {
                throw new Exception($"'{ve}' world bound rect dimensions are unknown, probably its layout still have to be recalculated.");
            }

            var snapshot = new VisualElement();
            snapshot.name = "snapshot";
            snapshot.style.width = rect.width;
            snapshot.style.height = rect.height;
            snapshot.style.SetPosition(new StylePosition() { position = Position.Absolute, left = rect.x, top = rect.y });

            var layer = LayerManager.CreateLayer(layerName);
            layer.rootVisualElement.Add(snapshot);

            try
            {
                await UniTask.WaitForEndOfFrame(this, ct);    // Wait until all UI changes are applied and snapshot background texture can be created.
            }
            catch (OperationCanceledException)
            {
                LayerManager.RemoveLayer(layer);
                throw;
            }

            snapshot.style.backgroundImage = TextureEditor.Crop(this.texture, ve.worldBound);
            await UniTask.WaitForEndOfFrame(this, ct);  // Wait yet again until snapshot background is applied.
            return layer;
        }

        public void Unmask()
        {
            var dirty = m_MaskedElements.Count > 0;
            m_MaskedElements.Clear();
            if (dirty)
            {
                UpdateAlphaTexture();
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
                UpdateAlphaTexture();
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
                UpdateAlphaTexture();
            }
        }

        Texture CreateAlphaTexture()
        {
            var rects = new List<Rect>();
            for (int i = 0; i < m_MaskedElements.Count; i++)
            {
                var ve = m_MaskedElements[i];
                if (ve == null || ve.panel == null)
                {
                    m_MaskedElements.RemoveAt(i);
                    i--;
                }

                rects.Add(ve.worldBound);
            }

            return TextureEditor.CreateMask(rootVisualElement.layout.size, invert: true, rects.ToArray());
        }

        void UpdateAlphaTexture()
        {
            if (m_RawImage.material == null)
            {
                m_RawImage.material = Instantiate(LayerManager.MaskMaterial);
            }

            if (!m_RawImage.material.HasTexture(k_AlphaTexPropertyName))
            {
                throw new Exception($"'{m_RawImage.material}' does not have '{k_AlphaTexPropertyName}' property thus it does not support element masking.");
            }

            m_RawImage.material.SetTexture(k_AlphaTexPropertyName, m_MaskedElements.Count > 0 ? CreateAlphaTexture() : null);
        }
    }
}
