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
    public class Diamond : VisualElement
    {
        const string k_UssClassName = "diamond";
        const string k_HalfUssClassName = k_UssClassName + "__half";

        public new class UxmlFactory : UxmlFactory<Diamond, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        VisualElement m_HalfLeft;
        VisualElement m_HalfRight;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;

        public bool ready
        {
            get => m_Status.IsCompleted();
        }

        public Diamond()
        {
            AddToClassList(k_UssClassName);

            m_HalfLeft = new VisualElement();
            m_HalfLeft.name = "half-left";
            m_HalfLeft.AddToClassList(k_HalfUssClassName);
            Add(m_HalfLeft);

            m_HalfRight = new VisualElement();
            m_HalfRight.name = "half-right";
            m_HalfRight.AddToClassList(k_HalfUssClassName);
            Add(m_HalfRight);
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
        public void UnfoldImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                UnfoldImmediateTask().Forget();
            })();
        }

        async UniTask UnfoldImmediateTask()
        {
            m_Status.SetPending();
            m_HalfLeft.style.RemoveTransition("width");
            m_HalfLeft.style.scale = new Vector2(-1f, 1f);
            await UniTask.NextFrame(PlayerLoopTiming.Initialization);
            m_Status.SetCompleted();
        }

        public void FoldImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                FoldImmediateTask().Forget();
            })();
        }

        async UniTask FoldImmediateTask()
        {
            m_Status.SetPending();
            m_HalfLeft.style.RemoveTransition("width");
            m_HalfLeft.style.scale = Vector2.one;
            await UniTask.NextFrame(PlayerLoopTiming.Initialization);
            m_Status.SetCompleted();
        }

        public UniTask Unfold(CancellationToken ct = default)
        {
            Stop();
            m_Cts = ct != default ? m_Cts = CancellationTokenSource.CreateLinkedTokenSource(ct) : new CancellationTokenSource();
            async UniTask Task()
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                await UnfoldTask();
            };

            return Task();
        }

        async UniTask UnfoldTask()
        {
            m_Status.SetPending();
            m_HalfLeft.style.AddTransition("scale", 0.5f, EasingMode.EaseInOutSine);
            m_HalfLeft.style.scale = new Vector2(-1f, 1f);

            try
            {
                await UniTask.WaitWhile(() => m_HalfLeft.resolvedStyle.scale != new Vector2(-1f, 1f), cancellationToken: m_Cts.Token);
            }
            catch (OperationCanceledException)
            {
                m_HalfLeft.style.RemoveTransition("scale");
                m_HalfLeft.style.scale = m_HalfLeft.resolvedStyle.scale;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                throw;
            }
            finally
            {
                m_Status.SetCompleted();
            }
        }

        public UniTask Fold(CancellationToken ct = default)
        {
            Stop();
            m_Cts = ct != default ? m_Cts = CancellationTokenSource.CreateLinkedTokenSource(ct) : new CancellationTokenSource();
            async UniTask Task()
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: m_Cts.Token);
                }

                await FoldTask();
            };

            return Task();
        }

        async UniTask FoldTask()
        {
            m_Status.SetPending();
            m_HalfLeft.style.AddTransition("scale", 0.5f, EasingMode.EaseInOutSine);
            m_HalfLeft.style.scale = Vector2.one;

            try
            {
                await UniTask.WaitWhile(() => m_HalfLeft.resolvedStyle.scale != Vector2.one, cancellationToken: m_Cts.Token);
            }
            catch (OperationCanceledException)
            {
                m_HalfLeft.style.RemoveTransition("scale");
                m_HalfLeft.style.scale = m_HalfLeft.resolvedStyle.scale;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                throw;
            }
            finally
            {
                m_Status.SetCompleted();
            }
        }
    }
}
