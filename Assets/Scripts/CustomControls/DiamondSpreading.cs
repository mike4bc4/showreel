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
    public class DiamondSpreading : VisualElement
    {
        const string k_UssClassName = "diamond-spreading";
        const string k_CornerContainerUssClassName = k_UssClassName + "__corner-container";
        const string k_CornerUssClassName = k_UssClassName + "__corner";
        const string k_CornerNUssClassName = k_UssClassName + "__corner-n";
        const string k_CornerSUssClassName = k_UssClassName + "__corner-s";
        const string k_CornerEUssClassName = k_UssClassName + "__corner-e";
        const string k_CornerWUssClassName = k_UssClassName + "__corner-w";
        const string k_CornerBodyUssClassName = k_UssClassName + "__corner-body";

        // Defines amount of overlapping which allows to avoid gaps between elements.
        const float k_SpreadEpsilon = 0.02f;
        const float k_FillEpsilon = 0.05f;

        public new class UxmlFactory : UxmlFactory<DiamondSpreading, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondSpreading diamondFolded = (DiamondSpreading)ve;
                diamondFolded.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_CornerContainer;
        VisualElement m_CornerW;
        VisualElement m_CornerN;
        VisualElement m_CornerE;
        VisualElement m_CornerS;
        List<VisualElement> m_CornerBodies;
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

        List<VisualElement> corners
        {
            get => new List<VisualElement>() { m_CornerW, m_CornerN, m_CornerE, m_CornerS };
        }

        public DiamondSpreading()
        {
            m_Player = new AnimationPlayer();
            m_Player.sampling = 60;
            var animation = new KeyframeSystem.KeyframeAnimation();
            m_Player.AddAnimation(animation, "Animation");
            m_Player.animation = animation;

            m_CornerBodies = new List<VisualElement>();

            AddToClassList(k_UssClassName);

            m_CornerContainer = new VisualElement() { name = "corner-container" };
            m_CornerContainer.AddToClassList(k_CornerContainerUssClassName);
            Add(m_CornerContainer);

            m_CornerN = new VisualElement() { name = "corner-n" };
            m_CornerN.AddToClassList(k_CornerUssClassName);
            m_CornerN.AddToClassList(k_CornerNUssClassName);
            m_CornerContainer.Add(m_CornerN);

            var cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerN.Add(cornerBody);

            m_CornerW = new VisualElement() { name = "corner-w" };
            m_CornerW.AddToClassList(k_CornerUssClassName);
            m_CornerW.AddToClassList(k_CornerWUssClassName);
            m_CornerContainer.Add(m_CornerW);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerW.Add(cornerBody);

            m_CornerS = new VisualElement() { name = "corner-s" };
            m_CornerS.AddToClassList(k_CornerUssClassName);
            m_CornerS.AddToClassList(k_CornerSUssClassName);
            m_CornerContainer.Add(m_CornerS);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerS.Add(cornerBody);

            m_CornerE = new VisualElement() { name = "corner-e" };
            m_CornerE.AddToClassList(k_CornerUssClassName);
            m_CornerE.AddToClassList(k_CornerEUssClassName);
            m_CornerContainer.Add(m_CornerE);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerE.Add(cornerBody);

            targetEdgeWidth = 0.3f;

            fill = 0f;
            spread = 0f;
            edgeWidth = targetEdgeWidth * 0.5f;

            var t1 = animation.AddTrack((float spread) => this.spread = spread);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = animation.AddTrack((float edgeWidth) => this.edgeWidth = edgeWidth);
            t2.AddKeyframe(0, targetEdgeWidth * 0.5f);
            t2.AddKeyframe(20, targetEdgeWidth);

            var t3 = animation.AddTrack((float fill) => this.fill = fill);
            t3.AddKeyframe(20, 0f);
            t3.AddKeyframe(40, 1f);
        }
    }
}
