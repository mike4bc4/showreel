using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using KeyframeSystem;
using StyleUtility;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls.Raw
{
    public class DiamondSpreading : Control
    {
        const string k_UssClassName = "diamond-spreading";
        const string k_CornerContainerUssClassName = k_UssClassName + "__corner-container";
        const string k_CornerUssClassName = k_UssClassName + "__corner";
        const string k_CornerNUssClassName = k_UssClassName + "__corner-n";
        const string k_CornerSUssClassName = k_UssClassName + "__corner-s";
        const string k_CornerEUssClassName = k_UssClassName + "__corner-e";
        const string k_CornerWUssClassName = k_UssClassName + "__corner-w";
        const string k_CornerBodyUssClassName = k_UssClassName + "__corner-body";
        const string k_FullUssClassName = k_UssClassName + "__full";
        const string k_SpreadAnimationName = "SpreadAnimation";

        // Defines amount of overlapping which allows to avoid gaps between elements.
        const float k_SpreadEpsilon = 0.02f;
        const float k_FillEpsilon = 0.05f;
        const float k_DefaultAnimationProgress = 1f;

        public new class UxmlFactory : UxmlFactory<DiamondSpreading, UxmlTraits> { }

        public new class UxmlTraits : Control.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = k_DefaultAnimationProgress };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondSpreading diamondFolded = (DiamondSpreading)ve;
                diamondFolded.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        Control m_CornerContainer;
        Control m_CornerW;
        Control m_CornerN;
        Control m_CornerE;
        Control m_CornerS;
        Control m_DiamondFull;
        List<Control> m_CornerBodies;
        float m_EdgeWidth;
        float m_Spread;
        float m_Fill;
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

        public float fill
        {
            get => m_Fill;
            set
            {
                m_Fill = Mathf.Clamp01(value);
                float scaleFactor = m_Fill * (1f - 2f * edgeWidth) / edgeWidth;
                scaleFactor += k_FillEpsilon * m_Fill;
                foreach (var cornerBody in m_CornerBodies)
                {
                    cornerBody.style.scale = new Vector2(1 + scaleFactor, 1);
                }
            }
        }

        public float spread
        {
            get => m_Spread;
            set
            {
                m_Spread = Mathf.Clamp01(value);
                var offset = (edgeWidth / 2f + m_Spread * (0.5f - edgeWidth));
                offset -= k_SpreadEpsilon * (1f - m_Spread);
                m_CornerN.style.translate = new Translate(Length.Percent(-offset * 100f), Length.Percent(-offset * 100f));
                m_CornerW.style.translate = new Translate(Length.Percent(-offset * 100f), Length.Percent(offset * 100f));
                m_CornerS.style.translate = new Translate(Length.Percent(offset * 100f), Length.Percent(offset * 100f));
                m_CornerE.style.translate = new Translate(Length.Percent(offset * 100f), Length.Percent(-offset * 100f));
            }
        }

        public float edgeWidth
        {
            get => m_EdgeWidth;
            set
            {
                m_EdgeWidth = Mathf.Clamp01(value);
                m_CornerN.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerW.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerS.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerE.style.scale = Vector2.one * m_EdgeWidth;

                // Update dependant properties.
                spread = m_Spread;
                fill = m_Fill;
            }
        }

        public float initialEdgeWidth { get; set; }
        public float targetEdgeWidth { get; set; }

        List<Control> corners
        {
            get => new List<Control>() { m_CornerW, m_CornerN, m_CornerE, m_CornerS };
        }

        public DiamondSpreading()
        {
            m_Player = new AnimationPlayer();
            m_Player.AddAnimation(CreateSpreadAnimation(), k_SpreadAnimationName);
            m_Player.animation = m_Player[k_SpreadAnimationName];

            m_CornerBodies = new List<Control>();

            AddToClassList(k_UssClassName);

            m_CornerContainer = new Control() { name = "corner-container" };
            m_CornerContainer.AddToClassList(k_CornerContainerUssClassName);
            Add(m_CornerContainer);

            m_CornerN = new Control() { name = "corner-n" };
            m_CornerN.AddToClassList(k_CornerUssClassName);
            m_CornerN.AddToClassList(k_CornerNUssClassName);
            m_CornerContainer.Add(m_CornerN);

            var cornerBody = new Control() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerN.Add(cornerBody);

            m_CornerW = new Control() { name = "corner-w" };
            m_CornerW.AddToClassList(k_CornerUssClassName);
            m_CornerW.AddToClassList(k_CornerWUssClassName);
            m_CornerContainer.Add(m_CornerW);

            cornerBody = new Control() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerW.Add(cornerBody);

            m_CornerS = new Control() { name = "corner-s" };
            m_CornerS.AddToClassList(k_CornerUssClassName);
            m_CornerS.AddToClassList(k_CornerSUssClassName);
            m_CornerContainer.Add(m_CornerS);

            cornerBody = new Control() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerS.Add(cornerBody);

            m_CornerE = new Control() { name = "corner-e" };
            m_CornerE.AddToClassList(k_CornerUssClassName);
            m_CornerE.AddToClassList(k_CornerEUssClassName);
            m_CornerContainer.Add(m_CornerE);

            cornerBody = new Control() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerE.Add(cornerBody);

            m_DiamondFull = new Control();
            m_DiamondFull.name = "diamond-full";
            m_DiamondFull.AddToClassList(k_FullUssClassName);
            m_CornerContainer.Add(m_DiamondFull);

            targetEdgeWidth = 0.28f;

            fill = 0f;
            spread = 0f;
            edgeWidth = targetEdgeWidth * 0.5f;
            animationProgress = k_DefaultAnimationProgress;
        }

        KeyframeAnimation CreateSpreadAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(spread => this.spread = spread);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = animation.AddTrack(t => this.edgeWidth = Mathf.Lerp(targetEdgeWidth * 0.5f, targetEdgeWidth, t));
            t2.AddKeyframe(0, 0f);
            t2.AddKeyframe(20, 1f);

            var t3 = animation.AddTrack(fill => this.fill = fill);
            t3.AddKeyframe(20, 0f);
            t3.AddKeyframe(40, 1f);

            var t4 = animation.AddTrack(visibility =>
            {
                // Using opacity as visibility is inherited from hierarchy and changes here would
                // override it.
                m_DiamondFull.style.opacity = visibility;
                foreach (var corner in corners)
                {
                    corner.style.opacity = 1f - visibility;
                }
            });
            t4.AddKeyframe(0, 0f, Easing.StepOut);
            t4.AddKeyframe(40, 1f);

            return animation;
        }
    }
}
