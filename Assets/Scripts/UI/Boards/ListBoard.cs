using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using UI;
using UI.Boards;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Utils;

public class ListBoard : Board, IBoard
{
    [SerializeField] VisualTreeAsset m_ListBoard;
    [SerializeField] float m_FadeTime;
    [SerializeField] VideoClip m_VideoClip;

    Layer m_ListBoardLayer;
    TaskScheduler m_TaskScheduler;
    TaskChain m_TaskChain;

    VisualElement m_FrameViewport;
    VisualElement m_TitleViewport;
    VisualElement m_ListViewport;

    VisualElement m_FrameSpacer;
    DiamondFrameVertical m_Frame;
    VisualElement m_VideoElement;
    DiamondTitle m_Title;
    List<ListElement> m_ListElements;

    public void Init()
    {
        m_TaskScheduler = new TaskScheduler();
        m_TaskChain = new TaskChain();

        m_TaskChain.AddLink(

            to: async () =>
            {
                m_ListBoardLayer = LayerManager.CreateLayer(m_ListBoard, "ListBoardLayer");
                m_ListBoardLayer.alpha = 0f;

                m_FrameViewport = m_ListBoardLayer.rootVisualElement.Q("frame-viewport");
                m_TitleViewport = m_ListBoardLayer.rootVisualElement.Q("title-viewport");

                m_ListViewport = m_ListBoardLayer.rootVisualElement.Q("list-viewport");
                m_ListViewport.visible = false;

                m_FrameSpacer = m_FrameViewport.Q("spacer-left");
                m_FrameSpacer.style.flexGrow = 0.5f;

                m_Frame = m_FrameViewport.Q<DiamondFrameVertical>();
                m_Frame.FoldImmediate();
                m_Frame.mainContainer.visible = false;
                m_VideoElement = m_Frame.mainContainer.Q("video");

                m_Title = m_TitleViewport.Q<DiamondTitle>();
                m_Title.FoldImmediate();
                m_Title.label.visible = false;

                m_ListElements = m_ListViewport.Query<ListElement>().ToList();
                foreach (var element in m_ListElements)
                {
                    element.visible = false;
                }

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                var t2 = UniTask.WaitUntil(() => m_Frame.ready && m_Title.ready, cancellationToken: m_TaskScheduler.token);

                try
                {
                    await (t1, t2);
                }
                catch (OperationCanceledException)
                {
                    LayerManager.RemoveLayer(m_ListBoardLayer);
                    throw;
                }
            },
            from: () =>
            {
                LayerManager.RemoveLayer(m_ListBoardLayer);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                m_ListBoardLayer.MaskElements(m_Title, m_Frame);
                m_ListBoardLayer.alpha = 1f;

                try
                {
                    var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Frame, "FrameSnapshotLayer", m_TaskScheduler.token);
                    layer.alpha = 0f;
                    layer.blurSize = Layer.DefaultBlurSize;
                }
                catch (OperationCanceledException)
                {
                    m_ListBoardLayer.Unmask();
                    m_ListBoardLayer.alpha = 0f;
                    throw;
                }
            },
            from: () =>
            {
                m_ListBoardLayer.Unmask();
                m_ListBoardLayer.alpha = 0f;
                LayerManager.RemoveLayer("FrameSnapshotLayer");
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("FrameSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("FrameSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("FrameSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("FrameSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: () =>
            {
                LayerManager.RemoveLayer("FrameSnapshotLayer");
                m_ListBoardLayer.UnmaskElements(m_Frame);
            },
            from: async () =>
            {
                await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Frame, "FrameSnapshotLayer", m_TaskScheduler.token);
                // Debug.Break();
                m_ListBoardLayer.MaskElements(m_Frame);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                await m_Frame.Unfold(m_TaskScheduler.token);
            },
            from: async () =>
            {
                await m_Frame.Fold(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                m_Frame.mainContainer.visible = true;

                // Using dummy task returning int here because typeless tasks cannot be awaited in parallel.
                var t1 = UniTask.Create(async () =>
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    return 0;
                });
                var t2 = m_ListBoardLayer.CreateSnapshotLayerAsync(m_Frame.mainContainer, "MainContainerSnapshotLayer", m_TaskScheduler.token);

                try
                {
                    var (_, layer) = await (t1, t2);
                    layer.alpha = 0f;
                    layer.blurSize = Layer.DefaultBlurSize;
                    m_ListBoardLayer.MaskElements(m_Frame.mainContainer);
                }
                catch (OperationCanceledException)
                {
                    m_Frame.mainContainer.visible = false;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            },
            from: async () =>
            {
                m_Frame.mainContainer.visible = false;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    LayerManager.RemoveLayer("MainContainerSnapshotLayer");
                    m_ListBoardLayer.UnmaskElements(m_Frame.mainContainer);
                }
                catch (OperationCanceledException)
                {
                    m_Frame.mainContainer.visible = true;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var videoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip, "VideoClipPlayer");
                videoPlayer.Play();

                var layer = (Layer)LayerManager.GetLayer("MainContainerSnapshotLayer");
                var snapshotElement = layer.rootVisualElement.Q("snapshot");
                snapshotElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    VideoPlayerManager.RemovePlayer(videoPlayer);

                    // Clear background image instead of restoring it, as we are not interested in snapshot content anyway.
                    snapshotElement.style.backgroundImage = null;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            },
            from: async () =>
            {
                var videoPlayer = VideoPlayerManager.GetPlayer("VideoClipPlayer");
                var videoFrame = videoPlayer.frame;
                VideoPlayerManager.RemovePlayer(videoPlayer);

                var layer = (Layer)LayerManager.GetLayer("MainContainerSnapshotLayer");
                var snapshotElement = layer.rootVisualElement.Q("snapshot");
                snapshotElement.style.backgroundImage = null;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    videoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip, "VideoClipPlayer");
                    videoPlayer.frame = videoFrame;
                    videoPlayer.Play();

                    snapshotElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("MainContainerSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("MainContainerSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("MainContainerSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("MainContainerSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var videoPlayer = VideoPlayerManager.GetPlayer("VideoClipPlayer");
                // Debug.Log(videoPlayer);
                m_VideoElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                m_Frame.mainContainer.visible = true;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    m_VideoElement.style.backgroundImage = null;
                    m_Frame.mainContainer.visible = false;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            },
            from: async () =>
            {
                m_VideoElement.style.backgroundImage = null;
                m_Frame.mainContainer.visible = false;

                try
                {
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken: m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    var videoPlayer = VideoPlayerManager.GetPlayer("VideoClipPlayer");
                    m_VideoElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                    m_Frame.mainContainer.visible = true;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            }
        );

        m_TaskChain.AddLink(
            to: () =>
            {
                m_ListBoardLayer.UnmaskElements(m_Frame.mainContainer);
                LayerManager.RemoveLayer("MainContainerSnapshotLayer");
            },
            from: async () =>
            {
                try
                {
                    var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Frame.mainContainer, "MainContainerSnapshotLayer", m_TaskScheduler.token);
                    var snapshot = layer.rootVisualElement.Q("snapshot");
                    snapshot.style.backgroundImage = Background.FromRenderTexture(VideoPlayerManager.GetPlayer("VideoClipPlayer").targetTexture);
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);
                    m_ListBoardLayer.MaskElements(m_Frame.mainContainer);
                }
                catch (OperationCanceledException)
                {
                    LayerManager.RemoveLayer("MainContainerSnapshotLayer");
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            }
        );

        // m_TaskChain.AddLink(new TaskChainLink()
        // {
        //     to = new LinkTask(async () =>
        //     {

        //     }),
        //     from = new LinkTask(async () =>
        //     {

        //     }),
        // });
    }

    public void ShowImmediate()
    {
        m_TaskScheduler.Schedule(() =>
        {

        });
    }

    public void HideImmediate()
    {
        m_TaskScheduler.Schedule(() =>
        {

        });
    }

    public UniTask Show(CancellationToken cancellationToken = default)
    {
        return m_TaskScheduler.Schedule(async () =>
        {
            await m_TaskChain.GoForward();
        }, cancellationToken);
    }

    public UniTask Hide(CancellationToken cancellationToken = default)
    {
        return m_TaskScheduler.Schedule(async () =>
        {
            await m_TaskChain.GoBackward();
        }, cancellationToken);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Show();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Hide();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowImmediate();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            HideImmediate();
        }
    }
}
