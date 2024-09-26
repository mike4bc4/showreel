using System;
using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class Diamond : VisualElement
    {
        const string k_UssClassName = "diamond";
        const string k_HalfUssClassName = k_UssClassName + "__half";
        const string k_FullUssClassName = k_UssClassName + "__full";
        const string k_MiddleUssClassName = k_UssClassName + "__middle";
        const string k_UnfoldAnimationName = "UnfoldAnimation";
        const float k_DefaultAnimationProgress = 1f;

        public new class UxmlFactory : UxmlFactory<Diamond, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = k_DefaultAnimationProgress };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var diamond = (Diamond)ve;
                diamond.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_HalfLeft;
        VisualElement m_HalfRight;
        VisualElement m_DiamondFull;
        VisualElement m_DiamondMiddle;
        AnimationPlayer m_Player;

        public float animationProgress
        {
            get => m_Player.animationTime / m_Player.duration;
            set
            {
                var previousFrameIndex = m_Player.frameIndex;
                m_Player.animationTime = m_Player.duration * Mathf.Clamp01(value);
                if (previousFrameIndex != m_Player.frameIndex)
                {
                    m_Player.Sample();
                }
            }
        }

        public Diamond()
        {
            m_Player = new AnimationPlayer();
            m_Player.AddAnimation(CreateUnfoldAnimation(), k_UnfoldAnimationName);
            m_Player.animation = m_Player[k_UnfoldAnimationName];

            AddToClassList(k_UssClassName);

            m_HalfLeft = new VisualElement();
            m_HalfLeft.name = "half-left";
            m_HalfLeft.AddToClassList(k_HalfUssClassName);
            Add(m_HalfLeft);

            m_HalfRight = new VisualElement();
            m_HalfRight.name = "half-right";
            m_HalfRight.AddToClassList(k_HalfUssClassName);
            Add(m_HalfRight);

            m_DiamondFull = new VisualElement();
            m_DiamondFull.name = "diamond-full";
            m_DiamondFull.AddToClassList(k_FullUssClassName);
            Add(m_DiamondFull);

            m_DiamondMiddle = new VisualElement();
            m_DiamondMiddle.name = "diamond-middle";
            m_DiamondMiddle.AddToClassList(k_MiddleUssClassName);
            Add(m_DiamondMiddle);

            animationProgress = k_DefaultAnimationProgress;
        }

        KeyframeAnimation CreateUnfoldAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(scaleX => m_HalfLeft.style.scale = new Vector2(scaleX, 1f));
            t1.AddKeyframe(0, 1f, Easing.EaseInOutSine);
            t1.AddKeyframe(60, -1f);

            var t2 = animation.AddTrack(opacity =>
            {
                m_DiamondFull.style.opacity = opacity;
                m_DiamondMiddle.style.opacity = 1f - opacity;
                m_HalfLeft.style.opacity = 1f - opacity;
                m_HalfRight.style.opacity = 1f - opacity;
            });
            t2.AddKeyframe(0, 0f, Easing.StepOut);
            t2.AddKeyframe(60, 1f);

            return animation;
        }
    }
}
