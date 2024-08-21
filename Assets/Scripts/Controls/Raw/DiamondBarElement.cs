using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class DiamondBarElement : VisualElement
    {
        const string k_UssClassName = "diamond-bar-element";
        const string k_EdgeUssClassName = k_UssClassName + "__edge";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";

        public new class UxmlFactory : UxmlFactory<DiamondBarElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        VisualElement m_LeftEdge;
        DiamondTiled m_Diamond;
        VisualElement m_RightEdge;
        float m_Size;

        public float size
        {
            get => m_Size;
            set
            {
                m_Size = Mathf.Max(0, value);
                m_Diamond.style.width = m_Size;
                m_Diamond.style.height = m_Size;
            }
        }

        public bool leftEdgeDisplayed
        {
            get => m_LeftEdge.style.display == DisplayStyle.Flex;
            set => m_LeftEdge.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public bool rightEdgeDisplayed
        {
            get => m_RightEdge.style.display == DisplayStyle.Flex;
            set => m_RightEdge.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public DiamondTiled diamond
        {
            get => m_Diamond;
        }

        public DiamondBarElement()
        {
            AddToClassList(k_UssClassName);

            m_LeftEdge = new VisualElement() { name = "left-edge" };
            m_LeftEdge.AddToClassList(k_EdgeUssClassName);
            Add(m_LeftEdge);

            m_Diamond = new DiamondTiled() { name = "diamond" };
            m_Diamond.AddToClassList(k_DiamondUssClassName);
            Add(m_Diamond);

            m_RightEdge = new VisualElement() { name = "right-edge" };
            m_RightEdge.AddToClassList(k_EdgeUssClassName);
            Add(m_RightEdge);

            leftEdgeDisplayed = true;
            rightEdgeDisplayed = true;
        }
    }
}
