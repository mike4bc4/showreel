using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public enum ScrollMode
    {
        Immediate,
        Smooth,
    }

    public class ScrollBox : VisualElement
    {
        const string k_UssClassName = "scroll-box";
        const string k_ViewportContainerUssClassName = k_UssClassName + "__viewport-container";
        const string k_ViewportUssClassName = k_UssClassName + "__viewport";
        const string k_ContentContainerUssClassName = k_UssClassName + "__content-container";
        const long k_AnimationInterval = 16L;
        const float k_ScrollStopEpsilon = 0.001f;

        public new class UxmlFactory : UxmlFactory<ScrollBox, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription m_ScrolledLines = new UxmlIntAttributeDescription() { name = "scrolled-lines", defaultValue = 3 };
            UxmlIntAttributeDescription m_LineHeight = new UxmlIntAttributeDescription() { name = "line-height", defaultValue = 24 };
            UxmlFloatAttributeDescription m_Deceleration = new UxmlFloatAttributeDescription() { name = "deceleration", defaultValue = 8f };
            UxmlEnumAttributeDescription<ScrollMode> m_ScrollMode = new UxmlEnumAttributeDescription<ScrollMode>() { name = "scroll-mode", defaultValue = ScrollMode.Immediate };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ScrollBox scrollBox = (ScrollBox)ve;
                scrollBox.scrolledLines = m_ScrolledLines.GetValueFromBag(bag, cc);
                scrollBox.lineHeight = m_LineHeight.GetValueFromBag(bag, cc);
                scrollBox.deceleration = m_Deceleration.GetValueFromBag(bag, cc);
                scrollBox.scrollMode = m_ScrollMode.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_ViewportContainer;
        VisualElement m_Viewport;
        VisualElement m_ContentContainer;
        ScrollBarVertical m_ScrollBar;
        int m_LineHeight;
        float m_NormalizedOffset;
        IVisualElementScheduledItem m_ScrollBarAnimation;
        float m_TargetOffset;
        ScrollMode m_ScrollMode;
        float m_Deceleration;
        int m_ScrolledLines;

        public bool isScrollBarDisplayed
        {
            get => m_ScrollBar.style.display == DisplayStyle.Flex;
        }

        public int scrolledLines
        {
            get => m_ScrolledLines;
            set => m_ScrolledLines = Mathf.Max(1, value);
        }

        public ScrollMode scrollMode
        {
            get => m_ScrollMode;
            set
            {
                m_ScrollMode = value;
                if (m_ScrollMode == ScrollMode.Smooth)
                {
                    m_TargetOffset = m_ScrollBar.normalizedOffset;
                }
            }
        }

        public float deceleration
        {
            get => m_Deceleration;
            set
            {
                m_Deceleration = MathF.Max(1f, value);
            }
        }

        float maxOffset
        {
            get => Mathf.Max(0, contentContainer.layout.height - m_Viewport.layout.height);
        }

        public float normalizedOffset
        {
            get => m_NormalizedOffset;
            set
            {
                m_NormalizedOffset = Mathf.Clamp01(value);
                var position = new Vector3(0f, m_NormalizedOffset * maxOffset, 0f);
                contentContainer.transform.position = -position;
            }
        }

        public int lineHeight
        {
            get => m_LineHeight;
            set => m_LineHeight = Mathf.Max(1, value);
        }

        public override VisualElement contentContainer
        {
            get => m_ContentContainer;
        }

        public ScrollBox()
        {
            AddToClassList(k_UssClassName);

            m_ViewportContainer = new VisualElement() { name = "viewport-container" };
            m_ViewportContainer.AddToClassList(k_ViewportContainerUssClassName);
            hierarchy.Add(m_ViewportContainer);

            m_Viewport = new VisualElement() { name = "viewport" };
            m_Viewport.AddToClassList(k_ViewportUssClassName);
            m_ViewportContainer.Add(m_Viewport);

            m_ContentContainer = new VisualElement() { name = "content-container" };
            m_ContentContainer.AddToClassList(k_ContentContainerUssClassName);
            m_Viewport.Add(m_ContentContainer);

            m_ScrollBar = new ScrollBarVertical() { name = "scroll-bar" };
            m_ScrollBar.offsetChanged += OnScrollBarOffsetChanged;
            m_ViewportContainer.Add(m_ScrollBar);

            RegisterCallback<WheelEvent>(OnWheel);
            m_Viewport.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            contentContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            scrolledLines = 3;
            lineHeight = 24;
            deceleration = 8f;
        }

        void OnWheel(WheelEvent evt)
        {
            if (maxOffset > 0)
            {
                if (m_ScrollBarAnimation != null)
                {
                    m_ScrollBarAnimation.Pause();
                }

                var offsetFactor = (Mathf.Sign(evt.delta.y) * m_ScrolledLines * m_LineHeight) / maxOffset;
                switch (scrollMode)
                {
                    case ScrollMode.Immediate:
                        m_ScrollBar.normalizedOffset += offsetFactor;
                        break;
                    case ScrollMode.Smooth:
                        m_TargetOffset = Mathf.Clamp01(m_TargetOffset + offsetFactor);
                        m_ScrollBarAnimation = schedule.Execute(AnimateScrollBar).Every(k_AnimationInterval);
                        break;
                }
            }

            evt.StopPropagation();
        }

        void AnimateScrollBar(TimerState timerState)
        {
            m_ScrollBar.normalizedOffset = Mathf.Lerp(m_ScrollBar.normalizedOffset, m_TargetOffset, timerState.deltaTime / 1000f * deceleration);
            if (Mathf.Abs(m_ScrollBar.normalizedOffset - m_TargetOffset) < k_ScrollStopEpsilon || m_ScrollBar.pointerCaptured)
            {
                m_ScrollBarAnimation.Pause();
                m_ScrollBarAnimation = null;
            }
        }

        void OnScrollBarOffsetChanged()
        {
            normalizedOffset = m_ScrollBar.normalizedOffset;
            if (m_ScrollBar.pointerCaptured)
            {
                m_TargetOffset = normalizedOffset;
            }
        }

        void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            normalizedOffset = m_ScrollBar.normalizedOffset;
            UpdateScrollBar();
        }

        void UpdateScrollBar()
        {
            m_ScrollBar.SetEnabled(maxOffset > 0);
            m_ScrollBar.style.display = maxOffset > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            var draggerSize = m_Viewport.layout.height / Mathf.Max(1, contentContainer.layout.height);
            m_ScrollBar.dragger.style.height = Length.Percent(Mathf.Min(draggerSize, 1) * 100f);
        }
    }
}