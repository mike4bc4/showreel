using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

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

        public event Action onChoiceChanged;

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
            set => SetIndexInternal(value);
        }

        public string choice
        {
            get => (0 <= m_Index && m_Index < m_Choices.Count) ? m_Choices[m_Index] : null;
            set => SetIndexInternal(m_Choices.IndexOf(value));
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

        void SetIndexInternal(int index)
        {
            var previousIndex = m_Index;
            m_Index = Mathf.Clamp(index, -1, m_Choices.Count - 1);
            m_OptionButtonLabel.text = m_Index >= 0 ? m_Choices[m_Index] : null;
            if (m_Index != previousIndex)
            {
                onChoiceChanged?.Invoke();
            }
        }

        void AttachScrollBoxOverlay()
        {
            var root = this.GetRootVisualElement();
            if (root == null)
            {
                return;
            }

            root.Add(m_ScrollBoxOverlay);

            // When ScrollBox is attached to the panel for the first time its layout properties are
            // NaN and are useless until GeometryChangedEvent is invoked. Even though we could simply 
            // wait for such event, it introduces another problem of not knowing whether geometry has 
            // actually been changed thus it's not possible to detect if callback can be invoked 
            // synchronously. The major issue here is that any further changes of geometry are not
            // resetting layout properties to NaN, so simple 'if NaN' will not work. What's even more
            // annoying layout will return old values until next frame, that's why we are making use
            // of scheduler and delayed calls in editor mode.
            switch (m_ScrollBoxOverlay.panel.contextType)
            {
                case ContextType.Player:
                    Scheduler.delayCall += FocusCurrentChoice;
                    break;
#if UNITY_EDITOR
                case ContextType.Editor:
                    UnityEditor.EditorApplication.delayCall += FocusCurrentChoice;
                    break;
#endif
            }

            UpdateListPosition();
            m_ListPositionUpdater = m_ScrollBoxContainer.schedule.Execute(UpdateListPosition).Every(k_ListPositionUpdateInterval);
        }

        void FocusCurrentChoice()
        {
            if (m_Choices.Count > 1)
            {
                var c = (float)m_Choices.Count;
                var h = m_ScrollBox.contentContainer.localBound.height;
                var v = m_ScrollBox.viewport.localBound.height;

                // This is amount of choices that can be focused by ScrollBox viewport. The higher 
                // it is, the less choices can be focused.
                var n = (h - v) / h * c;

                if (n > 0)
                {
                    // We have to remap ScrollBox offset using steeper linear function, here we are
                    // calculating it's slope factor (tangent of slope angle).
                    var tg = (c - 1f) / n;

                    var t = m_Index / (c - 1f);
                    var f = (tg - 1f) / 2f;
                    m_ScrollBox.normalizedOffset = Mathf.Lerp(-f, 1f + f, t);
                }
            }
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

        public void DetachScrollBoxOverlay()
        {
            m_ScrollBoxOverlay.RemoveFromHierarchy();
            m_ListPositionUpdater.Pause();
            m_ListPositionUpdater = null;
        }

        public void SetChoices(string choicesString)
        {
            SetChoices(choicesString.Split(',').Select(c => c.Trim()).ToList());
        }

        public void SetChoices(params string[] choices)
        {
            SetChoices(choices.ToList());
        }

        public void SetChoices(List<string> choices)
        {
            m_ScrollBoxContentWrapper.Clear();
            m_Choices = choices.Where(ch => !string.IsNullOrEmpty(ch)).Distinct().ToList();
            for (int i = 0; i < m_Choices.Count; i++)
            {
                var button = CreateChoiceButton(i, m_Choices[i]);
                m_ScrollBoxContentWrapper.Add(button);
            }

            SetIndexInternal(m_Index);
        }

        Button CreateChoiceButton(int index, string choice)
        {
            var button = new Button() { name = "button-" + index };
            button.AddToClassList(k_ButtonUssClassName);

            var label = new Label()
            {
                name = "label",
                text = choice
            };
            
            button.Add(label);

            var border = new VisualElement() { name = "border" };
            button.Add(border);

            button.clicked += () =>
            {
                SetIndexInternal(index);
                DetachScrollBoxOverlay();
            };

            return button;
        }
    }
}
