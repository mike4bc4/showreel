using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class Subtitle : VisualElement
    {
        public static readonly string ussClassName = "subtitle";
        public static readonly string labelUssClassName = ussClassName + "__label";
        public static readonly string borderUssClassName = ussClassName + "__border";

        public new class UxmlFactory : UxmlFactory<Subtitle, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Subtitle" };
            UxmlFloatAttributeDescription m_BorderScale = new UxmlFloatAttributeDescription() { name = "border-scale", defaultValue = 0.5f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Subtitle subtitle = (Subtitle)ve;
                subtitle.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
                subtitle.text = m_Text.GetValueFromBag(bag, cc);
                subtitle.borderScale = m_BorderScale.GetValueFromBag(bag, cc);
            }
        }

        Label m_Label;
        VisualElement m_Border;
        float m_BorderScale;
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

        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        public float borderScale
        {
            get => m_BorderScale;
            set
            {
                m_BorderScale = Mathf.Clamp01(value);
                m_Border.style.scale = new Vector2(m_BorderScale, 1f);
            }
        }

        public Subtitle()
        {
            m_Player = new AnimationPlayer();
            m_Player.sampling = 60;
            var animation = new KeyframeAnimation();
            m_Player.AddAnimation(animation, "Animation");
            m_Player.animation = animation;

            AddToClassList(ussClassName);

            m_Label = new Label() { name = "label" };
            m_Label.AddToClassList(labelUssClassName);
            Add(m_Label);

            m_Border = new VisualElement() { name = "border" };
            m_Border.AddToClassList(borderUssClassName);
            Add(m_Border);

            var t1 = animation.AddTrack((float translation) => m_Border.style.translate = new Translate(Length.Percent(translation), 0f));
            t1.AddKeyframe(0, -100f, Easing.EaseOutSine);
            t1.AddKeyframe(60, 100f);

            var t2 = animation.AddTrack((float origin) => m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(origin), 0f));
            t2.AddKeyframe(0, 100f, Easing.EaseOutSine);
            t2.AddKeyframe(60, 0);
        }

        public void SetAnimationProgress(float animationProgress)
        {
            this.animationProgress = animationProgress;
        }
    }
}
