using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

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
            UxmlBoolAttributeDescription m_Unfolded = new UxmlBoolAttributeDescription() { name = "unfolded", defaultValue = true };
            UxmlColorAttributeDescription m_Color = new UxmlColorAttributeDescription() { name = "color", defaultValue = s_DefaultColor };
            UxmlIntAttributeDescription m_CornerRadius = new UxmlIntAttributeDescription() { name = "corner-radius", defaultValue = k_DefaultCornerRadius };
            UxmlFloatAttributeDescription m_FillAnimationDuration = new UxmlFloatAttributeDescription() { name = "fill-animation-duration", defaultValue = 0.75f };
            UxmlFloatAttributeDescription m_ResizeAnimationDuration = new UxmlFloatAttributeDescription() { name = "resize-animation-duration", defaultValue = 0.75f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondFrameVertical diamondFrameVertical = (DiamondFrameVertical)ve;
                diamondFrameVertical.unfolded = m_Unfolded.GetValueFromBag(bag, cc);
                diamondFrameVertical.color = m_Color.GetValueFromBag(bag, cc);
                diamondFrameVertical.cornerRadius = m_CornerRadius.GetValueFromBag(bag, cc);
                diamondFrameVertical.fillAnimationDuration = m_FillAnimationDuration.GetValueFromBag(bag, cc);
                diamondFrameVertical.resizeAnimationDuration = m_ResizeAnimationDuration.GetValueFromBag(bag, cc);
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
        int m_StateIndex;
        // TaskStatus m_Status;
        // CancellationTokenSource m_Cts;
        TaskPool m_FoldTaskPool;
        TaskPool m_UnfoldTaskPool;
        float m_FillAnimationDuration;
        float m_ResizeAnimationDuration;
        // int m_ScheduledTaskCount;
        TaskScheduler m_TaskScheduler;

        public bool ready
        {
            // get => m_Status.IsCompleted() && m_ScheduledTaskCount == 0;
            get => m_TaskScheduler.ready;
        }

        public float fillAnimationDuration
        {
            get => m_FillAnimationDuration;
            set => m_FillAnimationDuration = Mathf.Max(0f, value);
        }

        public float resizeAnimationDuration
        {
            get => m_ResizeAnimationDuration;
            set => m_ResizeAnimationDuration = Mathf.Max(0f, value);
        }

        // CancellationToken token
        // {
        //     get => m_Cts.Token;
        // }

        public VisualElement mainContainer
        {
            get => m_MainContainer;
        }

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
                    UnfoldImmediate();
                }
                else
                {
                    FoldImmediate();
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
            m_TaskScheduler = new TaskScheduler();
            m_FoldTaskPool = new TaskPool();
            m_UnfoldTaskPool = new TaskPool();

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

            m_UnfoldTaskPool.Add(async () =>
            {
                var t1 = m_DiamondTop.Unfold(m_TaskScheduler.token);
                var t2 = m_DiamondBottom.Unfold(m_TaskScheduler.token);
                await (t1, t2);
                m_StateIndex++;
            });

            m_FoldTaskPool.Add(async () =>
            {
                var t1 = m_DiamondTop.Fold(m_TaskScheduler.token);
                var t2 = m_DiamondBottom.Fold(m_TaskScheduler.token);
                await (t1, t2);
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(this, nameof(fill), 1f);
                animation.time = fillAnimationDuration;
                animation.timingFunction = TimingFunction.EaseOutSine;
                await animation.AsTask(m_TaskScheduler.token);
                m_StateIndex++;
            });

            m_FoldTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(this, nameof(fill), 0f);
                animation.time = fillAnimationDuration;
                animation.timingFunction = TimingFunction.EaseOutSine;
                await animation.AsTask(m_TaskScheduler.token);
                m_StateIndex--;
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                // Make current size inline to avoid resizing when container positioning changes.
                style.SetSize(resolvedStyle.GetSize());

                // Allow main container to match its content.
                m_MainContainer.style.position = Position.Absolute;
                m_MainContainer.style.height = StyleKeyword.Null;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    style.SetSize(StyleKeyword.Null);
                    m_MainContainer.style.position = StyleKeyword.Null;
                    m_MainContainer.style.height = 0f;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                var previousSize = resolvedStyle.GetSize();
                style.SetSize(StyleKeyword.Null);
                m_MainContainer.style.position = StyleKeyword.Null;
                m_MainContainer.style.height = 0f;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_StateIndex--;
                }
                catch (OperationCanceledException)
                {
                    style.SetSize(previousSize);
                    m_MainContainer.style.position = Position.Absolute;
                    m_MainContainer.style.height = StyleKeyword.Null;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            float targetHeight = 0f;
            Vector2 previousSize = new Vector2();

            m_UnfoldTaskPool.Add(async () =>
            {
                targetHeight = m_MainContainer.resolvedStyle.height;
                previousSize = resolvedStyle.GetSize();

                style.SetSize(StyleKeyword.Null);
                m_MainContainer.style.position = StyleKeyword.Null;
                m_MainContainer.style.height = 0f;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    style.SetSize(previousSize);
                    m_MainContainer.style.position = Position.Absolute;
                    m_MainContainer.style.height = StyleKeyword.Null;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                style.SetSize(previousSize);
                m_MainContainer.style.position = Position.Absolute;
                m_MainContainer.style.height = StyleKeyword.Null;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_StateIndex--;
                }
                catch (OperationCanceledException)
                {
                    style.SetSize(StyleKeyword.Null);
                    m_MainContainer.style.position = StyleKeyword.Null;
                    m_MainContainer.style.height = 0f;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                m_MainContainer.style.AddTransition("height", m_ResizeAnimationDuration, EasingMode.EaseOutCubic);
                m_MainContainer.style.height = targetHeight;

                try
                {
                    await UniTask.WaitUntil(() => m_MainContainer.resolvedStyle.height == targetHeight, cancellationToken: m_TaskScheduler.token);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    m_MainContainer.style.RemoveTransition("height");
                    m_MainContainer.style.height = m_MainContainer.resolvedStyle.height;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                m_MainContainer.style.AddTransition("height", m_ResizeAnimationDuration, EasingMode.EaseOutCubic);
                m_MainContainer.style.height = 0f;

                try
                {
                    await UniTask.WaitUntil(() => m_MainContainer.resolvedStyle.height == 0f, cancellationToken: m_TaskScheduler.token);
                    m_StateIndex--;
                }
                catch (OperationCanceledException)
                {
                    m_MainContainer.style.RemoveTransition("height");
                    m_MainContainer.style.height = m_MainContainer.resolvedStyle.height;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                m_MainContainer.style.height = StyleKeyword.Null;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    m_MainContainer.style.height = m_MainContainer.resolvedStyle.height;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                m_MainContainer.style.height = m_MainContainer.resolvedStyle.height;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_StateIndex--;
                }
                catch (OperationCanceledException)
                {
                    m_MainContainer.style.height = StyleKeyword.Null;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });
        }

        public void UnfoldImmediate()
        {
            m_TaskScheduler.Schedule(async () =>
            {
                m_StateIndex = m_FoldTaskPool.length - 1;

                m_DiamondBottom.UnfoldImmediate();
                m_DiamondTop.UnfoldImmediate();

                AnimationManager.StopAnimation(this, nameof(fill));
                fill = 1f;

                m_MainContainer.style.RemoveTransition("height");
                m_MainContainer.style.height = StyleKeyword.Null;

                style.SetSize(StyleKeyword.Null);

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization);
                var t2 = UniTask.WaitUntil(() => m_DiamondBottom.ready && m_DiamondTop.ready);

                await (t1, t2);
            });
        }

        public void FoldImmediate()
        {
            m_TaskScheduler.Schedule(async () =>
            {
                m_StateIndex = 0;

                m_DiamondBottom.FoldImmediate();
                m_DiamondTop.FoldImmediate();

                AnimationManager.StopAnimation(this, nameof(fill));
                fill = 0f;

                m_MainContainer.style.RemoveTransition("height");
                m_MainContainer.style.height = 0f;

                style.SetSize(StyleKeyword.Null);

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization);
                var t2 = UniTask.WaitUntil(() => m_DiamondBottom.ready && m_DiamondTop.ready);

                await (t1, t2);
            });
        }

        public UniTask Unfold(CancellationToken cancellationToken = default)
        {
            return m_TaskScheduler.Schedule(async () =>
            {
                await UniTask.NextFrame(m_TaskScheduler.token).Chain(m_UnfoldTaskPool.GetRange(m_StateIndex, m_UnfoldTaskPool.length - m_StateIndex));
            }, cancellationToken);
        }

        public UniTask Fold(CancellationToken cancellationToken = default)
        {
            return m_TaskScheduler.Schedule(async () =>
            {
                var functions = m_FoldTaskPool.GetRange(0, m_StateIndex + 1);
                functions.Reverse();
                await UniTask.NextFrame(m_TaskScheduler.token).Chain(functions);
            }, cancellationToken);
        }
    }
}
