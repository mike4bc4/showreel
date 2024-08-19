using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace CustomControls
{
    public class DiamondTitle : VisualElement
    {
        const string k_UssClassName = "diamond-title";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_DiamondRightUssClassName = k_UssClassName + "__diamond-right";
        const string k_SeparatorUssClassName = k_UssClassName + "__separator";
        const string k_LabelUssClassName = k_UssClassName + "__label";
        const string k_MeasurementVariantLabelUssClassName = k_LabelUssClassName + "--measurement";
        const string k_LabelContainerUssClassName = k_UssClassName + "__label-container";

        public new class UxmlFactory : UxmlFactory<DiamondTitle, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Label" };
            UxmlFloatAttributeDescription m_AnimationProgress = new UxmlFloatAttributeDescription() { name = "animation-progress", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondTitle diamondTitle = (DiamondTitle)ve;
                diamondTitle.text = m_Text.GetValueFromBag(bag, cc);
                diamondTitle.animationProgress = m_AnimationProgress.GetValueFromBag(bag, cc);
            }
        }

        Diamond m_DiamondLeft;
        Diamond m_DiamondRight;
        Label m_Label;
        Label m_MeasurementLabel;
        VisualElement m_Separator;
        VisualElement m_LabelContainer;
        bool m_Unfolded;
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

        public Label label
        {
            get => m_Label;
        }

        public string text
        {
            get => m_Label.text;
            set
            {
                m_Label.text = value;
                m_MeasurementLabel.text = value;
            }
        }

        float unfoldedWidth
        {
            get => m_MeasurementLabel.resolvedStyle.marginLeft + m_MeasurementLabel.resolvedStyle.marginRight + m_MeasurementLabel.resolvedStyle.width;
        }

        public DiamondTitle()
        {
            m_Player = new AnimationPlayer();
            m_Player.sampling = 60;
            var animation = new KeyframeSystem.KeyframeAnimation();
            m_Player.AddAnimation(animation, "Animation");
            m_Player.animation = animation;

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

            m_MeasurementLabel = new Label();
            m_MeasurementLabel.name = "measurement-label";
            m_MeasurementLabel.pickingMode = PickingMode.Ignore;
            m_MeasurementLabel.AddToClassList(k_LabelUssClassName);
            m_MeasurementLabel.AddToClassList(k_MeasurementVariantLabelUssClassName);
            m_LabelContainer.Add(m_MeasurementLabel);

            m_Separator = new VisualElement();
            m_Separator.name = "separator";
            m_Separator.AddToClassList(k_SeparatorUssClassName);
            m_LabelContainer.Add(m_Separator);

            m_DiamondRight = new Diamond();
            m_DiamondRight.name = "diamond-right";
            m_DiamondRight.AddToClassList(k_DiamondUssClassName);
            m_DiamondRight.AddToClassList(k_DiamondRightUssClassName);
            Add(m_DiamondRight);

            var t1 = animation.AddTrack((float animationProgress) => m_DiamondRight.animationProgress = m_DiamondLeft.animationProgress = animationProgress);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(60, 1f);

            var t2 = animation.AddTrack((float widthScale) =>
            {
                if (!unfoldedWidth.IsNan())
                {
                    m_LabelContainer.style.width = unfoldedWidth * widthScale;
                }
                else
                {
                    void OnGeometryChanged(GeometryChangedEvent evt)
                    {
                        m_LabelContainer.style.width = unfoldedWidth * widthScale;
                        m_MeasurementLabel.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    }

                    m_MeasurementLabel.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                }
            });
            t2.AddKeyframe(60, 0f);
            t2.AddKeyframe(120, 1f);
        }
    }
}
