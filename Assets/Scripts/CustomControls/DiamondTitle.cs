using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class DiamondTitle : VisualElement
    {
        const string k_UssClassName = "diamond-title";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_DiamondRightUssClassName = k_UssClassName + "__diamond-right";
        const string k_SeparatorUssClassName = k_UssClassName + "__separator";
        const string k_LabelUssClassName = k_UssClassName + "__label";
        const string k_LabelContainerUssClassName = k_UssClassName + "__label-container";

        public new class UxmlFactory : UxmlFactory<DiamondTitle, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlBoolAttributeDescription m_Unfolded = new UxmlBoolAttributeDescription() { name = "unfolded", defaultValue = true };
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Label" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondTitle diamondTitle = (DiamondTitle)ve;
                diamondTitle.unfolded = m_Unfolded.GetValueFromBag(bag, cc);
                diamondTitle.text = m_Text.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondLeft;
        Diamond m_DiamondRight;
        Label m_Label;
        VisualElement m_Separator;
        VisualElement m_LabelContainer;
        Coroutine m_CoroutineHandle;
        bool m_Unfolded;

        public Label label
        {
            get => m_Label;
        }

        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
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

        public DiamondTitle()
        {
            AddToClassList(k_UssClassName);

            m_DiamondLeft = new Diamond();
            m_DiamondLeft.name = "diamond-left";
            m_DiamondLeft.AddToClassList(k_DiamondUssClassName);
            Add(m_DiamondLeft);

            m_LabelContainer = new VisualElement();
            m_LabelContainer.name = "label-container";
            m_LabelContainer.AddToClassList(k_LabelContainerUssClassName);
            Add(m_LabelContainer);

            m_Label = new Label();
            m_Label.name = "label";
            m_Label.AddToClassList(k_LabelUssClassName);
            m_LabelContainer.Add(m_Label);

            m_Separator = new VisualElement();
            m_Separator.name = "separator";
            m_Separator.AddToClassList(k_SeparatorUssClassName);
            m_LabelContainer.Add(m_Separator);

            m_DiamondRight = new Diamond();
            m_DiamondRight.name = "diamond-right";
            m_DiamondRight.AddToClassList(k_DiamondUssClassName);
            m_DiamondRight.AddToClassList(k_DiamondRightUssClassName);
            Add(m_DiamondRight);
        }

        void UnfoldImmediate()
        {
            m_LabelContainer.style.RemoveTransition("width");
            m_DiamondLeft.Unfold(immediate: true);
            m_DiamondRight.Unfold(immediate: true);
            m_LabelContainer.style.width = StyleKeyword.Auto;
            m_Label.style.visibility = StyleKeyword.Null;
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
                m_DiamondLeft.Unfold();
                yield return m_DiamondRight.Unfold();

                m_LabelContainer.style.AddTransition("width", 0.5f, EasingMode.EaseInOutSine);
                m_Label.style.position = Position.Absolute;
                yield return null;  // Wait until label position is updated to read its width correctly. 

                m_Label.style.position = StyleKeyword.Initial;  // Revert label's position.
                var targetWidth = m_Label.resolvedStyle.width + m_Label.resolvedStyle.marginLeft + m_Label.resolvedStyle.marginRight;
                m_LabelContainer.style.width = targetWidth;
                while (m_LabelContainer.resolvedStyle.width != targetWidth)
                {
                    yield return null;
                }

                m_LabelContainer.style.width = StyleKeyword.Auto;
                m_Label.style.visibility = StyleKeyword.Null;
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }

        void FoldImmediate()
        {
            m_LabelContainer.style.RemoveTransition("width");
            m_DiamondLeft.Fold(immediate: true);
            m_DiamondRight.Fold(immediate: true);
            m_LabelContainer.style.width = 0f;
            m_Label.style.visibility = Visibility.Hidden;
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

            IEnumerator Coroutine(bool immediate = false)
            {
                UnfoldImmediate();
                yield return null;  // Wait until unfolded with is recalculated.

                m_Label.style.visibility = Visibility.Hidden;
                m_LabelContainer.style.width = m_LabelContainer.resolvedStyle.width;
                m_LabelContainer.style.AddTransition("width", 0.5f, EasingMode.EaseInOutSine);
                m_LabelContainer.style.width = 0f;
                while (m_LabelContainer.resolvedStyle.width != 0f)
                {
                    yield return null;
                }

                m_DiamondLeft.Fold();
                yield return m_DiamondRight.Fold();
            }

            m_CoroutineHandle = AnimationManager.Instance.StartCoroutine(Coroutine());
            return m_CoroutineHandle;
        }
    }
}
