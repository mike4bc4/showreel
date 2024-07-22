using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

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
        bool m_Animated;
        bool m_PreviewMode;
        float m_BorderScale;
        float m_AnimationDuration;
        float m_AnimationDelay;
        CancellationTokenSource m_Cts;
        bool m_RunningAnimation;
        TaskStatus m_Status;

        public bool ready
        {
            get => m_Status.IsCompleted() || m_RunningAnimation;
        }

        public string text
        {
            get => m_Label.text;
            set => m_Label.text = value;
        }

        bool animated
        {
            get => m_Animated;
            set
            {
                m_Animated = value;
                if (m_Animated)
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
            set
            {
                m_BorderScale = Mathf.Clamp01(value);
                m_Border.style.scale = new Vector2(m_BorderScale, 1f);
            }
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
        }

        void Stop()
        {
            if (m_Cts != null)
            {
                m_Cts.Cancel();
                m_Cts.Dispose();
                m_Cts = null;
            }
        }

        public void StopAnimation(bool reset = false)
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                StopAnimationTask(reset).Forget();
            })();
        }

        async UniTask StopAnimationTask(bool reset)
        {
            m_Status.SetPending();
            m_Border.style.RemoveTransition("translate");
            m_Border.style.RemoveTransition("transform-origin");

            if (reset)
            {
                m_Border.style.translate = new Translate(Length.Percent(-100f), Length.Percent(0f));
                m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(100f), Length.Percent(0f));
            }
            else
            {
                await UniTask.WaitWhile(() => float.IsNaN(m_Border.resolvedStyle.width));
                var width = m_Border.resolvedStyle.width;
                m_Border.style.translate = new Translate(Length.Percent(m_Border.resolvedStyle.translate.x / width * 100f), Length.Percent(0f));
                m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(m_Border.resolvedStyle.transformOrigin.x / width * 100f), Length.Percent(0f));
            }

            await UniTask.NextFrame(PlayerLoopTiming.Initialization);
            m_Status.SetCompleted();
        }

        public void StartAnimation(bool reset = false, CancellationToken ct = default)
        {
            Stop();
            m_Cts = ct != default ? CancellationTokenSource.CreateLinkedTokenSource(ct) : new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                // As StartAnimationTask awaits multiple times, it's easier to create single execution
                // handler here rather that multiple inside try-catch-finally blocks inside mentioned method.
                // This works only here because task can only end with exception because of infinite loop,
                // Otherwise we wouldn't be able to notice successful completion.
                StartAnimationTask(reset).Forget((e) =>
                {
                    m_Status.SetCompleted();
                    m_RunningAnimation = false;
                });
            })();
        }

        async UniTask StartAnimationTask(bool reset)
        {
            m_Status.SetPending();
            m_RunningAnimation = true;
            await UniTask.WaitWhile(() => float.IsNaN(m_Border.resolvedStyle.width), cancellationToken: m_Cts.Token);
            var width = m_Border.resolvedStyle.width;

            // Set initial style values as percentage, otherwise transition to percentage will not start.
            m_Border.style.translate = new Translate(reset ? Length.Percent(-100f) : Length.Percent(m_Border.resolvedStyle.translate.x / width * 100f), Length.Percent(0f));
            m_Border.style.transformOrigin = new TransformOrigin(reset ? Length.Percent(100f) : Length.Percent(m_Border.resolvedStyle.transformOrigin.x / width * 100f), Length.Percent(0f));

            try
            {
                while (true)
                {
                    m_Border.style.AddTransition("translate", animationDuration, EasingMode.EaseOutSine);
                    m_Border.style.AddTransition("transform-origin", animationDuration, EasingMode.EaseOutSine);
                    m_Border.style.translate = new Translate(Length.Percent(100f), Length.Percent(0f));
                    m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(0f), Length.Percent(0f));

                    await UniTask.WaitForSeconds(animationDuration * 2f, cancellationToken: m_Cts.Token);

                    m_Border.style.RemoveTransition("translate");
                    m_Border.style.RemoveTransition("transform-origin");
                    m_Border.style.translate = new Translate(Length.Percent(-100f), Length.Percent(0f));
                    m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(100f), Length.Percent(0f));

                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_Cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                m_Border.style.RemoveTransition("translate");
                m_Border.style.RemoveTransition("transform-origin");
                m_Border.style.translate = new Translate(Length.Percent(m_Border.resolvedStyle.translate.x / width * 100f), Length.Percent(0f));
                m_Border.style.transformOrigin = new TransformOrigin(Length.Percent(m_Border.resolvedStyle.transformOrigin.x / width * 100f), Length.Percent(0f));
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                throw;
            }
        }
    }
}
