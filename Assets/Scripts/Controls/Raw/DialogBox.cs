using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Controls.Raw;
using Localization;
using UI;
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

    class DialogBox : Control
    {
        const string k_UssClassName = "dialog-box";
        const string k_ShadowClassName = k_UssClassName + "__shadow";
        const string k_BoxUssClassName = k_UssClassName + "__box";
        const string k_InterfaceButtonUssClassName = "interface__button";
        const string k_ButtonContainerUssClassName = k_UssClassName + "__button-container";
        const string k_ButtonContainerRightVariantUssClassName = k_ButtonContainerUssClassName + "--right";
        const string k_ButtonContainerCenterVariantUssClassName = k_ButtonContainerUssClassName + "--center";
        const string k_ScrollBoxContainerUssClassName = k_UssClassName +"__scroll-box-container";

        public new class UxmlFactory : UxmlFactory<DialogBox, UxmlTraits> { }

        public new class UxmlTraits : Control.UxmlTraits
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

        Control m_Shadow;
        Control m_Box;
        DiamondTitle m_Title;
        ScrollBox m_ScrollBox;
        Control m_ButtonContainer;
        ButtonControl m_LeftButton;
        LocalizedLabel m_LeftButtonLabel;
        ButtonControl m_RightButton;
        LocalizedLabel m_RightButtonLabel;
        ButtonDisplay m_ButtonDisplay;

        public DiamondTitle title => m_Title;
        public Control shadow => m_Shadow;

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

        public ButtonControl rightButton
        {
            get => m_RightButton;
        }

        public ButtonControl leftButton
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

        public LocalizationAddress titleLocalizationAddress
        {
            get => m_Title.localizationAddress;
            set => m_Title.localizationAddress = value;
        }

        public LocalizationAddress leftButtonLocalizationAddress
        {
            get => m_LeftButtonLabel.localizationAddress;
            set => m_LeftButtonLabel.localizationAddress = value;
        }

        public LocalizationAddress rightButtonLocalizationAddress
        {
            get => m_RightButtonLabel.localizationAddress;
            set => m_RightButtonLabel.localizationAddress = value;
        }

        public DialogBox()
        {
            AddToClassList(k_UssClassName);

            m_Shadow = new Control();
            m_Shadow.name = "shadow";
            m_Shadow.AddToClassList(k_ShadowClassName);
            hierarchy.Add(m_Shadow);

            m_Box = new Control();
            m_Box.name = "box";
            m_Box.AddToClassList(k_BoxUssClassName);
            m_Shadow.Add(m_Box);

            m_Title = new DiamondTitle();
            m_Title.name = "title";
            m_Title.animationProgress = 1f;
            m_Box.Add(m_Title);

            var scrollBoxContainer = new Control();
            scrollBoxContainer.name = "scroll-box-container";
            scrollBoxContainer.AddToClassList(k_ScrollBoxContainerUssClassName);
            m_Box.Add(scrollBoxContainer);

            m_ScrollBox = new ScrollBox();
            m_ScrollBox.name = "scroll-box";
            m_ScrollBox.scrollMode = ScrollMode.Smooth;
            scrollBoxContainer.Add(m_ScrollBox);

            m_ButtonContainer = new Control();
            m_ButtonContainer.name = "buttons-container";
            m_ButtonContainer.AddToClassList(k_ButtonContainerUssClassName);
            m_Box.Add(m_ButtonContainer);

            m_LeftButton = new ButtonControl();
            m_LeftButton.name = "button";
            m_LeftButton.AddToClassList(k_InterfaceButtonUssClassName);
            m_ButtonContainer.Add(m_LeftButton);

            m_LeftButtonLabel = new LocalizedLabel();
            m_LeftButtonLabel.name = "text";
            m_LeftButtonLabel.text = "Left Button";
            m_LeftButton.Add(m_LeftButtonLabel);

            var leftButtonBorder = new Control();
            leftButtonBorder.name = "border";
            m_LeftButton.Add(leftButtonBorder);

            m_RightButton = new ButtonControl();
            m_RightButton.name = "button";
            m_RightButton.AddToClassList(k_InterfaceButtonUssClassName);
            m_ButtonContainer.Add(m_RightButton);

            m_RightButtonLabel = new LocalizedLabel();
            m_RightButtonLabel.name = "text";
            m_RightButtonLabel.text = "Right Button";
            m_RightButton.Add(m_RightButtonLabel);

            var rightButtonBorder = new Control();
            rightButtonBorder.name = "border";
            m_RightButton.Add(rightButtonBorder);
        }
    }
}