using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI
{
    public class Layer2 : BaseLayer
    {
        UIDocument m_UIDocument;

        public UIDocument uiDocument
        {
            get => m_UIDocument;
        }

        public int inputSortOrder
        {
            get => (int)uiDocument.panelSettings.sortingOrder;
            set => uiDocument.panelSettings.sortingOrder = value;
        }

        public VisualElement rootVisualElement
        {
            get => uiDocument.rootVisualElement;
        }

        public bool interactable
        {
            get => rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>().isActiveAndEnabled;
            set => rootVisualElement.GetSelectableGameObject().GetComponent<PanelEventHandler>().enabled = value;
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
            get => rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>().isActiveAndEnabled;
            set => rootVisualElement.GetSelectableGameObject().GetComponent<PanelRaycaster>().enabled = value;
        }

        public void Init(Material material, UIDocument uiDocument)
        {
            base.Init(material);
            m_UIDocument = uiDocument;
        }

        void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(uiDocument.panelSettings.targetTexture);
        }
    }
}
