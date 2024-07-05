using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class ListElement : VisualElement
    {
        public static readonly string ussClassName = "list-element";
        public static readonly string bulletUssClassName = ussClassName + "__bullet";
        public static readonly string headerUssClassName = ussClassName + "__header";
        public static readonly string textUssClassName = ussClassName + "__text";
        public static readonly string borderUssClassName = ussClassName + "__border";
        public static readonly string buttonUssClassName = ussClassName + "__button";
        public static readonly string textContainerUssClassName = ussClassName + "__text-container";

        public new class UxmlFactory : UxmlFactory<ListElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Header = new UxmlStringAttributeDescription() { name = "header", defaultValue = "Header" };
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Text" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ListElement listElement = (ListElement)ve;
                listElement.header = m_Header.GetValueFromBag(bag, cc);
                listElement.text = m_Text.GetValueFromBag(bag, cc);
            }
        }

        DiamondBullet m_DiamondBullet;
        Button m_Button;
        VisualElement m_TextContainer;
        VisualElement m_Border;
        Label m_Header;
        Label m_Text;

        public string header
        {
            get => m_Header.text;
            set => m_Header.text = value;
        }

        public string text
        {
            get => m_Text.text;
            set => m_Text.text = value;
        }

        public ListElement()
        {
            AddToClassList(ussClassName);

            m_DiamondBullet = new DiamondBullet() { name = "bullet" };
            m_DiamondBullet.AddToClassList(bulletUssClassName);
            Add(m_DiamondBullet);

            m_Button = new Button() { name = "button" };
            m_Button.AddToClassList(buttonUssClassName);
            Add(m_Button);

            m_TextContainer = new VisualElement() { name = "text-container" };
            m_TextContainer.AddToClassList(textContainerUssClassName);
            m_Button.Add(m_TextContainer);

            m_Border = new VisualElement() { name = "border" };
            m_Border.AddToClassList(borderUssClassName);
            m_Button.Add(m_Border);

            m_Header = new Label() { name = "header" };
            m_Header.AddToClassList(headerUssClassName);
            m_TextContainer.Add(m_Header);

            m_Text = new Label() { name = "text" };
            m_Text.AddToClassList(textUssClassName);
            m_TextContainer.Add(m_Text);
        }
    }
}
