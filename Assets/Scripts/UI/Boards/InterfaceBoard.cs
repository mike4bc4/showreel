using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI.Boards
{
    public class InterfaceBoard : Board, IBoard
    {
        public const int SortingOrder = 1000;   // Sorting order affects UI element picking.
        public const int DisplayOrder = 1000;   // Display order affects Layer sorting

        [SerializeField] VisualTreeAsset m_ControlsVta;
        [SerializeField] float m_FadeTime;

        Layer m_ControlsLayer;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;
        int m_StateIndex;
        TaskPool m_ShowTaskPool;
        TaskPool m_HideTaskPool;

        CancellationToken token
        {
            get => m_Cts.Token;
        }

        public void Init()
        {
            m_ShowTaskPool = new TaskPool();
            m_HideTaskPool = new TaskPool();

            m_ControlsLayer = LayerManager.CreateLayer(m_ControlsVta, "InterfaceControls");
            m_ControlsLayer.displayOrder = DisplayOrder;
            m_ControlsLayer.interactable = false;
            m_ControlsLayer.alpha = 0f;
            m_ControlsLayer.blurSize = Layer.DefaultBlurSize;
            m_ControlsLayer.panelSortingOrder = SortingOrder;

            m_ShowTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(token);
                await UniTask.WaitForSeconds(m_FadeTime / 2f, cancellationToken: token);
                m_StateIndex++;
            });

            m_HideTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;
                await animation.AsTask(token);
            });

            m_ShowTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;
                await animation.AsTask(token);
                m_StateIndex++;
            });

            m_HideTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(token);
                await UniTask.WaitForSeconds(m_FadeTime / 2f, cancellationToken: token);
                m_StateIndex--;
            });

            m_ShowTaskPool.Add(() =>
            {
                m_ControlsLayer.interactable = true;
            });

            m_HideTaskPool.Add(() =>
            {
                m_ControlsLayer.interactable = false;
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

        public void ShowImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = m_ShowTaskPool.length - 1;
                m_ControlsLayer.interactable = true;
                m_ControlsLayer.alpha = 1f;
                m_ControlsLayer.blurSize = 0f;
                m_Status.SetCompleted();
            })();
        }

        public void HideImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = 0;
                m_ControlsLayer.interactable = false;
                m_ControlsLayer.alpha = 0f;
                m_ControlsLayer.blurSize = 1f;
                m_Status.SetCompleted();
            })();
        }

        public UniTask Show(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();

                try
                {
                    await UniTask.NextFrame(token).Chain(m_ShowTaskPool.GetRange(m_StateIndex, m_ShowTaskPool.length - m_StateIndex));
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });

            return task;
        }

        public UniTask Hide(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                var functions = m_HideTaskPool.GetRange(0, m_StateIndex + 1);
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

            return task;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            Show(default(CancellationToken));
        }

        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Z))
        //     {
        //         Show(default(CancellationToken));
        //     }
        //     else if (Input.GetKeyDown(KeyCode.C))
        //     {
        //         Hide(default(CancellationToken));
        //     }
        // }
    }
}
