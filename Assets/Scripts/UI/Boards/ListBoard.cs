using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
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
    KeyframeTrackPlayer m_Player;

    VisualElement m_FrameViewport;
    VisualElement m_TitleViewport;
    VisualElement m_ListViewport;

    VisualElement m_FrameSpacer;
    DiamondFrameVertical m_Frame;
    VisualElement m_VideoElement;
    DiamondTitle m_Title;
    ScrollBox m_ScrollBox;
    List<ListElement> m_ListElements;

    public void Init()
    {
        m_TaskScheduler = new TaskScheduler();
        m_TaskChain = new TaskChain();
        m_Player = new KeyframeTrackPlayer();
        m_Player.AddTrack();
        m_Player.AddTrack();

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
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

                m_ScrollBox = m_ListViewport.Query<ScrollBox>();
                m_ScrollBox.visible = false;

                m_ListElements = m_ListViewport.Query<ListElement>().ToList();
                foreach (var element in m_ListElements)
                {
                    element.visible = false;
                }

                var t1 = UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
                var t2 = UniTask.WaitUntil(() => m_Frame.ready && m_Title.ready, cancellationToken: cancellationToken);
                await (t1, t2);
            }),
            backward = new KeyframeAction((IKeyframe keyframe) =>
            {
                LayerManager.RemoveLayer(m_ListBoardLayer);
            }),
        });

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_ListBoardLayer.MaskElements(m_Title, m_Frame);
                m_ListBoardLayer.alpha = 1f;

                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame, "FrameSnapshotLayer");
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);

            }),
            backward = new KeyframeAction((IKeyframe keyframe) =>
            {
                m_ListBoardLayer.UnmaskElements();
                m_ListBoardLayer.alpha = 0f;
                LayerManager.RemoveLayer("FrameSnapshotLayer");
            })
        });

        m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>()
        {
            setter = alpha => LayerManager.GetLayer("FrameSnapshotLayer").alpha = alpha,
            from = 0f,
            to = 1f,
            duration = m_FadeTime,
            name = "FslAlpha",
        });

        m_Player[0].AddWaitUntilKeyframe(new WaitUntilKeyframeDescriptor()
        {
            forwardPredicate = () =>
            {
                var keyframe = m_Player[1]["FslBlur"];
                return m_Player[1].keyframeIndex >= keyframe.index && !keyframe.isPlaying;
            },
            backwardPredicate = () =>
            {
                var keyframe = (IAnimationKeyframe)m_Player[1]["FslBlur"];
                return keyframe.isPlaying && keyframe.progress < 0.5f;
            }
        });

        m_Player[1].AddWaitUntilKeyframe(new WaitUntilKeyframeDescriptor()
        {
            forwardPredicate = () =>
            {
                var keyframe = (IAnimationKeyframe)m_Player[0]["FslAlpha"];
                return keyframe.isPlaying && keyframe.progress > 0.5f;
            },
            backwardPredicate = () =>
            {
                return m_Player[0].keyframeIndex == 0 && !m_Player[0][0].isPlaying;
            }
        });

        m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>()
        {
            setter = blur => ((Layer)LayerManager.GetLayer("FrameSnapshotLayer")).blurSize = blur,
            from = Layer.DefaultBlurSize,
            to = 0f,
            duration = m_FadeTime,
            name = "FslBlur",
        });



        // UniTask.Create(async () =>
        // {
        //     while (true)
        //     {
        //         Debug.Log(strip2.GetKeyframe(1).status);
        //         await UniTask.NextFrame();
        //     }
        // }).Forget();

        // strip1.AddKeyframe(new KeyframeFactory()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) => { }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) => { })
        // });

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

                m_ScrollBox = m_ListViewport.Query<ScrollBox>();
                m_ScrollBox.visible = false;

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
                    m_ListBoardLayer.UnmaskElements();
                    m_ListBoardLayer.alpha = 0f;
                    throw;
                }
            },
            from: () =>
            {
                m_ListBoardLayer.UnmaskElements();
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

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Title, "TitleSnapshotLayer", m_TaskScheduler.token);
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
            },
            from: () =>
            {
                LayerManager.RemoveLayer("TitleSnapshotLayer");
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = (Layer)LayerManager.GetLayer("TitleSnapshotLayer");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: () =>
            {
                m_ListBoardLayer.UnmaskElements(m_Title);
                LayerManager.RemoveLayer("TitleSnapshotLayer");
            },
            from: async () =>
            {
                var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Title, "TitleSnapshotLayer", m_TaskScheduler.token);
                m_ListBoardLayer.MaskElements(m_Title);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                await m_Title.Unfold(m_TaskScheduler.token);
            },
            from: async () =>
            {
                await m_Title.Fold(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                await m_Title.label.style.SetPropertyAsync(nameof(IStyle.visibility), new StyleEnum<Visibility>(Visibility.Visible), m_TaskScheduler.token);
                m_ListBoardLayer.MaskElements(m_Title.label);

            },
            from: async () =>
            {
                await m_Title.label.style.SetPropertyAsync(nameof(IStyle.visibility), new StyleEnum<Visibility>(Visibility.Hidden), m_TaskScheduler.token);
                m_ListBoardLayer.UnmaskElements(m_Title.label);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Title.label, "TitleLabelSnapshot", m_TaskScheduler.token);
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
            },
            from: () =>
            {
                LayerManager.RemoveLayer("TitleLabelSnapshot");
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshot");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaOne);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshot");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.AlphaZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshot");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurZero);
                animation.time = m_FadeTime;
                await animation.AsTask(m_TaskScheduler.token);
            },
            from: async () =>
            {
                var layer = LayerManager.GetLayer("TitleLabelSnapshot");
                var animation = AnimationManager.Animate(layer, AnimationDescriptor.BlurDefault);
                animation.time = m_FadeTime;
                animation.SetTaskCancellationToken(m_TaskScheduler.token);
                await UniTask.WaitForSeconds(animation.time / 2f, cancellationToken: m_TaskScheduler.token);
            }
        );

        m_TaskChain.AddLink(
            to: () =>
            {
                LayerManager.RemoveLayer("TitleLabelSnapshot");
                m_ListBoardLayer.UnmaskElements(m_Title.label);
            },
            from: async () =>
            {
                var layer = await m_ListBoardLayer.CreateSnapshotLayerAsync(m_Title.label, "TitleLabelSnapshot", m_TaskScheduler.token);
                m_ListBoardLayer.MaskElements(m_Title.label);
            }
        );

        m_TaskChain.AddLink(
            to: async () =>
            {
                m_FrameSpacer.style.AddTransition("flex-grow", 1f);
                m_FrameSpacer.style.flexGrow = 1f;
                try
                {
                    await UniTask.WaitUntil(() => m_FrameSpacer.resolvedStyle.flexGrow == 1f, cancellationToken: m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    m_FrameSpacer.style.RemoveTransition("flex-grow");
                    m_FrameSpacer.style.flexGrow = m_FrameSpacer.resolvedStyle.flexGrow;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            },
            from: async () =>
            {
                m_FrameSpacer.style.AddTransition("flex-grow", 1f);
                m_FrameSpacer.style.flexGrow = 0.5f;
                try
                {
                    await UniTask.WaitUntil(() => m_FrameSpacer.resolvedStyle.flexGrow == 0.5f, cancellationToken: m_TaskScheduler.token);
                }
                catch (OperationCanceledException)
                {
                    m_FrameSpacer.style.RemoveTransition("flex-grow");
                    m_FrameSpacer.style.flexGrow = m_FrameSpacer.resolvedStyle.flexGrow;
                    await UniTask.NextFrame(PlayerLoopTiming.Initialization);
                    throw;
                }
            }
        );

        m_TaskChain.AddLink(
            to: () =>
            {

            },
            from: () =>
            {

            }
        );

        // m_TaskChain.AddLink(
        //     to: () =>
        //     {

        //     },
        //     from: () =>
        //     {

        //     }
        // );
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
        return m_TaskScheduler.Schedule(async (CancellationToken ct) =>
        {
            // await m_TaskChain.GoForward();
            await m_Player.Play(ct);
        }, cancellationToken);
    }

    public UniTask Hide(CancellationToken cancellationToken = default)
    {
        return m_TaskScheduler.Schedule(async (CancellationToken ct) =>
        {
            // await m_TaskChain.GoBackward();
            await m_Player.PlayBackwards(ct);
        }, cancellationToken);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Show();
            // UniTask.Create(async () =>
            // {
            //     await UniTask.WaitForSeconds(0.1f);
            //     // Debug.Log(Time.time);
            //     // await UniTask.CompletedTask;
            //     // Debug.Log(Time.time);
            //     var test = m_ListBoardLayer.rootVisualElement.Q("test");

            //     var keyframeStripGroup = new KeyframeStripGroup();
            //     var keyframeStrip = keyframeStripGroup.CreateStrip();
            //     keyframeStrip.AddAnimationKeyframe(
            //         color => test.style.backgroundColor = color,
            //         Color.red,
            //         Color.blue,
            //         2f
            //     );

            //     Debug.Log(Time.time);
            //     await keyframeStripGroup.Play();
            //     Debug.Log(Time.time);
            //     await keyframeStripGroup.PlayBackwards();
            //     Debug.Log(Time.time);
            // });
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
