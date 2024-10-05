using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using KeyframeSystem;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace Controls.Raw
{
    public class DiamondFrameVertical : Control
    {
        const string k_UssClassName = "diamond-frame-vertical";
        const string k_DiamondTopUssClassName = k_UssClassName + "__diamond-top";
        const string k_DiamondBottomUssClassName = k_UssClassName + "__diamond-bottom";
        const string k_FrameContainerUssClassName = k_UssClassName + "__frame-container";
        const string k_RoundedFrameRightUssClassName = k_UssClassName + "__rounded-frame-right";
        const string k_RoundedFrameLeftUssClassName = k_UssClassName + "__rounded-frame-left";
        const string k_ContentContainerUssClassName = k_UssClassName + "__content-container";
        const string k_ResizeElementUssClassName = k_UssClassName + "__resize-element";

        const string k_UnfoldAnimationName = "UnfoldAnimation";
        const float k_DefaultFill = 1f;
        const int k_DefaultCornerRadius = 10;

        public new class UxmlFactory : UxmlFactory<DiamondFrameVertical, UxmlTraits> { };

        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlIntAttributeDescription m_CornerRadius = new UxmlIntAttributeDescription() { name = "corner-radius", defaultValue = k_DefaultCornerRadius };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondFrameVertical diamondFrameVertical = (DiamondFrameVertical)ve;
                diamondFrameVertical.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
                diamondFrameVertical.cornerRadius = m_CornerRadius.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondTop;
        Diamond m_DiamondBottom;
        Control m_FrameContainer;
        RoundedFrame m_RoundedFrameRight;
        RoundedFrame m_RoundedFrameLeft;
        Control m_ContentContainer;
        Control m_ResizeElement;
        // Color m_Color;
        float m_Fill;
        int m_CornerRadius;
        AnimationPlayer m_Player;
        Vector2 m_ContentContainerSize;

        public float animationProgress
        {
            get => m_Player.animationTime / m_Player.duration;
            set
            {
                var previousFrameIndex = m_Player.frameIndex;
                m_Player.animationTime = m_Player.duration * Mathf.Clamp01(value);
                if (m_Player.frameIndex != previousFrameIndex)
                {
                    if (m_ContentContainer.layout.size.IsNaN())
                    {
                        // We are safe here as Unity does not register the same callback twice.
                        m_ContentContainer.RegisterCallback<GeometryChangedEvent>(SampleOnGeometryChanged);
                    }
                    else
                    {
                        m_Player.Sample();
                    }
                }
            }
        }

        public override VisualElement contentContainer
        {
            get => m_ContentContainer;
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
            m_Player = new AnimationPlayer();
            m_Player.AddAnimation(CreateUnfoldAnimation(), k_UnfoldAnimationName);
            m_Player.animation = m_Player[k_UnfoldAnimationName];

            AddToClassList(k_UssClassName);

            m_DiamondTop = new Diamond() { name = "diamond-top" };
            m_DiamondTop.AddToClassList(k_DiamondTopUssClassName);
            hierarchy.Add(m_DiamondTop);

            m_ContentContainer = new Control() { name = "content-container" };
            m_ContentContainer.AddToClassList(k_ContentContainerUssClassName);
            hierarchy.Add(m_ContentContainer);

            m_DiamondBottom = new Diamond() { name = "diamond-bottom" };
            m_DiamondBottom.AddToClassList(k_DiamondBottomUssClassName);
            hierarchy.Add(m_DiamondBottom);

            m_FrameContainer = new Control() { name = "frame-container" };
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

            m_ResizeElement = new Control() { name = "resize-element" };
            m_ResizeElement.AddToClassList(k_ResizeElementUssClassName);
            hierarchy.Add(m_ResizeElement);

            m_ContentContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                m_ResizeElement.style.width = m_ContentContainer.layout.width;
            });
        }

        void SampleOnGeometryChanged(GeometryChangedEvent evt)
        {
            m_Player.Sample();
            m_ContentContainer.UnregisterCallback<GeometryChangedEvent>(SampleOnGeometryChanged);
        }

        KeyframeAnimation CreateUnfoldAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack((float progress) => m_DiamondBottom.animationProgress = m_DiamondTop.animationProgress = progress);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(60, 1f);

            var t2 = animation.AddTrack((float fill) => m_RoundedFrameLeft.fill = m_RoundedFrameRight.fill = fill);
            t2.AddKeyframe(60, 0f);
            t2.AddKeyframe(120, 1f);

            var t3 = animation.AddTrack((float heightMultiplier) => m_ResizeElement.style.height = heightMultiplier * m_ContentContainer.layout.height);
            t3.AddKeyframe(120, 0f);
            t3.AddKeyframe(180, 1f);

            return animation;
        }
    }
}
