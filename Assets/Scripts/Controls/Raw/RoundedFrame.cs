using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class RoundedFrame : VisualElement
    {
        const string k_UssClassName = "rounded-frame";
        const string k_TopBorderContainerUssClassName = k_UssClassName + "__top-border-container";
        const string k_TopBorderUssClassName = k_UssClassName + "__top-border";
        const string k_TopRightCornerContainerUssClassName = k_UssClassName + "__top-right-corner-container";
        const string k_TopRightCornerUssClassName = k_UssClassName + "__top-right-corner";
        const string k_RightBorderContainerUssClassName = k_UssClassName + "__right-border-container";
        const string k_RightBorderUssClassName = k_UssClassName + "__right-border";
        const string k_BottomRightCornerContainerUssClassName = k_UssClassName + "__bottom-right-corner-container";
        const string k_BottomRightCornerUssClassName = k_UssClassName + "__bottom-right-corner";
        const string k_BottomBorderContainerUssClassName = k_UssClassName + "__bottom-border-container";
        const string k_BottomBorderUssClassName = k_UssClassName + "__bottom-border";

        const float k_DefaultFill = 0.5f;
        const int k_DefaultCornerRadius = 10;
        const int k_DefaultBorderWidth = 2;

        public new class UxmlFactory : UxmlFactory<RoundedFrame, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_Fill = new UxmlFloatAttributeDescription() { name = "fill", defaultValue = k_DefaultFill };
            UxmlIntAttributeDescription m_CornerRadius = new UxmlIntAttributeDescription() { name = "corner-radius", defaultValue = k_DefaultCornerRadius };
            UxmlIntAttributeDescription m_BorderWidth = new UxmlIntAttributeDescription() { name = "border-width", defaultValue = k_DefaultBorderWidth };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                RoundedFrame roundedFrame = (RoundedFrame)ve;
                roundedFrame.fill = m_Fill.GetValueFromBag(bag, cc);
                roundedFrame.cornerRadius = m_CornerRadius.GetValueFromBag(bag, cc);
                roundedFrame.borderWidth = m_BorderWidth.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_TopBorderContainer;
        VisualElement m_TopBorder;
        VisualElement m_TopRightCornerContainer;
        VisualElement m_TopRightCorner;
        VisualElement m_RightBorderContainer;
        VisualElement m_RightBorder;
        VisualElement m_BottomRightCornerContainer;
        VisualElement m_BottomRightCorner;
        VisualElement m_BottomBorderContainer;
        VisualElement m_BottomBorder;
        float m_Fill;
        int m_CornerRadius;
        int m_BorderWidth;

        float cornerLength
        {
            get => (2f * Mathf.PI * m_CornerRadius) / 4f;
        }

        float horizontalBorderLength
        {
            get => layout.width - m_CornerRadius;
        }

        float verticalBorderLength
        {
            get => layout.height - (2f * m_CornerRadius);
        }

        float frameLength
        {
            get => horizontalBorderLength * 2f + verticalBorderLength + cornerLength * 2f;
        }

        public float fill
        {
            get => m_Fill;
            set
            {
                m_Fill = Mathf.Clamp01(value);
                if (layout.size.IsNaN())
                {
                    return;
                }

                float l = m_Fill * frameLength;

                m_TopBorder.style.width = Length.Percent(Mathf.Clamp01(l / horizontalBorderLength) * 100f);
                l = Mathf.Max(0f, l - horizontalBorderLength);

                m_TopRightCorner.style.rotate = new Rotate(Mathf.Clamp01(l / cornerLength) * 90f - 45f);
                l = Mathf.Max(0f, l - cornerLength);

                m_RightBorder.style.height = Length.Percent(Mathf.Clamp01(l / verticalBorderLength) * 100f);
                l = Mathf.Max(0f, l - verticalBorderLength);

                m_BottomRightCorner.style.rotate = new Rotate(Mathf.Clamp01(l / cornerLength) * 90f - 45f);
                l = Mathf.Max(0f, l - cornerLength);

                m_BottomBorder.style.width = Length.Percent(Mathf.Clamp01(l / horizontalBorderLength) * 100f);
            }
        }

        public int cornerRadius
        {
            get => m_CornerRadius;
            set
            {
                m_CornerRadius = Mathf.Max(1, value);
                m_TopBorderContainer.style.marginRight = m_CornerRadius - 1;
                m_BottomBorderContainer.style.marginRight = m_CornerRadius - 1;
                m_RightBorderContainer.style.marginTop = m_CornerRadius - 1;
                m_RightBorderContainer.style.marginBottom = m_CornerRadius - 1;
                m_TopRightCornerContainer.style.width = m_CornerRadius;
                m_TopRightCornerContainer.style.height = m_CornerRadius;
                m_BottomRightCornerContainer.style.width = m_CornerRadius;
                m_BottomRightCornerContainer.style.height = m_CornerRadius;
            }
        }

        public int borderWidth
        {
            get => m_BorderWidth;
            set
            {
                m_BorderWidth = Mathf.Max(1, value);
                m_TopBorderContainer.style.height = m_BorderWidth;
                m_RightBorderContainer.style.width = m_BorderWidth;
                m_BottomBorderContainer.style.height = m_BorderWidth;
                m_TopRightCorner.style.borderTopWidth = m_BorderWidth;
                m_TopRightCorner.style.borderLeftWidth = m_BorderWidth;
                m_TopRightCorner.style.borderRightWidth = m_BorderWidth;
                m_TopRightCorner.style.borderBottomWidth = m_BorderWidth;
                m_BottomRightCorner.style.borderTopWidth = m_BorderWidth;
                m_BottomRightCorner.style.borderLeftWidth = m_BorderWidth;
                m_BottomRightCorner.style.borderRightWidth = m_BorderWidth;
                m_BottomRightCorner.style.borderBottomWidth = m_BorderWidth;
            }
        }

        public RoundedFrame()
        {
            AddToClassList(k_UssClassName);

            m_TopBorderContainer = new VisualElement() { name = "top-border-container" };
            m_TopBorderContainer.AddToClassList(k_TopBorderContainerUssClassName);
            Add(m_TopBorderContainer);

            m_TopBorder = new VisualElement() { name = "top-border" };
            m_TopBorder.AddToClassList(k_TopBorderUssClassName);
            m_TopBorderContainer.Add(m_TopBorder);

            m_TopRightCornerContainer = new VisualElement() { name = "top-right-corner-container" };
            m_TopRightCornerContainer.AddToClassList(k_TopRightCornerContainerUssClassName);
            Add(m_TopRightCornerContainer);

            m_TopRightCorner = new VisualElement() { name = "top-right-corner" };
            m_TopRightCorner.AddToClassList(k_TopRightCornerUssClassName);
            m_TopRightCornerContainer.Add(m_TopRightCorner);

            m_RightBorderContainer = new VisualElement() { name = "right-border-container" };
            m_RightBorderContainer.AddToClassList(k_RightBorderContainerUssClassName);
            Add(m_RightBorderContainer);

            m_RightBorder = new VisualElement() { name = "right-border" };
            m_RightBorder.AddToClassList(k_RightBorderUssClassName);
            m_RightBorderContainer.Add(m_RightBorder);

            m_BottomRightCornerContainer = new VisualElement() { name = "bottom-right-corner-container" };
            m_BottomRightCornerContainer.AddToClassList(k_BottomRightCornerContainerUssClassName);
            Add(m_BottomRightCornerContainer);

            m_BottomRightCorner = new VisualElement() { name = "bottom-right-corner" };
            m_BottomRightCorner.AddToClassList(k_BottomRightCornerUssClassName);
            m_BottomRightCornerContainer.Add(m_BottomRightCorner);

            m_BottomBorderContainer = new VisualElement() { name = "bottom-border-container" };
            m_BottomBorderContainer.AddToClassList(k_BottomBorderContainerUssClassName);
            Add(m_BottomBorderContainer);

            m_BottomBorder = new VisualElement() { name = "bottom-border" };
            m_BottomBorder.AddToClassList(k_BottomBorderUssClassName);
            m_BottomBorderContainer.Add(m_BottomBorder);

            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                fill = m_Fill;
            });
        }
    }
}
