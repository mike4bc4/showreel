using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomControls;
using Cysharp.Threading.Tasks;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        [SerializeField] VisualTreeAsset m_InitialBoardVta;
        [SerializeField] VisualTreeAsset m_EmptyVta;
        [SerializeField] float m_FadeTime;

        Layer m_InitialBoardLayer;
        DiamondTitle m_Title;
        Subtitle m_Subtitle;
        int m_StateIndex;
        TaskPool m_ShowFunctions;
        TaskPool m_HideFunctions;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;

        CancellationToken token
        {
            get => m_Cts.Token;
        }

        public bool ready
        {
            get => m_Status.IsCompleted();
        }

        public void Init()
        {
            m_ShowFunctions = new TaskPool();
            m_HideFunctions = new TaskPool();

            m_ShowFunctions.Add(async () =>
            {
                m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVta);
                m_InitialBoardLayer.alpha = 0f;

                m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>("title");
                m_Title.label.visible = false;
                m_Title.FoldImmediate();

                m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>("subtitle");
                m_Subtitle.StopAnimation(true);

                try
                {
                    var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization, token);   // Wait for UI styles to apply and layer creation to finish.
                    var t2 = UniTask.WaitUntil(() => m_Title.ready && m_Subtitle.ready, cancellationToken: token);

                    await (t1, t2);
                    m_StateIndex++;
                }
                catch (OperationCanceledException)
                {
                    // There is no need to revert state of title and subtitle, as both will be removed with layer.
                    LayerManager.RemoveLayer(m_InitialBoardLayer);
                    throw;
                }
            });

            m_HideFunctions.Add(() =>
            {
                LayerManager.RemoveLayer(m_InitialBoardLayer);
            });

            m_ShowFunctions.Add(async () =>
            {
                m_InitialBoardLayer.MaskElements(m_Title, m_Subtitle);
                m_InitialBoardLayer.alpha = 1f;

                var layer = await m_InitialBoardLayer.CreateSnapshotLayerAsync(m_Title, "TitleSnapshotLayer", token);
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                m_StateIndex++;
            });

            m_HideFunctions.Add(() =>
            {
                m_InitialBoardLayer.UnmaskElements(m_Title, m_Subtitle);
                m_InitialBoardLayer.alpha = 0f;

                LayerManager.RemoveLayer("TitleSnapshotLayer");
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;

                await animation.AsTask(token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;

                await animation.AsTask(token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(() =>
            {
                m_InitialBoardLayer.UnmaskElements(m_Title);
                LayerManager.RemoveLayer("TitleSnapshotLayer");
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                await m_InitialBoardLayer.CreateSnapshotLayerAsync(m_Title, "TitleSnapshotLayer", token);

                m_InitialBoardLayer.MaskElements(m_Title);   // Hide when snapshot becomes visible.
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                await m_Title.Unfold(token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                await m_Title.Fold(token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(() =>
            {
                m_Title.label.visible = true;
                var layer = m_InitialBoardLayer.CreateSnapshotLayer(m_Title.label, "TitleLabelSnapshotLayer");
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                m_InitialBoardLayer.MaskElements(m_Title.label);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                await m_Title.label.style.SetPropertyAsync(nameof(IStyle.visibility), new StyleEnum<Visibility>(Visibility.Hidden), token);
                LayerManager.RemoveLayer("TitleLabelSnapshotLayer");
                m_InitialBoardLayer.UnmaskElements(m_Title.label);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_Cts.Token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_Cts.Token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;

                await animation.AsTask(m_Cts.Token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;

                await animation.AsTask(m_Cts.Token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_Cts.Token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_Cts.Token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(() =>
            {
                m_InitialBoardLayer.UnmaskElements(m_Title.label);
                LayerManager.RemoveLayer("TitleLabelSnapshotLayer");
                var layer = m_InitialBoardLayer.CreateSnapshotLayer(m_Subtitle, "SubtitleSnapshotLayer");
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                await m_InitialBoardLayer.CreateSnapshotLayerAsync(m_Title.label, "TitleLabelSnapshotLayer", token);

                m_InitialBoardLayer.MaskElements(m_Title.label); // Hide when snapshot becomes visible.
                LayerManager.RemoveLayer("SubtitleSnapshotLayer");
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("SubtitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_Cts.Token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_Cts.Token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("SubtitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;

                await animation.AsTask(m_Cts.Token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("SubtitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;

                await animation.AsTask(m_Cts.Token);
                m_StateIndex++;
            });

            m_HideFunctions.Add(async () =>
            {
                var layer = LayerManager.GetLayer("SubtitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_Cts.Token);

                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_Cts.Token);
                m_StateIndex--;
            });

            m_ShowFunctions.Add(() =>
            {
                LayerManager.RemoveLayer("SubtitleSnapshotLayer");
                m_InitialBoardLayer.UnmaskElements(m_Subtitle);
                m_Subtitle.StartAnimation();
            });

            m_HideFunctions.Add(async () =>
            {
                await m_InitialBoardLayer.CreateSnapshotLayerAsync(m_Subtitle, "SubtitleSnapshotLayer", token);

                m_InitialBoardLayer.MaskElements(m_Subtitle);    // Hide when snapshot becomes visible.
                m_Subtitle.StopAnimation(true);
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

                ShowImmediateTask().Forget();
            })();
        }

        async UniTask ShowImmediateTask()
        {
            m_Status.SetPending();
            m_StateIndex = m_ShowFunctions.length - 1;

            m_InitialBoardLayer = (Layer)LayerManager.GetLayer(m_InitialBoardVta.name);
            if (m_InitialBoardLayer == null)
            {
                m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVta);
            }

            m_InitialBoardLayer.alpha = 0f;
            m_InitialBoardLayer.Unmask();

            m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>("title");
            m_Title.label.visible = true;
            m_Title.UnfoldImmediate();

            m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>("subtitle");
            m_Subtitle.StartAnimation(true);

            // Suppressing cancellation as this task is always supposed to finish.
            await UniTask.WaitUntil(() => m_Subtitle.ready && m_Title.ready).SuppressCancellationThrow();
            m_InitialBoardLayer.Clear();    // Clear layer texture as it may contain remains of old UI when reused (not recreated).

            m_InitialBoardLayer.alpha = 1f;
            LayerManager.RemoveLayer("TitleSnapshotLayer");
            LayerManager.RemoveLayer("TitleLabelSnapshotLayer");
            LayerManager.RemoveLayer("SubtitleSnapshotLayer");
            m_Status.SetCompleted();
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
                LayerManager.RemoveLayer(m_InitialBoardLayer);
                LayerManager.RemoveLayer("TitleSnapshotLayer");
                LayerManager.RemoveLayer("TitleLabelSnapshotLayer");
                LayerManager.RemoveLayer("SubtitleSnapshotLayer");
                m_Status.SetCompleted();
            })();
        }

        public UniTask Show(CancellationToken ct = default)
        {
            Stop();
            m_Cts = ct != default ? CancellationTokenSource.CreateLinkedTokenSource(ct) : new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                // Everything is happening synchronously, so there is no need to await for task or change state to pending.
                m_Status.SetPending();
                try
                {
                    await UniTask.NextFrame(m_Cts.Token).Chain(m_ShowFunctions.GetRange(m_StateIndex, m_ShowFunctions.length - m_StateIndex));
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });

            return task;
        }

        public UniTask Hide(CancellationToken ct = default)
        {
            Stop();
            m_Cts = ct != default ? CancellationTokenSource.CreateLinkedTokenSource(ct) : new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                var functions = m_HideFunctions.GetRange(0, m_StateIndex + 1);
                functions.Reverse();

                try
                {
                    await UniTask.NextFrame(m_Cts.Token).Chain(functions);
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
            // Show();
        }

        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.A))
            // {
            //     Show(default(CancellationToken));
            // }
            // else if (Input.GetKeyDown(KeyCode.D))
            // {
            //     Hide(default(CancellationToken));
            // }
            // else if (Input.GetKeyDown(KeyCode.Q))
            // {
            //     ShowImmediate();
            // }
            // else if (Input.GetKeyDown(KeyCode.E))
            // {
            //     HideImmediate();
            // }
        }

        void OnDestroy()
        {
            Stop();
        }
    }
}
