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

        public new class UxmlFactory : UxmlFactory<Diamond, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var diamond = (Diamond)ve;
                diamond.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_HalfLeft;
        VisualElement m_HalfRight;
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
            m_Player.sampling = 60;

            var animation = new KeyframeSystem.KeyframeAnimation();
            m_Player.AddAnimation(animation, "Animation");
            m_Player.animation = animation;

            AddToClassList(k_UssClassName);

            m_HalfLeft = new VisualElement();
            m_HalfLeft.name = "half-left";
            m_HalfLeft.AddToClassList(k_HalfUssClassName);
            Add(m_HalfLeft);

            m_HalfRight = new VisualElement();
            m_HalfRight.name = "half-right";
            m_HalfRight.AddToClassList(k_HalfUssClassName);
            Add(m_HalfRight);

            var t1 = animation.AddTrack((float scaleX) => m_HalfLeft.style.scale = new Vector2(scaleX, 1f));
            t1.AddKeyframe(0, 1f, Easing.EaseInOutSine);
            t1.AddKeyframe(60, -1f);
        }
    }
}
