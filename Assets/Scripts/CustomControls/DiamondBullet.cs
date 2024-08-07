using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace CustomControls
{
    public class DiamondBullet : VisualElement
    {
        const string k_UssClassName = "diamond-bullet";
        const string k_LineContainerUssClassName = k_UssClassName + "__line-container";
        const string k_LineUssClassName = k_UssClassName + "__line";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_SpacerUssClassName = k_UssClassName + "__spacer";

        const float k_SpacerAnimationFlex = 1f;
        const float k_LineAnimationFlex = 2f;
        const float k_DiamondAnimationFlex = 3f;

        public new class UxmlFactory : UxmlFactory<DiamondBullet, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBullet diamondBullet = (DiamondBullet)ve;
                diamondBullet.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_LineContainer;
        VisualElement m_Line;
        DiamondSpreading m_Diamond;
        VisualElement m_Spacer;
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

        public DiamondBullet()
        {
            m_Player = new KeyframeTrackPlayer();
            AddToClassList(k_UssClassName);

            m_Spacer = new VisualElement() { name = "spacer" };
            m_Spacer.AddToClassList(k_SpacerUssClassName);
            Add(m_Spacer);

            m_Diamond = new DiamondSpreading() { name = "diamond" };
            m_Diamond.AddToClassList(k_DiamondUssClassName);
            Add(m_Diamond);

            m_Line = new VisualElement() { name = "line" };
            m_Line.AddToClassList(k_LineUssClassName);
            m_Spacer.Add(m_Line);

            m_Diamond.style.scale = Vector2.one * 0.5f;

            var t1 = m_Player.AddKeyframeTrack((float scale) => m_Diamond.style.scale = Vector2.one * scale);
            t1.AddKeyframe(0, 0.5f);
            t1.AddKeyframe(30, 1f);

            var t2 = m_Player.AddKeyframeTrack((float flexGrow) => m_Spacer.style.flexGrow = flexGrow);
            t2.AddKeyframe(0, 0f);
            t2.AddKeyframe(30, 1f);

            var t3 = m_Player.AddKeyframeTrack((float width) => m_Line.style.width = Length.Percent(width * 100f));
            t3.AddKeyframe(20, 1f);
            t3.AddKeyframe(50, 0f);

            var t4 = m_Player.AddKeyframeTrack((float progress) => m_Diamond.animationProgress = progress);
            t4.AddKeyframe(50, 0f);
            t4.AddKeyframe(95, 1f);
        }

        public void SetAnimationProgress(float animationProgress)
        {
            this.animationProgress = animationProgress;
        }
    }
}
