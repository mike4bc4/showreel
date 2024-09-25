using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Extensions;
using UnityEditor;

namespace Controls.Raw
{
    public class DiamondBar : VisualElement
    {
        public const int DefaultSize = 3;
        public const float DefaultDiamondSize = 32f;
        public const float DefaultActiveScale = 1.75f;

        const string k_UssClassName = "diamond-bar";
        const string k_ElementUssClassName = k_UssClassName + "__element";
        const string k_BarUssClassName = k_UssClassName + "__bar";
        const string k_ScaleAnimationName = "ScaleAnimation";

        public new class UxmlFactory : UxmlFactory<DiamondBar, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlIntAttributeDescription m_Size = new UxmlIntAttributeDescription() { name = "size", defaultValue = DefaultSize };
            UxmlFloatAttributeDescription m_DiamondSize = new UxmlFloatAttributeDescription() { name = "diamond-size", defaultValue = DefaultDiamondSize };
            UxmlFloatAttributeDescription m_ActiveScale = new UxmlFloatAttributeDescription() { name = "active-scale", defaultValue = DefaultActiveScale };
            UxmlIntAttributeDescription m_ActiveIndex = new UxmlIntAttributeDescription() { name = "active-index", defaultValue = -1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBar diamondBar = (DiamondBar)ve;
                diamondBar.size = m_Size.GetValueFromBag(bag, cc);
                diamondBar.diamondSize = m_DiamondSize.GetValueFromBag(bag, cc);
                diamondBar.activeScale = m_ActiveScale.GetValueFromBag(bag, cc);
                diamondBar.activeIndex = m_ActiveIndex.GetValueFromBag(bag, cc);
                diamondBar.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        int m_Size;
        List<VisualElement> m_Elements;
        List<DiamondTiled> m_Diamonds;
        float m_ElementSize;
        AnimationPlayer m_Player;
        int m_ActiveIndex;
        float m_ActiveScale;

        public IReadOnlyList<DiamondTiled> diamonds
        {
            get => m_Diamonds.AsReadOnly();
        }

        public DiamondTiled activeDiamond
        {
            get
            {
                if (0 <= m_ActiveIndex && m_ActiveIndex < m_Diamonds.Count)
                {
                    return m_Diamonds[m_ActiveIndex];
                }

                return null;
            }
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
            get => m_Size;
            set
            {
                m_Size = Mathf.Max(2, value);
                RebuildElements();

                // Fire setter to clamp active index and sample animation.
                activeIndex = m_ActiveIndex;
            }
        }

        public float diamondSize
        {
            get => m_ElementSize;
            set
            {
                m_ElementSize = Mathf.Ceil(Mathf.Max(0, value));
                foreach (var diamond in m_Diamonds)
                {
                    diamond.style.width = m_ElementSize;
                    diamond.style.height = m_ElementSize;
                }

                m_Player.Sample();
            }
        }

        public int activeIndex
        {
            get => m_ActiveIndex;
            set
            {
                m_ActiveIndex = Mathf.Clamp(value, -1, m_Size - 1);
                foreach (var diamond in m_Diamonds)
                {
                    diamond.style.width = m_ElementSize;
                    diamond.style.height = m_ElementSize;
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

            m_Elements = new List<VisualElement>();
            m_Diamonds = new List<DiamondTiled>();

            AddToClassList(k_UssClassName);

            size = DefaultSize;
            diamondSize = DefaultDiamondSize;
            activeIndex = -1;
            activeScale = DefaultActiveScale;
        }

        void RebuildElements()
        {
            Clear();
            m_Elements.Clear();
            m_Diamonds.Clear();

            for (int i = 0; i < size; i++)
            {
                var element = new VisualElement();
                element.name = "element-" + i;
                element.AddToClassList(k_ElementUssClassName);
                Add(element);
                m_Elements.Add(element);

                if (i > 0)
                {
                    var barLeft = new VisualElement();
                    barLeft.name = "bar-left";
                    barLeft.AddToClassList(k_BarUssClassName);
                    barLeft.style.marginRight = -2f;
                    element.Add(barLeft);
                }

                var diamond = new DiamondTiled();
                diamond.name = "diamond";
                element.Add(diamond);
                m_Diamonds.Add(diamond);

                if (i < size - 1)
                {
                    var barRight = new VisualElement();
                    barRight.name = "bar-right";
                    barRight.AddToClassList(k_BarUssClassName);
                    barRight.style.marginLeft = -2f;
                    element.Add(barRight);
                }
            }

            if (layout.width.IsNaN())
            {
                EventCallback<GeometryChangedEvent> callback = null;
                callback = evt =>
                {
                    RecalculateElementsWidth();
                    UnregisterCallback(callback);
                };

                RegisterCallback(callback);
            }
            else
            {
                RecalculateElementsWidth();
            }
        }

        void RecalculateElementsWidth()
        {
            var barCount = size * 2 - 2;
            var totalBarWidth = layout.width - size * m_ElementSize;
            var barWidth = totalBarWidth / barCount;

            var endElementWidth = ((barWidth + m_ElementSize) / layout.width) * 100f;
            m_Elements.First().style.width = Length.Percent(endElementWidth);
            m_Elements.Last().style.width = Length.Percent(endElementWidth);

            var elementWidth = (100f - 2 * endElementWidth) / (size - 2);
            for (int i = 1; i < childCount - 1; i++)
            {
                m_Elements[i].style.width = Length.Percent(elementWidth);
            }
        }

        KeyframeAnimation CreateScaleAnimation()
        {
            var animation = new KeyframeAnimation();
            var track = animation.AddTrack(t =>
            {
                if (0 <= activeIndex && activeIndex < m_Size)
                {
                    var size = Mathf.Ceil(diamondSize * Mathf.Lerp(1f, activeScale, t));
                    m_Diamonds[activeIndex].style.width = size;
                    m_Diamonds[activeIndex].style.height = size;
                }
            });
            track.AddKeyframe(0, 0f);
            track.AddKeyframe(30, 1f);

            return animation;
        }
    }
}