using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondLineHorizontal : VisualElement
    {
        const string k_UssClassName = "diamond-line-horizontal";
        const string k_DiamondRightUssClassName = k_UssClassName + "__diamond-right";
        const string k_SeparatorUssClassName = k_UssClassName + "__separator";

        public new class UxmlFactory : UxmlFactory<DiamondLineHorizontal, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_TargetWidth = new UxmlFloatAttributeDescription() { name = "target-width" };
            UxmlBoolAttributeDescription m_Unfolded = new UxmlBoolAttributeDescription() { name = "unfolded", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondLineHorizontal diamondLineHorizontal = (DiamondLineHorizontal)ve;
                diamondLineHorizontal.targetWidth = m_TargetWidth.GetValueFromBag(bag, cc);
                diamondLineHorizontal.unfolded = m_Unfolded.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondLeft;
        Diamond m_DiamondRight;
        VisualElement m_Separator;
        float m_DefaultWidth;
        Coroutine m_CoroutineHandle;
        bool m_Unfolded;

        public float targetWidth { get; set; }

        bool unfolded
        {
            get => m_Unfolded;
            set
            {
                m_Unfolded = value;
                if (m_Unfolded)
                {
                    style.width = targetWidth;
                    m_DiamondLeft.Unfold();
                    m_DiamondRight.Unfold();
                }
                else
                {
                    style.width = StyleKeyword.Auto;
                    m_DiamondLeft.Fold();
                    m_DiamondRight.Fold();
                }
            }
        }

        public DiamondLineHorizontal()
        {
            AddToClassList(k_UssClassName);

            m_DiamondLeft = new Diamond();
            m_DiamondLeft.name = "diamond-left";
            Add(m_DiamondLeft);

            m_Separator = new VisualElement();
            m_Separator.name = "separator";
            m_Separator.AddToClassList(k_SeparatorUssClassName);
            Add(m_Separator);

            m_DiamondRight = new Diamond();
            m_DiamondRight.name = "diamond-right";
            m_DiamondRight.AddToClassList(k_DiamondRightUssClassName);
            Add(m_DiamondRight);

            if (Application.isPlaying)
            {
                RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            m_DefaultWidth = resolvedStyle.width;
            style.width = m_DefaultWidth;
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public Coroutine Unfold()
        {
            IEnumerator Coroutine()
            {
                m_DiamondLeft.Unfold();
                yield return m_DiamondRight.Unfold();
                style.width = targetWidth;
                while (resolvedStyle.width != targetWidth)
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
                m_DiamondLeft.Fold();
                yield return m_DiamondRight.Fold();
                style.width = m_DefaultWidth;
                while (resolvedStyle.width != m_DefaultWidth)
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
