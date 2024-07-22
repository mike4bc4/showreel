using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace CustomControls
{
    public class DiamondBullet : VisualElement
    {
        const string k_UssClassName = "diamond-bullet";
        const string k_LineContainerUssClassName = k_UssClassName + "__line-container";
        const string k_LineUssClassName = k_UssClassName + "__line";
        const string k_DiamondUssClassName = k_UssClassName + "__diamond";
        const string k_SpacerUssClassName = k_UssClassName + "__spacer";

        const float k_SpacerAnimationFlex = 1f;
        const float k_LineAnimationFlex = 2f;
        const float k_DiamondAnimationFlex = 3f;

        public new class UxmlFactory : UxmlFactory<DiamondBullet, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_AnimationDuration = new UxmlFloatAttributeDescription() { name = "animation-duration", defaultValue = 1f };
            UxmlBoolAttributeDescription m_Unfolded = new UxmlBoolAttributeDescription() { name = "unfolded", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondBullet diamondBullet = (DiamondBullet)ve;
                diamondBullet.animationDuration = m_AnimationDuration.GetValueFromBag(bag, cc);
                diamondBullet.unfolded = m_Unfolded.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_LineContainer;
        VisualElement m_Line;
        DiamondSpreading m_Diamond;
        VisualElement m_Spacer;
        bool m_Unfolded;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;
        int m_StateIndex;
        TaskPool m_UnfoldTaskPool;
        TaskPool m_FoldTaskPool;
        float m_AnimationDuration;

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

        float diamondAnimationDuration => m_AnimationDuration * (k_DiamondAnimationFlex / animationFlex);

        float lineAnimationDuration => m_AnimationDuration * (k_LineAnimationFlex / animationFlex);

        float spacerAnimationDuration => m_AnimationDuration * (k_SpacerAnimationFlex / animationFlex);

        float animationFlex => k_SpacerAnimationFlex + k_LineAnimationFlex + k_DiamondAnimationFlex;

        CancellationToken token
        {
            get => m_Cts.Token;
        }

        float animationDuration
        {
            get => m_AnimationDuration;
            set => m_AnimationDuration = Mathf.Max(0f, value);
        }

        public DiamondBullet()
        {
            m_UnfoldTaskPool = new TaskPool();
            m_FoldTaskPool = new TaskPool();

            AddToClassList(k_UssClassName);

            m_Spacer = new VisualElement() { name = "spacer" };
            m_Spacer.AddToClassList(k_SpacerUssClassName);
            Add(m_Spacer);

            m_Diamond = new DiamondSpreading() { name = "diamond" };
            m_Diamond.AddToClassList(k_DiamondUssClassName);
            Add(m_Diamond);

            m_Line = new VisualElement() { name = "line" };
            m_Line.AddToClassList(k_LineUssClassName);
            m_Spacer.Add(m_Line);

            m_UnfoldTaskPool.Add(async () =>
            {
                m_Diamond.style.AddTransition("scale", spacerAnimationDuration);
                m_Diamond.style.scale = Vector2.one;

                m_Spacer.style.AddTransition("flex-grow", spacerAnimationDuration, EasingMode.EaseIn);
                m_Spacer.style.flexGrow = 1f;

                try
                {
                    await UniTask.WaitUntil(() => m_Diamond.resolvedStyle.scale == Vector2.one && m_Spacer.resolvedStyle.flexGrow == 1f);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    m_Diamond.style.RemoveTransition("scale");
                    m_Diamond.style.scale = m_Diamond.resolvedStyle.scale;

                    m_Spacer.style.RemoveTransition("flex-grow");
                    m_Spacer.style.flexGrow = m_Diamond.resolvedStyle.flexGrow;

                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                m_Diamond.style.AddTransition("scale", spacerAnimationDuration);
                m_Diamond.style.scale = Vector2.zero;

                m_Spacer.style.AddTransition("flex-grow", spacerAnimationDuration, EasingMode.EaseIn);
                m_Spacer.style.flexGrow = 0f;

                try
                {
                    await UniTask.WaitUntil(() => m_Diamond.resolvedStyle.scale == Vector2.zero && m_Spacer.resolvedStyle.flexGrow == 0f);
                }
                catch (OperationCanceledException)
                {
                    m_Diamond.style.RemoveTransition("scale");
                    m_Diamond.style.scale = m_Diamond.resolvedStyle.scale;

                    m_Spacer.style.RemoveTransition("flex-grow");
                    m_Spacer.style.flexGrow = m_Diamond.resolvedStyle.flexGrow;

                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                m_Line.style.AddTransition("width", lineAnimationDuration, EasingMode.EaseInOut);
                m_Line.style.width = Length.Percent(0f);

                try
                {
                    await UniTask.WaitForSeconds(lineAnimationDuration, cancellationToken: token);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    var parent = m_Line.hierarchy.parent;
                    if (parent.resolvedStyle.width.IsNan())
                    {
                        await UniTask.WaitUntil(() => !parent.resolvedStyle.width.IsNan());
                    }

                    m_Line.style.RemoveTransition("width");
                    m_Line.style.width = Length.Percent(m_Line.resolvedStyle.width / parent.resolvedStyle.width * 100f);

                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_FoldTaskPool.Add(async () =>
            {
                m_Line.style.AddTransition("width", lineAnimationDuration, EasingMode.EaseInOut);
                m_Line.style.width = Length.Percent(100f);

                try
                {
                    await UniTask.WaitForSeconds(lineAnimationDuration, cancellationToken: token);
                    m_StateIndex--;
                }
                catch (OperationCanceledException)
                {
                    var parent = m_Line.hierarchy.parent;
                    if (parent.resolvedStyle.width.IsNan())
                    {
                        await UniTask.WaitUntil(() => !parent.resolvedStyle.width.IsNan());
                    }

                    m_Line.style.RemoveTransition("width");
                    m_Line.style.width = Length.Percent(m_Line.resolvedStyle.width / parent.resolvedStyle.width * 100f);

                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                m_Diamond.animationDuration = diamondAnimationDuration;
                await m_Diamond.Unfold(token);
            });

            m_FoldTaskPool.Add(async () =>
            {
                m_Diamond.animationDuration = diamondAnimationDuration;
                await m_Diamond.Fold(token);
                m_StateIndex--;
            });
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

        public void FoldImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = 0;

                m_Diamond.style.RemoveTransition("scale");
                m_Diamond.style.scale = Vector2.zero;

                m_Spacer.style.RemoveTransition("flex-grow");
                m_Spacer.style.flexGrow = 0f;

                m_Line.style.RemoveTransition("width");
                m_Line.style.width = Length.Percent(100f);

                m_Diamond.FoldImmediate();

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization);
                var t2 = UniTask.WaitUntil(() => m_Diamond.ready);
                await (t1, t2);

                m_Status.SetCompleted();
            });
        }

        public void UnfoldImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = m_UnfoldTaskPool.length - 1;

                m_Diamond.style.RemoveTransition("scale");
                m_Diamond.style.scale = Vector2.one;

                m_Spacer.style.RemoveTransition("flex-grow");
                m_Spacer.style.flexGrow = 1f;

                m_Line.style.RemoveTransition("width");
                m_Line.style.width = Length.Percent(0f);

                m_Diamond.UnfoldImmediate();

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization);
                var t2 = UniTask.WaitUntil(() => m_Diamond.ready);
                await (t1, t2);

                m_Status.SetCompleted();
            });
        }

        public UniTask Fold(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
            return UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                var functions = m_FoldTaskPool.GetRange(0, m_StateIndex + 1);
                functions.Reverse();

                try
                {
                    await UniTask.NextFrame(token).Chain(functions);
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });
        }

        public UniTask Unfold(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
            return UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();

                try
                {
                    await UniTask.NextFrame(token).Chain(m_UnfoldTaskPool.GetRange(m_StateIndex, m_UnfoldTaskPool.length - m_StateIndex));
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });
        }
    }
}
