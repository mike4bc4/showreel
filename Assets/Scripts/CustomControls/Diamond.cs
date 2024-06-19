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
        const string k_HalfTransitionUssClassName = k_HalfUssClassName + "--transition";

        public new class UxmlFactory : UxmlFactory<Diamond, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        VisualElement m_HalfLeft;
        VisualElement m_HalfRight;

        public Diamond()
        {
            AddToClassList(k_UssClassName);

            m_HalfLeft = new VisualElement();
            m_HalfLeft.name = "half-left";
            m_HalfLeft.AddToClassList(k_HalfUssClassName);
            m_HalfLeft.AddToClassList(k_HalfTransitionUssClassName);
            Add(m_HalfLeft);

            m_HalfRight = new VisualElement();
            m_HalfRight.name = "half-right";
            m_HalfRight.AddToClassList(k_HalfUssClassName);
            Add(m_HalfRight);
        }

        Coroutine m_CoroutineHandle;

        public Coroutine Unfold()
        {
            IEnumerator Coroutine()
            {
                m_HalfLeft.style.scale = new Vector2(-1f, 1f);
                while (m_HalfLeft.resolvedStyle.scale != new Vector2(-1f, 1f))
                {
                    yield return null;
                }
            }

            if (m_CoroutineHandle != null)
            {
                AnimationManager.Instance.StopCoroutine(m_CoroutineHandle);
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }

        public Coroutine Fold()
        {
            IEnumerator Coroutine()
            {
                m_HalfLeft.style.scale = Vector2.one;
                while (m_HalfLeft.resolvedStyle.scale != Vector2.one)
                {
                    yield return null;
                }
            }

            if (m_CoroutineHandle != null)
            {
                AnimationManager.Instance.StopCoroutine(m_CoroutineHandle);
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }
    }
}
