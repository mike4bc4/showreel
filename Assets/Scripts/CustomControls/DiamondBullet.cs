using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondBullet : VisualElement
    {
        const string k_UssClassName = "diamond-bullet";
        const string k_LineContainerUssClassName = k_UssClassName + "__line-container";
        const string k_LineUssClassName = k_UssClassName + "__line";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_SpacerUssClassName = k_UssClassName + "__spacer";

        public new class UxmlFactory : UxmlFactory<DiamondBullet, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBullet diamondBullet = (DiamondBullet)ve;
            }
        }

        VisualElement m_LineContainer;
        VisualElement m_Line;
        DiamondSpreading m_Diamond;
        VisualElement m_Spacer;
        bool m_DiamondUnfolded;
        Coroutine m_Coroutine;

        public DiamondBullet()
        {
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

            m_Diamond.Unfold(immediate: true);
        }

        public Coroutine Fold(bool immediate = false)
        {
            if (immediate)
            {
                m_Spacer.style.RemoveTransition("flex-grow");
                m_Spacer.style.flexGrow = 0f;

                m_Line.style.RemoveTransition("width");
                m_Line.style.width = Length.Percent(100f);

                m_Diamond.style.RemoveTransition("scale");
                m_Diamond.style.scale = Vector2.zero;
                m_Diamond.Fold(immediate: true);
                return null;
            }

            IEnumerator Coroutine()
            {
                Unfold(immediate = true);
                yield return m_Diamond.Fold();

                m_Line.style.AddTransition("width", 0.5f, EasingMode.EaseInCubic);
                m_Line.style.width = Length.Percent(100f);
                while (m_Line.resolvedStyle.width < m_Spacer.resolvedStyle.width)
                {
                    yield return null;
                }

                m_Diamond.style.AddTransition("scale", 0.5f);
                m_Diamond.style.scale = Vector2.zero;

                m_Spacer.style.AddTransition("flex-grow", 0.5f, EasingMode.EaseOutSine);
                m_Spacer.style.flexGrow = 0f;
                while (m_Spacer.resolvedStyle.flexGrow > 0f)
                {
                    yield return null;
                }
            }

            if (m_Coroutine != null)
            {
                AnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public Coroutine Unfold(bool immediate = false)
        {
            if (immediate)
            {
                m_Spacer.style.RemoveTransition("flex-grow");
                m_Spacer.style.flexGrow = 1f;

                m_Line.style.RemoveTransition("width");
                m_Line.style.width = Length.Percent(0f);

                m_Diamond.style.RemoveTransition("scale");
                m_Diamond.style.scale = Vector2.one;
                m_Diamond.Fold(immediate: true);
                return null;
            }

            IEnumerator Coroutine()
            {
                Fold(immediate: true);

                m_Diamond.style.AddTransition("scale", 0.5f);
                m_Diamond.style.scale = Vector2.one;

                m_Spacer.style.AddTransition("flex-grow", 0.5f, EasingMode.EaseInCubic);
                m_Spacer.style.flexGrow = 1f;
                while (m_Spacer.resolvedStyle.flexGrow != 1f)
                {
                    yield return null;
                }

                m_Line.style.AddTransition("width", 0.5f, EasingMode.EaseOutSine);
                m_Line.style.width = Length.Percent(0f);
                while (m_Line.resolvedStyle.width != 0f)
                {
                    yield return null;
                }

                yield return m_Diamond.Unfold();
            }

            if (m_Coroutine != null)
            {
                AnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }
    }
}
