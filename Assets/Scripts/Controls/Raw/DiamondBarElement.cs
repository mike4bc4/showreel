using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public enum BarElementEdge
    {
        Both,
        Left,
        Right,
    }

    public class DiamondBarElement : VisualElement
    {
        const string k_UssClassName = "diamond-bar-element";
        const string k_EdgeUssClassName = k_UssClassName + "__edge";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";

        public new class UxmlFactory : UxmlFactory<DiamondBarElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription m_DiamondSize = new UxmlIntAttributeDescription() { name = "diamond-size", defaultValue = 42 };
            UxmlEnumAttributeDescription<BarElementEdge> m_Edge = new UxmlEnumAttributeDescription<BarElementEdge>() { name = "edge" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBarElement diamondBarElement = (DiamondBarElement)ve;
                diamondBarElement.diamondSize = m_DiamondSize.GetValueFromBag(bag, cc);
                diamondBarElement.edge = m_Edge.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_LeftEdge;
        DiamondTiled m_Diamond;
        VisualElement m_RightEdge;
        int m_DiamondSize;
        BarElementEdge m_Edge;

        public int diamondSize
        {
            get => m_DiamondSize;
            set
            {
                m_DiamondSize = Mathf.Max(0, value);
                m_Diamond.style.width = m_DiamondSize;
                m_Diamond.style.height = m_DiamondSize;
            }
        }

        public BarElementEdge edge
        {
            get => m_Edge;
            set
            {
                m_Edge = value;
                switch (m_Edge)
                {
                    case BarElementEdge.Both:
                        m_LeftEdge.style.display = DisplayStyle.Flex;
                        m_RightEdge.style.display = DisplayStyle.Flex;
                        break;
                    case BarElementEdge.Left:
                        m_LeftEdge.style.display = DisplayStyle.Flex;
                        m_RightEdge.style.display = DisplayStyle.None;
                        break;
                    case BarElementEdge.Right:
                        m_LeftEdge.style.display = DisplayStyle.None;
                        m_RightEdge.style.display = DisplayStyle.Flex;
                        break;
                }
            }
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
        }
    }
}
