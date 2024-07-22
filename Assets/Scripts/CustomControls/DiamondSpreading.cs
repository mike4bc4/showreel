using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        const float k_SpreadEpsilon = 0.05f;
        const float k_AnimationTime = 0.5f;

        public new class UxmlFactory : UxmlFactory<DiamondSpreading, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_EdgeWidth = new UxmlFloatAttributeDescription() { name = "edge-width", defaultValue = 0.3f };
            UxmlFloatAttributeDescription m_Spread = new UxmlFloatAttributeDescription() { name = "spread", defaultValue = 1f };
            UxmlFloatAttributeDescription m_Fill = new UxmlFloatAttributeDescription() { name = "fill", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondSpreading diamondFolded = (DiamondSpreading)ve;
                diamondFolded.edgeWidth = m_EdgeWidth.GetValueFromBag(bag, cc);
                diamondFolded.spread = m_Spread.GetValueFromBag(bag, cc);
                diamondFolded.fill = m_Fill.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_CornerContainer;
        VisualElement m_CornerW;
        VisualElement m_CornerN;
        VisualElement m_CornerE;
        VisualElement m_CornerS;
        List<VisualElement> m_CornerBodies;

        Coroutine m_Coroutine;
        float m_EdgeWidth;
        float m_Spread;
        float m_Fill;

        public float fill
        {
            get => m_Fill;
            set
            {
                m_Fill = Mathf.Clamp01(value);
                float scaleFactor = m_Fill * (1f - 2f * edgeWidth) / edgeWidth;
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
                var offset = (edgeWidth / 2f + m_Spread * (0.5f - edgeWidth)) * (1 - k_SpreadEpsilon);
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

            initialEdgeWidth = 0.15f;
            targetEdgeWidth = 0.3f;
        }

        public Coroutine Fold(bool immediate = false)
        {
            if (immediate)
            {
                spread = 0f;
                edgeWidth = initialEdgeWidth;
                fill = 0f;
                return null;
            }

            IEnumerator Coroutine()
            {
                spread = 1f;
                edgeWidth = targetEdgeWidth;
                fill = 1f;

                var anim1 = CoroutineAnimationManager.Animate(this, nameof(fill), 0f);
                anim1.time = k_AnimationTime;
                anim1.timingFunction = TimingFunction.EaseInCubic;

                yield return anim1.coroutine;

                var anim2 = CoroutineAnimationManager.Animate(this, nameof(spread), 0f);
                anim2.time = k_AnimationTime;
                anim2.timingFunction = TimingFunction.EaseInCubic;
                var anim3 = CoroutineAnimationManager.Animate(this, nameof(edgeWidth), initialEdgeWidth);
                anim3.time = k_AnimationTime;
                anim3.timingFunction = TimingFunction.EaseInCubic;

                yield return anim3.coroutine;
            }

            if (m_Coroutine != null)
            {
                CoroutineAnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = CoroutineAnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public Coroutine Unfold(bool immediate = false)
        {
            if (immediate)
            {
                spread = 1f;
                edgeWidth = targetEdgeWidth;
                fill = 1f;
                return null;
            }

            IEnumerator Coroutine()
            {
                spread = 0f;
                edgeWidth = initialEdgeWidth;
                fill = 0f;

                var anim1 = CoroutineAnimationManager.Animate(this, nameof(spread), 1f);
                anim1.time = k_AnimationTime;
                anim1.timingFunction = TimingFunction.EaseOutCubic;
                var anim2 = CoroutineAnimationManager.Animate(this, nameof(edgeWidth), targetEdgeWidth);
                anim2.time = k_AnimationTime;
                anim2.timingFunction = TimingFunction.EaseOutCubic;

                yield return anim2.coroutine;

                var anim3 = CoroutineAnimationManager.Animate(this, nameof(fill), 1f);
                anim3.time = k_AnimationTime;
                anim3.timingFunction = TimingFunction.EaseOutCubic;

                yield return anim3.coroutine;
            }


            if (m_Coroutine != null)
            {
                CoroutineAnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = CoroutineAnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }
    }
}
