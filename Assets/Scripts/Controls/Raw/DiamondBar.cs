using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Extensions;

namespace Controls.Raw
{
    public class DiamondBar : VisualElement
    {
        public const int DefaultSize = 3;
        public const float DefaultElementSize = 32f;
        public const float DefaultActiveScale = 1.75f;

        const string k_UssClassName = "diamond-bar";
        const string k_ElementUssClassName = k_UssClassName + "__element";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_EndVariantElementUssClassName = k_ElementUssClassName + "--end";
        const string k_ScaleAnimationName = "ScaleAnimation";

        public new class UxmlFactory : UxmlFactory<DiamondBar, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlIntAttributeDescription m_Size = new UxmlIntAttributeDescription() { name = "size", defaultValue = DefaultSize };
            UxmlFloatAttributeDescription m_ElementSize = new UxmlFloatAttributeDescription() { name = "element-size", defaultValue = DefaultElementSize };
            UxmlFloatAttributeDescription m_ActiveScale = new UxmlFloatAttributeDescription() { name = "active-scale", defaultValue = DefaultActiveScale };
            UxmlIntAttributeDescription m_ActiveIndex = new UxmlIntAttributeDescription() { name = "active-index", defaultValue = -1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBar diamondBar = (DiamondBar)ve;
                diamondBar.size = m_Size.GetValueFromBag(bag, cc);
                diamondBar.elementSize = m_ElementSize.GetValueFromBag(bag, cc);
                diamondBar.activeScale = m_ActiveScale.GetValueFromBag(bag, cc);
                diamondBar.activeIndex = m_ActiveIndex.GetValueFromBag(bag, cc);
                diamondBar.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        int m_ElementCount;
        List<DiamondBarElement> m_Elements;
        float m_ElementSize;
        AnimationPlayer m_Player;
        int m_ActiveIndex;
        float m_ActiveScale;

        public List<DiamondBarElement> elements
        {
            get => m_Elements;
        }

        public float animationProgress
        {
            get => m_Player.animationTime / m_Player.duration;
            set
            {
                var previousFrameIndex = m_Player.frameIndex;
                m_Player.animationTime = m_Player.duration * Mathf.Clamp01(value);
                if (m_Player.frameIndex != previousFrameIndex)
                {
                    m_Player.Sample();
                }
            }
        }

        public int size
        {
            get => m_ElementCount;
            set
            {
                m_ElementCount = Mathf.Max(2, value);
                RebuildElements();

                // Fire setter to clamp active index and sample animation.
                activeIndex = m_ActiveIndex;
            }
        }

        public float elementSize
        {
            get => m_ElementSize;
            set
            {
                m_ElementSize = Mathf.Max(0, value);
                foreach (var element in m_Elements)
                {
                    element.size = m_ElementSize;
                }

                m_Player.Sample();
            }
        }

        public int activeIndex
        {
            get => m_ActiveIndex;
            set
            {
                m_ActiveIndex = Mathf.Clamp(value, -1, m_ElementCount);
                foreach (var element in m_Elements)
                {
                    element.size = m_ElementSize;
                }

                m_Player.Sample();
            }
        }

        public float activeScale
        {
            get => m_ActiveScale;
            set
            {
                m_ActiveScale = Mathf.Max(0f, value);
                m_Player.Sample();
            }
        }

        public DiamondBar()
        {
            m_Player = new AnimationPlayer();
            m_Player.AddAnimation(CreateScaleAnimation(), k_ScaleAnimationName);
            m_Player.animation = m_Player[k_ScaleAnimationName];

            m_Elements = new List<DiamondBarElement>();
            AddToClassList(k_UssClassName);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            size = DefaultSize;
            elementSize = DefaultElementSize;
            activeIndex = -1;
            activeScale = DefaultActiveScale;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RecalculateFirstAndLastElementWidth();
        }

        void RecalculateFirstAndLastElementWidth()
        {
            float barWidth = resolvedStyle.width;
            if (barWidth.IsNaN())
            {
                return;
            }

            // element size = total edge size / edge count + diamond size
            var elementSize = (barWidth - this.elementSize * size) / (size * 2 - 2) + this.elementSize;
            m_Elements.First().style.minWidth = elementSize;
            m_Elements.Last().style.minWidth = elementSize;
        }

        void RebuildElements()
        {
            Clear();
            m_Elements.Clear();
            for (int i = 0; i < size; i++)
            {
                var element = new DiamondBarElement() { name = $"element-{i}" };
                element.AddToClassList(k_ElementUssClassName);
                m_Elements.Add(element);
                Add(element);
            }

            var firstElement = m_Elements.First();
            firstElement.leftEdgeDisplayed = false;
            firstElement.AddToClassList(k_EndVariantElementUssClassName);

            var lastElement = m_Elements.Last();
            lastElement.rightEdgeDisplayed = false;
            lastElement.AddToClassList(k_EndVariantElementUssClassName);
            RecalculateFirstAndLastElementWidth();
        }

        KeyframeAnimation CreateScaleAnimation()
        {
            var animation = new KeyframeAnimation();
            var track = animation.AddTrack(t =>
            {
                if (0 <= activeIndex && activeIndex < m_ElementCount)
                {
                    m_Elements[activeIndex].size = (int)(elementSize * Mathf.Lerp(1f, activeScale, t));
                }
            });
            track.AddKeyframe(0, 0f);
            track.AddKeyframe(30, 1f);

            return animation;
        }
    }
}