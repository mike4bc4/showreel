using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomControls
{
    public class Subtitle : VisualElement
    {
        public static readonly string ussClassName = "subtitle";
        public static readonly string labelUssClassName = ussClassName + "__label";
        public static readonly string borderUssClassName = ussClassName + "__border";

        public new class UxmlFactory : UxmlFactory<Subtitle, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription() { name = "text", defaultValue = "Subtitle" };
            UxmlBoolAttributeDescription m_Animated = new UxmlBoolAttributeDescription() { name = "animated", defaultValue = true };
            UxmlFloatAttributeDescription m_BorderScale = new UxmlFloatAttributeDescription() { name = "border-scale", defaultValue = 0.5f };
            UxmlFloatAttributeDescription m_AnimationDuration = new UxmlFloatAttributeDescription() { name = "animation-duration", defaultValue = 1.75f };
            UxmlFloatAttributeDescription m_AnimationDelay = new UxmlFloatAttributeDescription() { name = "animation-delay", defaultValue = 1.25f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Subtitle subtitle = (Subtitle)ve;
                subtitle.text = m_Text.GetValueFromBag(bag, cc);
                subtitle.animated = m_Animated.GetValueFromBag(bag, cc);
                subtitle.borderScale = m_BorderScale.GetValueFromBag(bag, cc);
                subtitle.animationDuration = m_AnimationDuration.GetValueFromBag(bag, cc);
                subtitle.animationDelay = m_AnimationDelay.GetValueFromBag(bag, cc);
            }
        }

        Label m_Label;
        VisualElement m_Border;
        VisualElementCoroutine m_Coroutine;
        bool m_Animated;
        bool m_PreviewMode;
        float m_BorderScale;
        float m_AnimationDuration;
        float m_AnimationDelay;

        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        public bool animated
        {
            get => m_Animated;
            set
            {
                m_Animated = value;
                if (m_Animated && m_Coroutine == null && (Application.isPlaying || m_PreviewMode))
                {
                    StartAnimation();
                }
                else
                {
                    StopAnimation();
                }
            }
        }

        public float borderScale
        {
            get => m_BorderScale;
            set => m_BorderScale = Mathf.Clamp01(value);
        }

        public float animationDuration
        {
            get => m_AnimationDuration;
            set => m_AnimationDuration = Mathf.Max(0f, value);
        }

        public float animationDelay
        {
            get => m_AnimationDelay;
            set => m_AnimationDelay = Mathf.Max(0f, value);
        }

        public Subtitle()
        {
            AddToClassList(ussClassName);

            m_Label = new Label() { name = "label" };
            m_Label.AddToClassList(labelUssClassName);
            Add(m_Label);

            m_Border = new VisualElement() { name = "border" };
            m_Border.AddToClassList(borderUssClassName);
            Add(m_Border);

            if (!Application.isPlaying)
            {
                RegisterCallback<AttachToPanelEvent>(evt => this.RegisterPreviewModeChangedCallback(OnPreviewModeChanged));
                RegisterCallback<DetachFromPanelEvent>(evt => this.UnregisterPreviewModeChangedCallback(OnPreviewModeChanged));
            }
        }

        void OnPreviewModeChanged(ChangeEvent<bool> evt)
        {
            m_PreviewMode = evt.newValue;
            if (evt.newValue)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }

        void StartAnimation()
        {
            if (panel == null)
            {
                EventCallback<GeometryChangedEvent> callback = null;
                callback = (evt) =>
                {
                    m_Coroutine = this.StartCoroutine(Coroutine());
                    m_Border.UnregisterCallback(callback);
                };

                m_Border.RegisterCallback(callback);
            }
            else
            {
                m_Coroutine = this.StartCoroutine(Coroutine());
            }
        }

        void StopAnimation()
        {
            m_Coroutine?.Stop();
            m_Coroutine = null;
            m_Border.style.scale = new Vector2(0f, 1f);
            m_Border.style.RemoveTransition("translate");
            m_Border.style.RemoveTransition("transform-origin");
            m_Border.style.translate = StyleKeyword.Null;
            m_Border.style.transformOrigin = StyleKeyword.Null;
        }

        IEnumerator Coroutine()
        {
            float width = m_Border.resolvedStyle.width;
            while (true)
            {
                m_Border.style.scale = new Vector2(borderScale, 1f);
                m_Border.style.RemoveTransition("translate");
                m_Border.style.RemoveTransition("transform-origin");
                m_Border.style.translate = new Translate(-width, 0f);
                m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(100f), Length.Percent(50f));

                yield return null;

                m_Border.style.AddTransition("translate", animationDuration, EasingMode.EaseOutSine);
                m_Border.style.AddTransition("transform-origin", animationDuration, EasingMode.EaseOutSine);
                m_Border.style.translate = new Translate(width, 0f);
                m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(0f), Length.Percent(50f));

                while (m_Border.resolvedStyle.translate != new Vector3(width, 0f, 0f))
                {
                    yield return null;
                }

                yield return new WaitForTime(animationDelay);
            }
        }
    }
}
