using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace CustomControls
{
    public class DiamondFrameVertical : VisualElement
    {
        const string k_UssClassName = "diamond-frame-vertical";
        const string k_DiamondTopUssClassName = k_UssClassName + "__diamond-top";
        const string k_DiamondBottomUssClassName = k_UssClassName + "__diamond-bottom";
        const string k_FrameContainerUssClassName = k_UssClassName + "__frame-container";
        const string k_RoundedFrameRightUssClassName = k_UssClassName + "__rounded-frame-right";
        const string k_RoundedFrameLeftUssClassName = k_UssClassName + "__rounded-frame-left";
        const string k_ContentContainerUssClassName = k_UssClassName + "__content-container";
        const string k_ResizingElementUssClassName = k_UssClassName + "__resizing-element";

        static readonly Color s_DefaultColor = Color.black;
        const float k_DefaultFill = 1f;
        const int k_DefaultCornerRadius = 10;

        public new class UxmlFactory : UxmlFactory<DiamondFrameVertical, UxmlTraits> { };

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription() { name = "color", defaultValue = s_DefaultColor };
            UxmlIntAttributeDescription m_CornerRadius = new UxmlIntAttributeDescription() { name = "corner-radius", defaultValue = k_DefaultCornerRadius };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondFrameVertical diamondFrameVertical = (DiamondFrameVertical)ve;
                diamondFrameVertical.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
                diamondFrameVertical.color = m_Color.GetValueFromBag(bag, cc);
                diamondFrameVertical.cornerRadius = m_CornerRadius.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondTop;
        Diamond m_DiamondBottom;
        VisualElement m_FrameContainer;
        RoundedFrame m_RoundedFrameRight;
        RoundedFrame m_RoundedFrameLeft;
        VisualElement m_ContentContainer;
        VisualElement m_ResizingElement;
        Color m_Color;
        float m_Fill;
        int m_CornerRadius;
        KeyframeTrackPlayer m_Player;

        public float animationProgress
        {
            get => m_Player.time / m_Player.duration;
            set
            {
                var previousFrameIndex = m_Player.frameIndex;
                m_Player.time = m_Player.duration * Mathf.Clamp01(value);
                if (m_Player.frameIndex != previousFrameIndex)
                {
                    m_Player.Update();
                }
            }
        }

        public override VisualElement contentContainer
        {
            get => m_ContentContainer;
        }

        public Color color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                m_RoundedFrameLeft.color = m_Color;
                m_RoundedFrameRight.color = m_Color;
            }
        }

        public int cornerRadius
        {
            get => m_CornerRadius;
            set
            {
                m_CornerRadius = Mathf.Max(1, value);
                m_RoundedFrameLeft.cornerRadius = m_CornerRadius;
                m_RoundedFrameRight.cornerRadius = m_CornerRadius;
            }
        }

        public DiamondFrameVertical()
        {
            m_Player = new KeyframeTrackPlayer();

            AddToClassList(k_UssClassName);

            m_DiamondTop = new Diamond() { name = "diamond-top" };
            m_DiamondTop.AddToClassList(k_DiamondTopUssClassName);
            hierarchy.Add(m_DiamondTop);

            m_ResizingElement = new VisualElement() { name = "resizing-element" };
            m_ResizingElement.AddToClassList(k_ResizingElementUssClassName);
            hierarchy.Add(m_ResizingElement);

            m_ContentContainer = new VisualElement() { name = "content-container" };
            m_ContentContainer.AddToClassList(k_ContentContainerUssClassName);
            hierarchy.Add(m_ContentContainer);

            m_DiamondBottom = new Diamond() { name = "diamond-bottom" };
            m_DiamondBottom.AddToClassList(k_DiamondBottomUssClassName);
            hierarchy.Add(m_DiamondBottom);

            m_FrameContainer = new VisualElement() { name = "frame-container" };
            m_FrameContainer.AddToClassList(k_FrameContainerUssClassName);
            hierarchy.Add(m_FrameContainer);

            m_RoundedFrameRight = new RoundedFrame() { name = "rounded-frame-right" };
            m_RoundedFrameRight.AddToClassList(k_RoundedFrameRightUssClassName);
            m_RoundedFrameRight.fill = 0f;
            m_FrameContainer.Add(m_RoundedFrameRight);

            m_RoundedFrameLeft = new RoundedFrame() { name = "rounded-frame-left" };
            m_RoundedFrameLeft.AddToClassList(k_RoundedFrameLeftUssClassName);
            m_RoundedFrameLeft.fill = 0f;
            m_FrameContainer.Add(m_RoundedFrameLeft);

            m_ContentContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                m_ResizingElement.style.width = m_ContentContainer.resolvedStyle.width;
                m_Player.Update(force: true);
            });

            var t1 = m_Player.AddKeyframeTrack((float progress) => m_DiamondBottom.animationProgress = m_DiamondTop.animationProgress = progress);
            t1.AddKeyframe(0f, 0f);
            t1.AddKeyframe(1f, 1f);

            var t2 = m_Player.AddKeyframeTrack((float fill) => m_RoundedFrameLeft.fill = m_RoundedFrameRight.fill = fill);
            t2.AddKeyframe(1f, 0f);
            t2.AddKeyframe(2f, 1f);

            var t3 = m_Player.AddKeyframeTrack((float heightMultiplier) =>
            {
                if (!m_ContentContainer.resolvedStyle.height.IsNan())
                {
                    m_ResizingElement.style.height = m_ContentContainer.resolvedStyle.height * heightMultiplier;
                }
                else
                {
                    void OnGeometryChanged(GeometryChangedEvent evt)
                    {
                        m_ResizingElement.style.height = m_ContentContainer.resolvedStyle.height * heightMultiplier;
                        m_ContentContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    }

                    m_ContentContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                }
            });
            t3.AddKeyframe(2f, 0f);
            t3.AddKeyframe(3f, 1f);
        }

        public void SetAnimationProgress(float animationProgress)
        {
            this.animationProgress = animationProgress;
        }
    }
}
