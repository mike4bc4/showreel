using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIDocumentController : MonoBehaviour
    {
        UIDocument m_UIDocument;
        DiamondTitle m_DiamondLineHorizontal;

        void OnEnable()
        {
            m_UIDocument = GetComponent<UIDocument>();
            if (m_UIDocument == null)
            {
                Debug.LogError("No UIComponent attached.");
                return;
            }

            m_DiamondLineHorizontal = m_UIDocument.rootVisualElement.Q<DiamondTitle>();

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                m_DiamondLineHorizontal.Unfold();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                m_DiamondLineHorizontal.Fold();
            }
        }
    }
}
