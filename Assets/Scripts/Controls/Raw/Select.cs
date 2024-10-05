using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Localization;
using UI;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace Controls.Raw
{
    public class Select : LocalizedElement
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

        public new class UxmlTraits : LocalizedElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription() { name = "label", defaultValue = "Select" };
            UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription() { name = "choices" };
            UxmlIntAttributeDescription m_Index = new UxmlIntAttributeDescription() { name = "index", defaultValue = -1 };
            UxmlFloatAttributeDescription m_OffsetX = new UxmlFloatAttributeDescription() { name = "offset-x", defaultValue = -8f };
            UxmlFloatAttributeDescription m_OffsetY = new UxmlFloatAttributeDescription() { name = "offset-y", defaultValue = -4f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var select = (Select)ve;
                select.label = m_Label.GetValueFromBag(bag, cc);
                select.SetChoices(m_Choices.GetValueFromBag(bag, cc));
                select.index = m_Index.GetValueFromBag(bag, cc);
                select.offsetX = m_OffsetX.GetValueFromBag(bag, cc);
                select.offsetY = m_OffsetY.GetValueFromBag(bag, cc);
            }
        }

        public event Action onChoiceChanged;

        LocalizedLabel m_Label;
        Control m_OptionContainer;
        ButtonControl m_OptionButton;
        LocalizedLabel m_OptionButtonLabel;
        Control m_ScrollBoxOverlay;
        Control m_ScrollBoxContainer;
        ScrollBox m_ScrollBox;
        Control m_ScrollBoxContentWrapper;
        IVisualElementScheduledItem m_ListPositionUpdater;
        int m_Index;
        List<string> m_Choices;

        float offsetX { get; set; }

        float offsetY { get; set; }

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

        protected override ILocalizedElement localizedElement => m_Label;

        public Select()
        {
            m_Choices = new List<string>();

            AddToClassList(k_UssClassName);

            m_Label = new LocalizedLabel();
            m_Label.name = "label";
            m_Label.AddToClassList(k_LabelUssClassName);
            Add(m_Label);

            m_OptionContainer = new Control();
            m_OptionContainer.name = "option-container";
            m_OptionContainer.AddToClassList(k_OptionContainerUssClassName);
            Add(m_OptionContainer);

            m_OptionButton = new ButtonControl();
            m_OptionButton.name = "button";
            m_OptionButton.AddToClassList(k_OptionButtonUssClassName);
            m_OptionContainer.Add(m_OptionButton);

            m_OptionButtonLabel = new LocalizedLabel();
            m_OptionButtonLabel.name = "label";
            m_OptionButton.Add(m_OptionButtonLabel);

            var border = new Control();
            border.name = "border";
            m_OptionButton.Add(border);

            var arrowIcon = new Control();
            arrowIcon.name = "icon";
            m_OptionButton.Add(arrowIcon);

            m_ScrollBoxOverlay = new Control();
            m_ScrollBoxOverlay.name = "list-container";
            m_ScrollBoxOverlay.AddToClassList(k_ScrollBoxOverlayUssClassName);
            m_ScrollBoxOverlay.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == m_ScrollBoxOverlay)
                {
                    DetachScrollBoxOverlay();
                }
            });

            m_ScrollBoxContainer = new Control();
            m_ScrollBoxContainer.name = "scroll-box-container";
            m_ScrollBoxContainer.AddToClassList(k_ScrollBoxContainerUssClassName);
            m_ScrollBoxOverlay.Add(m_ScrollBoxContainer);

            m_ScrollBox = new ScrollBox();
            m_ScrollBox.AddToClassList(k_ScrollBoxUssClassName);
            m_ScrollBox.scrollMode = ScrollMode.Smooth;
            m_ScrollBoxContainer.Add(m_ScrollBox);

            m_ScrollBoxContentWrapper = new Control();
            m_ScrollBoxContentWrapper.name = "content-wrapper";
            m_ScrollBoxContentWrapper.AddToClassList(k_ScrollBoxContentWrapperUssClassName);
            m_ScrollBox.Add(m_ScrollBoxContentWrapper);

            m_OptionContainer.RegisterCallback<ClickEvent>(evt => AttachScrollBoxOverlay());
        }

        void SetIndexInternal(int index)
        {
            var previousIndex = m_Index;
            m_Index = Mathf.Clamp(index, -1, m_Choices.Count - 1);

            string choice = 0 <= m_Index ? m_Choices[m_Index] : null;
            m_OptionButtonLabel.text = choice;
            if (LocalizationAddress.IsAddress(choice))
            {
                m_OptionButtonLabel.localizationAddress = choice;
                m_OptionButtonLabel.text = m_OptionButtonLabel.localizationAddress.key;
                m_OptionButtonLabel.Localize();
            }

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
                    Scheduler.delayCall += UpdateListPosition;
                    break;
#if UNITY_EDITOR
                case ContextType.Editor:
                    UnityEditor.EditorApplication.delayCall += FocusCurrentChoice;
                    UnityEditor.EditorApplication.delayCall += UpdateListPosition;
                    break;
#endif
            }

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
            var offset = m_ScrollBoxOverlay.worldBound.position - m_OptionContainer.worldBound.position;

            // Viewport scale is useful when entire root size is changed through style's translation
            // property, e.g. when user is zooming in and out in builder mode.
            var viewportScale = m_ScrollBoxOverlay.worldBound.width / m_ScrollBoxOverlay.localBound.width;

            // This is final scale of select element (after all parent transformations applied), it
            // should affect size of scroll box container.
            var localScale = (worldBound.size / localBound.size) / viewportScale;

            var position = -offset / viewportScale;
            position.y += (localBound.height + offsetY) * localScale.y;
            position.x += offsetX * localScale.x;

            m_ScrollBoxContainer.transform.position = position;
            m_ScrollBoxContainer.transform.scale = Vector2.one * localScale;
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

        ButtonControl CreateChoiceButton(int index, string choice)
        {
            var button = new ButtonControl() { name = "button-" + index };
            button.AddToClassList(k_ButtonUssClassName);

            var label = new LocalizedLabel()
            {
                name = "label",
                text = choice
            };

            if (LocalizationAddress.IsAddress(choice))
            {
                label.localizationAddress = choice;
                label.text = label.localizationAddress.key;
                label.Localize();
            }

            button.Add(label);

            var border = new Control() { name = "border" };
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
