using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class Subtitle : LocalizedElementContainer
    {
        const string k_UssClassName = "subtitle";
        const string k_LabelUssClassName = k_UssClassName + "__label";
        const string k_BorderUssClassName = k_UssClassName + "__border";
        const string k_AnimationName = "Animation";

        public new class UxmlFactory : UxmlFactory<Subtitle, UxmlTraits> { }

        public new class UxmlTraits : LocalizedElementContainer.UxmlTraits
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

        LocalizedLabel m_Label;
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

        protected override ILocalizedElement localizedElement => m_Label;

        public Subtitle()
        {
            m_Player = new AnimationPlayer();
            m_Player.sampling = 120;
            m_Player.AddAnimation(CreateAnimation(), k_AnimationName);
            m_Player.animation = m_Player[k_AnimationName];

            AddToClassList(k_UssClassName);

            m_Label = new LocalizedLabel() { name = "label" };
            m_Label.AddToClassList(k_LabelUssClassName);
            Add(m_Label);

            m_Border = new VisualElement() { name = "border" };
            m_Border.AddToClassList(k_BorderUssClassName);
            Add(m_Border);
        }

        KeyframeAnimation CreateAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack((float translation) => m_Border.style.translate = new Translate(Length.Percent(translation), 0f));
            t1.AddKeyframe(0, -100f, Easing.EaseOutSine);
            t1.AddKeyframe(120, 100f);

            var t2 = animation.AddTrack((float origin) => m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(origin), 0f));
            t2.AddKeyframe(0, 100f, Easing.EaseOutSine);
            t2.AddKeyframe(120, 0);

            return animation;
        }
    }
}
