using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondBar : VisualElement
    {
        const string k_UssClassName = "diamond-bar";
        const string k_ElementUssClassName = k_UssClassName + "__element";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_DiamondTransitionUssClassName = k_DiamondUssClassName + "--transition";
        const string k_EndVariantElementUssClassName = k_ElementUssClassName + "--end";

        public new class UxmlFactory : UxmlFactory<DiamondBar, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription m_ElementCount = new UxmlIntAttributeDescription() { name = "element-count", defaultValue = 3 };
            UxmlIntAttributeDescription m_DiamondSize = new UxmlIntAttributeDescription() { name = "diamond-size", defaultValue = 42 };
            UxmlFloatAttributeDescription m_ActiveScale = new UxmlFloatAttributeDescription() { name = "active-scale", defaultValue = 1.66f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBar diamondBar = (DiamondBar)ve;
                diamondBar.elementCount = m_ElementCount.GetValueFromBag(bag, cc);
                diamondBar.diamondSize = m_DiamondSize.GetValueFromBag(bag, cc);
                diamondBar.activeScale = m_ActiveScale.GetValueFromBag(bag, cc);
            }
        }

        int m_ElementCount;
        List<DiamondBarElement> m_Elements;
        int m_ActiveIndex;
        int m_DiamondSize;

        public int elementCount
        {
            get => m_ElementCount;
            set
            {
                m_ElementCount = Mathf.Max(2, value);
                CreateElements();
            }
        }

        public int diamondSize
        {
            get => m_DiamondSize;
            set
            {
                m_DiamondSize = Mathf.Max(0, value);
                foreach (var element in m_Elements)
                {
                    element.diamondSize = m_DiamondSize;
                }
            }
        }

        public float activeScale { get; set; }

        public int activeIndex
        {
            get => m_ActiveIndex;
        }

        Coroutine m_Coroutine;

        public Coroutine SetActiveIndex(int activeIndex, bool immediate = false)
        {
            m_ActiveIndex = Mathf.Clamp(activeIndex, -1, m_ElementCount);
            if (immediate)
            {
                for (int i = 0; i < m_Elements.Count; i++)
                {
                    var element = m_Elements[i];
                    element.diamond.RemoveFromClassList(k_DiamondTransitionUssClassName);
                    if (i != m_ActiveIndex)
                    {
                        element.diamond.StopAnimation();
                        element.diamondSize = m_DiamondSize;
                    }
                }

                if (m_ActiveIndex >= 0 && m_ActiveIndex < m_Elements.Count)
                {
                    var targetSize = (int)(m_DiamondSize * activeScale);
                    var element = m_Elements[m_ActiveIndex];
                    element.diamond.StartAnimation();
                    element.diamondSize = targetSize;
                }

                return null;
            }

            IEnumerator Coroutine()
            {
                foreach (var element in m_Elements)
                {
                    element.diamond.AddToClassList(k_DiamondTransitionUssClassName);
                }

                // Wait one frame to apply transition class.
                yield return null;

                for (int i = 0; i < m_Elements.Count; i++)
                {
                    if (i != m_ActiveIndex)
                    {
                        var element = m_Elements[i];
                        element.diamond.StopAnimation();
                        element.diamondSize = m_DiamondSize;
                    }
                }

                if (m_ActiveIndex >= 0 && m_ActiveIndex < m_Elements.Count)
                {
                    var element = m_Elements[m_ActiveIndex];
                    element.diamond.StartAnimation();
                    var targetSize = (int)(m_DiamondSize * activeScale);
                    element.diamondSize = targetSize;
                    while (element.diamondSize != targetSize)
                    {
                        yield return null;
                    }
                }
            }

            if (m_Coroutine != null)
            {
                CoroutineAnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = CoroutineAnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public DiamondBar()
        {
            m_Elements = new List<DiamondBarElement>();
            AddToClassList(k_UssClassName);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float barWidth = resolvedStyle.width;
            if (float.IsNaN(barWidth))
            {
                return;
            }

            // element size = total edge size / edge count + diamond size
            var elementSize = (barWidth - diamondSize * elementCount) / (elementCount * 2 - 2) + diamondSize;
            m_Elements.First().style.minWidth = elementSize;
            m_Elements.Last().style.minWidth = elementSize;
        }

        void CreateElements()
        {
            Clear();
            m_Elements.Clear();
            for (int i = 0; i < elementCount; i++)
            {
                var element = new DiamondBarElement() { name = $"element-{i}" };
                element.AddToClassList(k_ElementUssClassName);
                m_Elements.Add(element);
                Add(element);
            }

            var firstElement = m_Elements.First();
            firstElement.edge = BarElementEdge.Right;
            firstElement.AddToClassList(k_EndVariantElementUssClassName);

            var lastElement = m_Elements.Last();
            lastElement.edge = BarElementEdge.Left;
            lastElement.AddToClassList(k_EndVariantElementUssClassName);
            OnGeometryChanged(null);
        }
    }
}
