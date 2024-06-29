using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondFrameVertical : VisualElement
    {
        const string k_UssClassName = "diamond-frame-vertical";
        const string k_DiamondTopUssClassName = k_UssClassName + "__diamond-top";
        const string k_DiamondBottomUssClassName = k_UssClassName + "__diamond-bottom";
        const string k_FrameContainerUssClassName = k_UssClassName + "__frame-container";
        const string k_RoundedFrameRightUssClassName = k_UssClassName + "__rounded-frame-right";
        const string k_RoundedFrameLeftUssClassName = k_UssClassName + "__rounded-frame-left";
        const string k_MainContainerUssClassName = k_UssClassName + "__main-container";
        const string k_MainContainerTransitionUssClassName = k_MainContainerUssClassName + "--transition";
        // const string k_TransitionUssClassName = k_UssClassName + "--transition";

        static readonly Color s_DefaultColor = Color.black;
        const float k_DefaultFill = 1f;
        const int k_DefaultCornerRadius = 10;

        public new class UxmlFactory : UxmlFactory<DiamondFrameVertical, UxmlTraits> { };

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlBoolAttributeDescription m_Unfolded = new UxmlBoolAttributeDescription() { name = "unfolded" };
            UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription() { name = "color", defaultValue = s_DefaultColor };
            UxmlFloatAttributeDescription m_Fill = new UxmlFloatAttributeDescription() { name = "fill", defaultValue = k_DefaultFill };
            UxmlIntAttributeDescription m_CornerRadius = new UxmlIntAttributeDescription() { name = "corner-radius", defaultValue = k_DefaultCornerRadius };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondFrameVertical diamondFrameVertical = (DiamondFrameVertical)ve;
                diamondFrameVertical.unfolded = m_Unfolded.GetValueFromBag(bag, cc);
                diamondFrameVertical.color = m_Color.GetValueFromBag(bag, cc);
                diamondFrameVertical.fill = m_Fill.GetValueFromBag(bag, cc);
                diamondFrameVertical.cornerRadius = m_CornerRadius.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondTop;
        Diamond m_DiamondBottom;
        VisualElement m_FrameContainer;
        RoundedFrame m_RoundedFrameRight;
        RoundedFrame m_RoundedFrameLeft;
        VisualElement m_MainContainer;
        bool m_Unfolded;
        Color m_Color;
        float m_Fill;
        int m_CornerRadius;
        Coroutine m_Coroutine;
        float m_DefaultHeight;

        public override VisualElement contentContainer
        {
            get => m_MainContainer;
        }

        bool unfolded
        {
            get => m_Unfolded;
            set
            {
                m_Unfolded = value;
                if (m_Unfolded)
                {
                    Unfold(immediate: true);
                }
                else
                {
                    Fold(immediate: true);
                }
            }
        }

        public float fill
        {
            get => m_Fill;
            set
            {
                m_Fill = Mathf.Clamp01(value);
                m_RoundedFrameLeft.fill = m_Fill;
                m_RoundedFrameRight.fill = m_Fill;
            }
        }

        public Color color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                m_RoundedFrameLeft.color = m_Color;
                m_RoundedFrameRight.color = m_Color;
            }
        }

        public int cornerRadius
        {
            get => m_CornerRadius;
            set
            {
                m_CornerRadius = Mathf.Max(1, value);
                m_RoundedFrameLeft.cornerRadius = m_CornerRadius;
                m_RoundedFrameRight.cornerRadius = m_CornerRadius;
            }
        }

        public DiamondFrameVertical()
        {
            AddToClassList(k_UssClassName);

            m_DiamondTop = new Diamond() { name = "diamond-top" };
            m_DiamondTop.AddToClassList(k_DiamondTopUssClassName);
            hierarchy.Add(m_DiamondTop);

            m_MainContainer = new VisualElement() { name = "main-container" };
            m_MainContainer.AddToClassList(k_MainContainerUssClassName);
            hierarchy.Add(m_MainContainer);

            m_DiamondBottom = new Diamond() { name = "diamond-bottom" };
            m_DiamondBottom.AddToClassList(k_DiamondBottomUssClassName);
            hierarchy.Add(m_DiamondBottom);

            m_FrameContainer = new VisualElement() { name = "frame-container" };
            m_FrameContainer.AddToClassList(k_FrameContainerUssClassName);
            hierarchy.Add(m_FrameContainer);

            m_RoundedFrameRight = new RoundedFrame() { name = "rounded-frame-right" };
            m_RoundedFrameRight.AddToClassList(k_RoundedFrameRightUssClassName);
            m_FrameContainer.Add(m_RoundedFrameRight);

            m_RoundedFrameLeft = new RoundedFrame() { name = "rounded-frame-left" };
            m_RoundedFrameLeft.AddToClassList(k_RoundedFrameLeftUssClassName);
            m_FrameContainer.Add(m_RoundedFrameLeft);

            color = s_DefaultColor;
            fill = k_DefaultFill;
            cornerRadius = k_DefaultCornerRadius;

            if (Application.isPlaying)
            {
                RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            m_DefaultHeight = m_DiamondTop.resolvedStyle.height + m_DiamondBottom.resolvedStyle.height;
            style.height = m_DefaultHeight;
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public Coroutine Unfold(bool immediate = false)
        {
            if (immediate)
            {
                m_MainContainer.RemoveFromClassList(k_MainContainerTransitionUssClassName);
                m_DiamondTop.Unfold(immediate: true);
                m_DiamondBottom.Unfold(immediate: true);
                m_MainContainer.style.height = StyleKeyword.Auto;
                fill = 1f;
                return null;
            }

            IEnumerator Coroutine()
            {
                m_DiamondTop.Unfold();
                yield return m_DiamondBottom.Unfold();

                var anim = AnimationManager.Animate(this, nameof(fill), 1f);
                anim.time = 1.25f;
                anim.timingFunction = TimingFunction.EaseInOutCubic;
                yield return anim.coroutine;

                // Set inline size to prevent changes when main container position changes.
                style.width = resolvedStyle.width;
                style.height = resolvedStyle.height;

                // Cache current main container height.
                float mainContainerHeight = m_MainContainer.resolvedStyle.height;

                // Hide main container and allow it to match it's content.
                m_MainContainer.style.visibility = Visibility.Hidden;
                m_MainContainer.style.position = Position.Absolute;
                m_MainContainer.style.height = StyleKeyword.Initial;

                // Wait until changes take effect.
                yield return null;

                // Measure main container and revert it's style changes.
                float targetHeight = m_MainContainer.resolvedStyle.height;
                m_MainContainer.style.visibility = StyleKeyword.Initial;
                m_MainContainer.style.position = StyleKeyword.Initial;

                // Set main container height in pixels, so transition can be started.
                m_MainContainer.style.height = mainContainerHeight;

                // Reset inline size to once again match main container.
                style.width = StyleKeyword.Initial;
                style.height = StyleKeyword.Initial;
                m_MainContainer.AddToClassList(k_MainContainerTransitionUssClassName);

                // Wait until changes take effect.
                yield return null;

                // Fire transition.
                m_MainContainer.style.height = targetHeight;
                while (m_MainContainer.resolvedStyle.height != targetHeight)
                {
                    yield return null;
                }

                // Reset main container inline height.
                m_MainContainer.style.height = StyleKeyword.Null;
            }

            if (m_Coroutine != null)
            {
                AnimationManager.Instance.StopCoroutine(m_Coroutine);
            }

            m_Coroutine = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public Coroutine Fold(bool immediate = false)
        {
            if (immediate)
            {
                m_MainContainer.RemoveFromClassList(k_MainContainerTransitionUssClassName);
                m_DiamondTop.Fold(immediate: true);
                m_DiamondBottom.Fold(immediate: true);
                m_MainContainer.style.height = 0f;
                fill = 0f;
                return null;
            }

            IEnumerator Coroutine()
            {
                // Reset inline styles that could be possibly changed by unfold coroutine.
                m_MainContainer.style.visibility = StyleKeyword.Null;
                m_MainContainer.style.position = StyleKeyword.Null;
                style.width = StyleKeyword.Null;
                style.height = StyleKeyword.Null;

                // Set inline height, so transition can be started.
                m_MainContainer.style.height = m_MainContainer.resolvedStyle.height;
                m_MainContainer.AddToClassList(k_MainContainerTransitionUssClassName);
                yield return null;

                m_MainContainer.style.height = 0f;
                while (m_MainContainer.resolvedStyle.height != 0f)
                {
                    yield return null;
                }

                var anim = AnimationManager.Animate(this, nameof(fill), 0f);
                anim.time = 1.25f;
                anim.timingFunction = TimingFunction.EaseInOutCubic;
                yield return anim.coroutine;

                m_DiamondTop.Fold();
                yield return m_DiamondBottom.Fold();
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
