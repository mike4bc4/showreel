using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Controls.Raw;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("Controls")]

namespace Controls.Raw
{
    public enum ButtonDisplay
    {
        Both,
        Left,
        Right,
        LeftCenter,
        RightCenter,
    }

    class DialogBox : VisualElement
    {
        const string k_UssClassName = "dialog-box";
        const string k_ShadowClassName = k_UssClassName + "__shadow";
        const string k_BoxUssClassName = k_UssClassName + "__box";
        const string k_ScrollBoxUssClassName = k_UssClassName + "__scroll-box";
        const string k_InterfaceButtonUssClassName = "interface__button";
        const string k_ButtonContainerUssClassName = k_UssClassName + "__button-container";
        const string k_ButtonContainerRightVariantUssClassName = k_ButtonContainerUssClassName + "--right";
        const string k_ButtonContainerCenterVariantUssClassName = k_ButtonContainerUssClassName + "--center";

        public new class UxmlFactory : UxmlFactory<DialogBox, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlEnumAttributeDescription<ButtonDisplay> m_ButtonDisplay = new UxmlEnumAttributeDescription<ButtonDisplay>()
            {
                name = "button-display",
                defaultValue = ButtonDisplay.Both
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var dialogBox = (DialogBox)ve;
                dialogBox.buttonDisplay = m_ButtonDisplay.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_Shadow;
        VisualElement m_Box;
        DiamondTitle m_Title;
        ScrollBox m_ScrollBox;
        VisualElement m_ButtonContainer;
        Button m_LeftButton;
        Label m_LeftButtonLabel;
        Button m_RightButton;
        Label m_RightButtonLabel;
        ButtonDisplay m_ButtonDisplay;

        public DiamondTitle title => m_Title;
        public VisualElement shadow => m_Shadow;

        public ButtonDisplay buttonDisplay
        {
            get => m_ButtonDisplay;
            set
            {
                m_ButtonDisplay = value;
                switch (m_ButtonDisplay)
                {
                    case ButtonDisplay.Both:
                    case ButtonDisplay.Left:
                        m_ButtonContainer.RemoveFromClassList(k_ButtonContainerCenterVariantUssClassName);
                        m_ButtonContainer.RemoveFromClassList(k_ButtonContainerRightVariantUssClassName);
                        break;
                    case ButtonDisplay.Right:
                        m_ButtonContainer.RemoveFromClassList(k_ButtonContainerCenterVariantUssClassName);
                        m_ButtonContainer.AddToClassList(k_ButtonContainerRightVariantUssClassName);
                        break;
                    case ButtonDisplay.LeftCenter:
                    case ButtonDisplay.RightCenter:
                        m_ButtonContainer.AddToClassList(k_ButtonContainerCenterVariantUssClassName);
                        m_ButtonContainer.RemoveFromClassList(k_ButtonContainerRightVariantUssClassName);
                        break;
                }

                bool enableLeftButton = m_ButtonDisplay == ButtonDisplay.Both || m_ButtonDisplay == ButtonDisplay.Left || m_ButtonDisplay == ButtonDisplay.LeftCenter;
                m_LeftButton.SetEnabled(enableLeftButton);
                m_LeftButton.style.display = enableLeftButton ? DisplayStyle.Flex : DisplayStyle.None;

                bool enableRightButton = m_ButtonDisplay == ButtonDisplay.Both || m_ButtonDisplay == ButtonDisplay.Right || m_ButtonDisplay == ButtonDisplay.RightCenter;
                m_RightButton.SetEnabled(enableRightButton);
                m_RightButton.style.display = enableRightButton ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public Button rightButton
        {
            get => m_RightButton;
        }

        public Button leftButton
        {
            get => m_LeftButton;
        }

        public override VisualElement contentContainer
        {
            get
            {
                return m_ScrollBox.contentContainer;
            }
        }

        public string leftButtonLabel
        {
            get => m_LeftButtonLabel.text;
            set => m_LeftButtonLabel.text = value;
        }

        public string rightButtonLabel
        {
            get => m_RightButtonLabel.text;
            set => m_RightButtonLabel.text = value;
        }

        public string titleLabel
        {
            get => m_Title.text;
            set => m_Title.text = value;
        }

        public DialogBox()
        {
            AddToClassList(k_UssClassName);

            m_Shadow = new VisualElement();
            m_Shadow.name = "shadow";
            m_Shadow.AddToClassList(k_ShadowClassName);
            hierarchy.Add(m_Shadow);

            m_Box = new VisualElement();
            m_Box.name = "box";
            m_Box.AddToClassList(k_BoxUssClassName);
            m_Shadow.Add(m_Box);

            m_Title = new DiamondTitle();
            m_Title.name = "title";
            m_Title.animationProgress = 1f;
            m_Box.Add(m_Title);

            m_ScrollBox = new ScrollBox();
            m_ScrollBox.name = "scroll-box";
            m_ScrollBox.scrollMode = ScrollMode.Smooth;
            m_ScrollBox.AddToClassList(k_ScrollBoxUssClassName);
            m_Box.Add(m_ScrollBox);

            m_ButtonContainer = new VisualElement();
            m_ButtonContainer.name = "buttons-container";
            m_ButtonContainer.AddToClassList(k_ButtonContainerUssClassName);
            m_Box.Add(m_ButtonContainer);

            m_LeftButton = new Button();
            m_LeftButton.name = "button";
            m_LeftButton.AddToClassList(k_InterfaceButtonUssClassName);
            m_ButtonContainer.Add(m_LeftButton);

            m_LeftButtonLabel = new Label();
            m_LeftButtonLabel.name = "text";
            m_LeftButtonLabel.text = "Left Button";
            m_LeftButton.Add(m_LeftButtonLabel);

            var leftButtonBorder = new VisualElement();
            leftButtonBorder.name = "border";
            m_LeftButton.Add(leftButtonBorder);

            m_RightButton = new Button();
            m_RightButton.name = "button";
            m_RightButton.AddToClassList(k_InterfaceButtonUssClassName);
            m_ButtonContainer.Add(m_RightButton);

            m_RightButtonLabel = new Label();
            m_RightButtonLabel.name = "text";
            m_RightButtonLabel.text = "Right Button";
            m_RightButton.Add(m_RightButtonLabel);

            var rightButtonBorder = new VisualElement();
            rightButtonBorder.name = "border";
            m_RightButton.Add(rightButtonBorder);
        }
    }
}