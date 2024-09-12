using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class Select : VisualElement
    {
        const string k_UssClassName = "select";
        const string k_LabelUssClassName = k_UssClassName + "__label";
        const string k_OptionContainerUssClassName = k_UssClassName + "__option-container";
        const string k_ScrollBoxOverlayUssClassName = k_UssClassName + "__scroll-box-overlay";
        const string k_ScrollBoxContainerUssClassName = k_UssClassName + "__scroll-box-container";
        const string k_ScrollBoxUssClassName = k_UssClassName + "__scroll-box";
        const string k_ScrollBoxContentWrapperUssClassName = k_UssClassName + "__scroll-box-content-wrapper";
        const string k_ButtonUssClassName = k_UssClassName + "__button";
        const string k_OptionButtonUssClassName = k_UssClassName + "__option-button";
        const long k_ListPositionUpdateInterval = 16L;

        public new class UxmlFactory : UxmlFactory<Select, UxmlTraits> { };

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription() { name = "label", defaultValue = "Select" };
            UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription() { name = "choices" };
            UxmlIntAttributeDescription m_Index = new UxmlIntAttributeDescription() { name = "index", defaultValue = -1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var select = (Select)ve;
                select.label = m_Label.GetValueFromBag(bag, cc);
                select.SetChoices(m_Choices.GetValueFromBag(bag, cc));
                select.index = m_Index.GetValueFromBag(bag, cc);
            }
        }

        Label m_Label;
        VisualElement m_OptionContainer;
        Button m_OptionButton;
        Label m_OptionButtonLabel;
        VisualElement m_ScrollBoxOverlay;
        VisualElement m_ScrollBoxContainer;
        ScrollBox m_ScrollBox;
        VisualElement m_ScrollBoxContentWrapper;
        IVisualElementScheduledItem m_ListPositionUpdater;
        int m_Index;
        List<string> m_Choices;

        public IReadOnlyList<string> choices
        {
            get => m_Choices.AsReadOnly();
        }

        public int index
        {
            get => m_Index;
            set
            {

                m_Index = Mathf.Clamp(value, -1, m_Choices.Count - 1);
                string choice = m_Choices.ElementAtOrDefault(m_Index);
                if (choice != default)
                {
                    m_OptionButtonLabel.text = choice;
                }
                else
                {
                    m_OptionButtonLabel.text = string.Empty;
                }
            }
        }

        public string label
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        public Select()
        {
            m_Choices = new List<string>();

            AddToClassList(k_UssClassName);

            m_Label = new Label();
            m_Label.name = "label";
            m_Label.AddToClassList(k_LabelUssClassName);
            Add(m_Label);

            m_OptionContainer = new VisualElement();
            m_OptionContainer.name = "option-container";
            m_OptionContainer.AddToClassList(k_OptionContainerUssClassName);
            Add(m_OptionContainer);

            m_OptionButton = new Button();
            m_OptionButton.name = "button";
            m_OptionButton.AddToClassList(k_OptionButtonUssClassName);
            m_OptionContainer.Add(m_OptionButton);

            m_OptionButtonLabel = new Label();
            m_OptionButtonLabel.name = "label";
            m_OptionButton.Add(m_OptionButtonLabel);

            var border = new VisualElement();
            border.name = "border";
            m_OptionButton.Add(border);

            var arrowIcon = new VisualElement();
            arrowIcon.name = "icon";
            m_OptionButton.Add(arrowIcon);

            m_ScrollBoxOverlay = new VisualElement();
            m_ScrollBoxOverlay.name = "list-container";
            m_ScrollBoxOverlay.AddToClassList(k_ScrollBoxOverlayUssClassName);
            m_ScrollBoxOverlay.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == m_ScrollBoxOverlay)
                {
                    DetachScrollBoxOverlay();
                }
            });

            m_ScrollBoxContainer = new VisualElement();
            m_ScrollBoxContainer.name = "scroll-box-container";
            m_ScrollBoxContainer.AddToClassList(k_ScrollBoxContainerUssClassName);
            m_ScrollBoxOverlay.Add(m_ScrollBoxContainer);

            m_ScrollBox = new ScrollBox();
            m_ScrollBox.AddToClassList(k_ScrollBoxUssClassName);
            m_ScrollBox.scrollMode = ScrollMode.Smooth;
            m_ScrollBoxContainer.Add(m_ScrollBox);

            m_ScrollBoxContentWrapper = new VisualElement();
            m_ScrollBoxContentWrapper.name = "content-wrapper";
            m_ScrollBoxContentWrapper.AddToClassList(k_ScrollBoxContentWrapperUssClassName);
            m_ScrollBox.Add(m_ScrollBoxContentWrapper);

            m_OptionContainer.RegisterCallback<ClickEvent>(evt => AttachScrollBoxOverlay());
        }

        void AttachScrollBoxOverlay()
        {
            var root = this.GetRootVisualElement();
            if (root == null)
            {
                return;
            }

            root.Add(m_ScrollBoxOverlay);
            UpdateListPosition();
            m_ListPositionUpdater = m_ScrollBoxContainer.schedule.Execute(UpdateListPosition).Every(k_ListPositionUpdateInterval);
        }

        void UpdateListPosition()
        {
            var offset = worldBound.position - m_ScrollBoxOverlay.worldBound.position;
            var scale = worldBound.width / localBound.width;

            var position = offset / scale;
            position.x += m_OptionContainer.localBound.position.x;
            position.y += localBound.height;

            m_ScrollBoxContainer.transform.position = position;
        }

        void DetachScrollBoxOverlay()
        {
            m_ScrollBoxOverlay.RemoveFromHierarchy();
            m_ListPositionUpdater.Pause();
            m_ListPositionUpdater = null;
        }


        void SetChoices(string choicesString)
        {
            SetChoices(choicesString.Split(',').Select(c => c.Trim()).ToList());
        }

        void SetChoices(params string[] choices)
        {
            SetChoices(choices.ToList());
        }

        void SetChoices(List<string> choices)
        {
            m_ScrollBoxContentWrapper.Clear();
            m_Choices.Clear();
            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                string choice = choices[i];
                m_Choices.Add(choice);

                var button = new Button();
                button.name = "button-" + i;
                button.AddToClassList(k_ButtonUssClassName);
                button.clicked += () =>
                {
                    index = idx;
                    DetachScrollBoxOverlay();
                };

                m_ScrollBoxContentWrapper.Add(button);

                var label = new Label();
                label.name = "label";
                label.text = choice;
                button.Add(label);

                var border = new VisualElement();
                border.name = "border";
                button.Add(border);
            }
        }
    }
}
