using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class Diamond : VisualElement
    {
        const string k_UssClassName = "diamond";
        const string k_HalfUssClassName = k_UssClassName + "__half";

        public new class UxmlFactory : UxmlFactory<Diamond, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        VisualElement m_HalfLeft;
        VisualElement m_HalfRight;
        Coroutine m_CoroutineHandle;

        public Diamond()
        {
            AddToClassList(k_UssClassName);

            m_HalfLeft = new VisualElement();
            m_HalfLeft.name = "half-left";
            m_HalfLeft.AddToClassList(k_HalfUssClassName);
            Add(m_HalfLeft);

            m_HalfRight = new VisualElement();
            m_HalfRight.name = "half-right";
            m_HalfRight.AddToClassList(k_HalfUssClassName);
            Add(m_HalfRight);
        }

        void UnfoldImmediate()
        {
            m_HalfLeft.style.RemoveTransition("scale");
            m_HalfLeft.style.scale = new Vector2(-1f, 1f);
        }

        public Coroutine Unfold(bool immediate = false)
        {
            if (m_CoroutineHandle != null)
            {
                AnimationManager.Instance.StopCoroutine(m_CoroutineHandle);
            }

            if (immediate)
            {
                UnfoldImmediate();
                return null;
            }

            IEnumerator Coroutine()
            {
                FoldImmediate();
                m_HalfLeft.style.AddTransition("scale", 0.5f, EasingMode.EaseInOutSine);
                m_HalfLeft.style.scale = new Vector2(-1f, 1f);
                while (m_HalfLeft.resolvedStyle.scale != new Vector2(-1f, 1f))
                {
                    yield return null;
                }
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }

        void FoldImmediate()
        {
            m_HalfLeft.style.RemoveTransition("width");
            m_HalfLeft.style.scale = Vector2.one;
        }

        public Coroutine Fold(bool immediate = false)
        {
            if (m_CoroutineHandle != null)
            {
                AnimationManager.Instance.StopCoroutine(m_CoroutineHandle);
            }

            if (immediate)
            {
                FoldImmediate();
                return null;
            }

            IEnumerator Coroutine()
            {
                UnfoldImmediate();
                m_HalfLeft.style.AddTransition("scale", 0.5f, EasingMode.EaseInOutSine);
                m_HalfLeft.style.scale = Vector2.one;
                while (m_HalfLeft.resolvedStyle.scale != Vector2.one)
                {
                    yield return null;
                }
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }
    }
}
