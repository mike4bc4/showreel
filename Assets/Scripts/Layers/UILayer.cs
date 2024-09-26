using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Extensions;

namespace Layers
{
    public class UILayer : Layer
    {
        const string k_LayerRootUssClassName = "layer-root";

        UIDocument m_UIDocument;
        VisualElement m_RootVisualElement;
        int? m_InputSortOrder;

        public override bool visible
        {
            get => base.visible;
            set
            {
                // In addition to default behavior make sure that ui document root visual element is
                // empty when layer is not visible. This effectively removes panel from ui renderer
                // and no write to render texture occurs.
                var previousVisible = visible;
                base.visible = value;
                if (visible != previousVisible)
                {
                    if (!visible)
                    {
                        uiDocument.rootVisualElement.Remove(m_RootVisualElement);
                    }
                    else
                    {
                        uiDocument.rootVisualElement.Add(m_RootVisualElement);
                    }
                }
            }
        }

        public UIDocument uiDocument
        {
            get => m_UIDocument;
        }

        public override int displaySortOrder
        {
            get => base.displaySortOrder;
            set
            {
                base.displaySortOrder = value;
                if (m_InputSortOrder == null)
                {
                    uiDocument.panelSettings.sortingOrder = value;
                }
            }
        }

        public int inputSortOrder
        {
            get => (int)uiDocument.panelSettings.sortingOrder;
            set
            {
                m_InputSortOrder = value;
                uiDocument.panelSettings.sortingOrder = value;
            }
        }

        public VisualElement rootVisualElement
        {
            get => m_RootVisualElement;
        }

        public bool interactable
        {
            get => uiDocument.rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>().isActiveAndEnabled;
            set => uiDocument.rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>().enabled = value;
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
            get => uiDocument.rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>().isActiveAndEnabled;
            set => uiDocument.rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>().enabled = value;
        }

        public void Init(Material material, UIDocument uiDocument)
        {
            base.Init(material);
            m_UIDocument = uiDocument;

            m_RootVisualElement = new VisualElement();
            m_RootVisualElement.name = "layer-root";
            m_RootVisualElement.pickingMode = PickingMode.Ignore;
            m_RootVisualElement.AddToClassList(k_LayerRootUssClassName);
            uiDocument.rootVisualElement.Add(m_RootVisualElement);
        }

        public TemplateContainer AddTemplateFromVisualTreeAsset(VisualTreeAsset visualTreeAsset)
        {
            var templateContainer = visualTreeAsset.Instantiate();
            templateContainer.pickingMode = PickingMode.Ignore;
            templateContainer.style.flexGrow = 1f;
            rootVisualElement.Add(templateContainer);
            return templateContainer;
        }

        public void Clear()
        {
            var activeRenderTexture = RenderTexture.active;
            RenderTexture.active = m_UIDocument.panelSettings.targetTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = activeRenderTexture;
        }

        void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(uiDocument.panelSettings.targetTexture);
        }
    }
}
