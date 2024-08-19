using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class ScrollBarVertical : VisualElement
    {
        const string k_UssClassName = "scroll-bar";
        const string k_DraggerContainerUssClassName = k_UssClassName + "__dragger-container";
        const string k_DraggerUssClassName = k_UssClassName + "__dragger";
        const string k_LineUssClassName = k_UssClassName + "__line";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_TopVariantDiamondUssClassName = k_DiamondUssClassName + "--top";
        const string k_BottomVariantDiamondUssClassName = k_DiamondUssClassName + "--bottom";
        const string k_DraggerBackgroundUssClassName = k_DraggerUssClassName + "__background";
        const string k_ActiveVariantDraggerBackgroundUssClassName = k_DraggerBackgroundUssClassName + "--active";

        public new class UxmlFactory : UxmlFactory<ScrollBarVertical, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public Action offsetChanged;

        VisualElement m_DraggerContainer;
        VisualElement m_Dragger;
        VisualElement m_DraggerBackground;
        bool m_PointerCaptured;
        float m_NormalizedOffset;
        bool m_PointerOver;

        public bool pointerCaptured
        {
            get => m_PointerCaptured;
        }

        float maxOffset
        {
            get => m_DraggerContainer.layout.height - m_Dragger.layout.height;
        }

        public VisualElement dragger
        {
            get => m_Dragger;
        }

        public float normalizedOffset
        {
            get => m_NormalizedOffset;
            set
            {
                var previousNormalizedOffset = normalizedOffset;
                m_NormalizedOffset = Mathf.Clamp01(value);
                if (m_NormalizedOffset != previousNormalizedOffset)
                {
                    offsetChanged?.Invoke();
                }

                var position = new Vector3(0f, maxOffset * m_NormalizedOffset, 0f);
                m_Dragger.transform.position = position;
            }
        }

        public ScrollBarVertical()
        {
            AddToClassList(k_UssClassName);

            var line = new VisualElement() { name = "line" };
            line.AddToClassList(k_LineUssClassName);
            Add(line);

            var topDiamond = new VisualElement() { name = "diamond-top" };
            topDiamond.AddToClassList(k_DiamondUssClassName);
            topDiamond.AddToClassList(k_TopVariantDiamondUssClassName);
            Add(topDiamond);

            var bottomDiamond = new VisualElement() { name = "bottom-diamond" };
            bottomDiamond.AddToClassList(k_DiamondUssClassName);
            bottomDiamond.AddToClassList(k_BottomVariantDiamondUssClassName);
            Add(bottomDiamond);

            m_DraggerContainer = new VisualElement() { name = "dragger-container" };
            m_DraggerContainer.AddToClassList(k_DraggerContainerUssClassName);
            Add(m_DraggerContainer);

            m_Dragger = new VisualElement() { name = "dragger" };
            m_Dragger.AddToClassList(k_DraggerUssClassName);
            m_DraggerContainer.Add(m_Dragger);

            m_DraggerBackground = new VisualElement() { name = "dragger-background" };
            m_DraggerBackground.AddToClassList(k_DraggerBackgroundUssClassName);
            m_Dragger.Add(m_DraggerBackground);

            m_Dragger.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            m_Dragger.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            m_Dragger.RegisterCallback<PointerDownEvent>(OnPointerDown);
            m_Dragger.RegisterCallback<PointerUpEvent>(OnPointerUp);
            m_Dragger.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_Dragger.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_DraggerBackground.AddToClassList(k_ActiveVariantDraggerBackgroundUssClassName);
            m_PointerOver = true;
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (!m_PointerCaptured)
            {
                m_DraggerBackground.RemoveFromClassList(k_ActiveVariantDraggerBackgroundUssClassName);
            }

            m_PointerOver = false;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            PointerCaptureHelper.CapturePointer(m_Dragger, evt.pointerId);
            m_PointerCaptured = true;
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            PointerCaptureHelper.ReleasePointer(m_Dragger, evt.pointerId);
            m_PointerCaptured = false;
            if (!m_PointerOver)
            {
                m_DraggerBackground.RemoveFromClassList(k_ActiveVariantDraggerBackgroundUssClassName);
            }
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (m_PointerCaptured)
            {
                var targetOffset = m_Dragger.transform.position.y;
                targetOffset += evt.deltaPosition.y;
                normalizedOffset = targetOffset / maxOffset;
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            normalizedOffset = m_NormalizedOffset;
        }
    }
}
