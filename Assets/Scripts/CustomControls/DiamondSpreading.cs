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
    public class DiamondSpreading : VisualElement
    {
        const string k_UssClassName = "diamond-spreading";
        const string k_CornerContainerUssClassName = k_UssClassName + "__corner-container";
        const string k_CornerUssClassName = k_UssClassName + "__corner";
        const string k_CornerNUssClassName = k_UssClassName + "__corner-n";
        const string k_CornerSUssClassName = k_UssClassName + "__corner-s";
        const string k_CornerEUssClassName = k_UssClassName + "__corner-e";
        const string k_CornerWUssClassName = k_UssClassName + "__corner-w";
        const string k_CornerBodyUssClassName = k_UssClassName + "__corner-body";

        // Defines amount of overlapping which allows to avoid gaps between elements.
        const float k_SpreadEpsilon = 0.05f;

        public new class UxmlFactory : UxmlFactory<DiamondSpreading, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_EdgeWidth = new UxmlFloatAttributeDescription() { name = "edge-width", defaultValue = 0.3f };
            UxmlFloatAttributeDescription m_Spread = new UxmlFloatAttributeDescription() { name = "spread", defaultValue = 1f };
            UxmlFloatAttributeDescription m_Fill = new UxmlFloatAttributeDescription() { name = "fill", defaultValue = 1f };
            UxmlFloatAttributeDescription m_AnimationDuration = new UxmlFloatAttributeDescription() { name = "animation-duration", defaultValue = 0.5f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DiamondSpreading diamondFolded = (DiamondSpreading)ve;
                diamondFolded.edgeWidth = m_EdgeWidth.GetValueFromBag(bag, cc);
                diamondFolded.spread = m_Spread.GetValueFromBag(bag, cc);
                diamondFolded.fill = m_Fill.GetValueFromBag(bag, cc);
                diamondFolded.animationDuration = m_AnimationDuration.GetValueFromBag(bag, cc);
            }
        }

        VisualElement m_CornerContainer;
        VisualElement m_CornerW;
        VisualElement m_CornerN;
        VisualElement m_CornerE;
        VisualElement m_CornerS;
        List<VisualElement> m_CornerBodies;
        float m_EdgeWidth;
        float m_Spread;
        float m_Fill;
        float m_AnimationDuration;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;
        TaskPool m_UnfoldTaskPool;
        TaskPool m_FoldTaskPool;
        int m_StateIndex;

        public bool ready
        {
            get => m_Status.IsCompleted();
        }

        CancellationToken token
        {
            get => m_Cts.Token;
        }

        public float animationDuration
        {
            get => m_AnimationDuration;
            set => m_AnimationDuration = Mathf.Max(0f, value);
        }

        public float fill
        {
            get => m_Fill;
            set
            {
                m_Fill = Mathf.Clamp01(value);
                float scaleFactor = m_Fill * (1f - 2f * edgeWidth) / edgeWidth;
                foreach (var cornerBody in m_CornerBodies)
                {
                    cornerBody.style.scale = new Vector2(1 + scaleFactor, 1);
                }
            }
        }

        public float spread
        {
            get => m_Spread;
            set
            {
                m_Spread = Mathf.Clamp01(value);
                var offset = (edgeWidth / 2f + m_Spread * (0.5f - edgeWidth)) * (1 - k_SpreadEpsilon);
                m_CornerN.style.translate = new Translate(Length.Percent(-offset * 100f), Length.Percent(-offset * 100f));
                m_CornerW.style.translate = new Translate(Length.Percent(-offset * 100f), Length.Percent(offset * 100f));
                m_CornerS.style.translate = new Translate(Length.Percent(offset * 100f), Length.Percent(offset * 100f));
                m_CornerE.style.translate = new Translate(Length.Percent(offset * 100f), Length.Percent(-offset * 100f));
            }
        }

        public float edgeWidth
        {
            get => m_EdgeWidth;
            set
            {
                m_EdgeWidth = Mathf.Clamp01(value);
                m_CornerN.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerW.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerS.style.scale = Vector2.one * m_EdgeWidth;
                m_CornerE.style.scale = Vector2.one * m_EdgeWidth;

                // Update dependant properties.
                spread = m_Spread;
                fill = m_Fill;
            }
        }

        public float initialEdgeWidth { get; set; }

        public float targetEdgeWidth { get; set; }

        List<VisualElement> corners
        {
            get => new List<VisualElement>() { m_CornerW, m_CornerN, m_CornerE, m_CornerS };
        }

        public DiamondSpreading()
        {
            m_FoldTaskPool = new TaskPool();
            m_UnfoldTaskPool = new TaskPool();
            m_CornerBodies = new List<VisualElement>();

            AddToClassList(k_UssClassName);

            m_CornerContainer = new VisualElement() { name = "corner-container" };
            m_CornerContainer.AddToClassList(k_CornerContainerUssClassName);
            Add(m_CornerContainer);


            m_CornerN = new VisualElement() { name = "corner-n" };
            m_CornerN.AddToClassList(k_CornerUssClassName);
            m_CornerN.AddToClassList(k_CornerNUssClassName);
            m_CornerContainer.Add(m_CornerN);

            var cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerN.Add(cornerBody);

            m_CornerW = new VisualElement() { name = "corner-w" };
            m_CornerW.AddToClassList(k_CornerUssClassName);
            m_CornerW.AddToClassList(k_CornerWUssClassName);
            m_CornerContainer.Add(m_CornerW);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerW.Add(cornerBody);

            m_CornerS = new VisualElement() { name = "corner-s" };
            m_CornerS.AddToClassList(k_CornerUssClassName);
            m_CornerS.AddToClassList(k_CornerSUssClassName);
            m_CornerContainer.Add(m_CornerS);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerS.Add(cornerBody);

            m_CornerE = new VisualElement() { name = "corner-e" };
            m_CornerE.AddToClassList(k_CornerUssClassName);
            m_CornerE.AddToClassList(k_CornerEUssClassName);
            m_CornerContainer.Add(m_CornerE);

            cornerBody = new VisualElement() { name = "corner-body" };
            m_CornerBodies.Add(cornerBody);
            cornerBody.AddToClassList(k_CornerBodyUssClassName);
            m_CornerE.Add(cornerBody);

            initialEdgeWidth = 0.15f;
            targetEdgeWidth = 0.3f;

            m_UnfoldTaskPool.Add(async () =>
            {
                var animation1 = AnimationManager.Animate(this, nameof(spread), 1f);
                animation1.time = animationDuration * 0.4f;
                animation1.timingFunction = TimingFunction.EaseOutCubic;

                var animation2 = AnimationManager.Animate(this, nameof(edgeWidth), targetEdgeWidth);
                animation2.time = animationDuration * 0.4f;
                animation2.timingFunction = TimingFunction.EaseOutCubic;

                await (animation1.AsTask(token), animation2.AsTask(token));
                m_StateIndex++;
            });

            m_FoldTaskPool.Add(async () =>
            {
                var animation1 = AnimationManager.Animate(this, nameof(spread), 0f);
                animation1.time = animationDuration * 0.4f;
                animation1.timingFunction = TimingFunction.EaseOutCubic;

                var animation2 = AnimationManager.Animate(this, nameof(edgeWidth), initialEdgeWidth);
                animation2.time = animationDuration * 0.4f;
                animation2.timingFunction = TimingFunction.EaseOutCubic;

                await (animation1.AsTask(token), animation2.AsTask(token));
            });

            m_UnfoldTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(this, nameof(fill), 1f);
                animation.time = animationDuration * 0.6f;
                animation.timingFunction = TimingFunction.EaseOutCubic;

                await animation.AsTask(token);
            });

            m_FoldTaskPool.Add(async () =>
           {
               var animation = AnimationManager.Animate(this, nameof(fill), 0f);
               animation.time = animationDuration * 0.6f;
               animation.timingFunction = TimingFunction.EaseOutCubic;

               await animation.AsTask(token);
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

                AnimationManager.StopAnimation(this, nameof(edgeWidth));
                AnimationManager.StopAnimation(this, nameof(spread));
                AnimationManager.StopAnimation(this, nameof(fill));

                edgeWidth = initialEdgeWidth;
                spread = 0f;
                fill = 0f;

                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
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

                AnimationManager.StopAnimation(this, nameof(edgeWidth));
                AnimationManager.StopAnimation(this, nameof(spread));
                AnimationManager.StopAnimation(this, nameof(fill));

                edgeWidth = targetEdgeWidth;
                spread = 1f;
                fill = 1f;

                await UniTask.NextFrame(PlayerLoopTiming.Initialization);
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
