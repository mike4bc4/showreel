using System;
using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class DiamondBullet : Control
    {
        const string k_UssClassName = "diamond-bullet";
        const string k_LineContainerUssClassName = k_UssClassName + "__line-container";
        const string k_LineUssClassName = k_UssClassName + "__line";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_SpacerUssClassName = k_UssClassName + "__spacer";

        const float k_DefaultAnimationProgress = 1f;

        public new class UxmlFactory : UxmlFactory<DiamondBullet, UxmlTraits> { }

        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = k_DefaultAnimationProgress };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBullet diamondBullet = (DiamondBullet)ve;
                diamondBullet.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        Control m_LineContainer;
        Control m_Line;
        DiamondSpreading m_Diamond;
        Control m_Spacer;
        AnimationPlayer m_Player;

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

        public DiamondBullet()
        {
            m_Player = new AnimationPlayer();
            var animation = new KeyframeSystem.KeyframeAnimation();
            m_Player.AddAnimation(animation, "Animation");
            m_Player.animation = animation;

            AddToClassList(k_UssClassName);

            m_Spacer = new Control() { name = "spacer" };
            m_Spacer.AddToClassList(k_SpacerUssClassName);
            Add(m_Spacer);

            m_Diamond = new DiamondSpreading() { name = "diamond" };
            m_Diamond.AddToClassList(k_DiamondUssClassName);
            Add(m_Diamond);

            m_Line = new Control() { name = "line" };
            m_Line.AddToClassList(k_LineUssClassName);
            m_Spacer.Add(m_Line);

            m_Diamond.style.scale = Vector2.one * 0.5f;

            var t1 = animation.AddTrack((float scale) => m_Diamond.style.scale = Vector2.one * scale);
            t1.AddKeyframe(0, 0.5f);
            t1.AddKeyframe(30, 1f);

            var t2 = animation.AddTrack((float flexGrow) => m_Spacer.style.flexGrow = flexGrow);
            t2.AddKeyframe(0, 0f);
            t2.AddKeyframe(30, 1f);

            var t3 = animation.AddTrack((float width) => m_Line.style.width = Length.Percent(width * 100f));
            t3.AddKeyframe(20, 1f);
            t3.AddKeyframe(50, 0f);

            var t4 = animation.AddTrack((float progress) => m_Diamond.animationProgress = progress);
            t4.AddKeyframe(50, 0f);
            t4.AddKeyframe(95, 1f);

            animationProgress = k_DefaultAnimationProgress;
        }
    }
}
