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
    public const string TitleSnapshotLayerName = "TitleSnapshotLayer";
    public const string TitleLabelSnapshotLayerName = "TitleLabelSnapshotLayer";
    public const string ScrollBoxSnapshotLayerName = "ScrollBoxSnapshotLayer";

    [SerializeField] VisualTreeAsset m_ListBoard;
    [SerializeField] float m_FadeTime;
    [SerializeField] float m_FlexTime;
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

    [SerializeField] float m_TestProperty;

    public float testProperty
    {
        get => m_TestProperty;
        set
        {
            if (value != m_TestProperty)
            {
                m_TestProperty = value;
                Debug.Log("Set");
            }
        }
    }

    

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
                    element.button.visible = false;
                    element.bullet.FoldImmediate();
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

        m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("A")
        {
            setter = alpha => LayerManager.GetLayer("FrameSnapshotLayer").alpha = alpha,
            from = 0f,
            to = 1f,
            duration = m_FadeTime,
        });

        m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("B")
        {
            setter = blur => ((Layer)LayerManager.GetLayer("FrameSnapshotLayer")).blurSize = blur,
            from = Layer.DefaultBlurSize,
            to = 0f,
            duration = m_FadeTime,
        })
        .DelayForward(() => m_Player[0]["A"].progress > 0.5f)
        .DelayBackward(() => m_Player[0]["C"].progress <= 0f);

        m_Player[0].AddKeyframe(new KeyframeDescriptor("C")
        {
            forward = new KeyframeAction((IKeyframe keyframe) =>
            {
                LayerManager.RemoveLayer("FrameSnapshotLayer");
                m_ListBoardLayer.UnmaskElements(m_Frame);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame, "FrameSnapshotLayer");
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken: cancellationToken);
                m_ListBoardLayer.MaskElements(m_Frame);
            }),
        })
        .DelayForward(() => m_Player[1]["B"].progress >= 1f);

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                await m_Frame.Unfold(cancellationToken);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                await m_Frame.Fold(cancellationToken);
            }),
        });

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_Frame.mainContainer.visible = true;
                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame.mainContainer, "MainContainerSnapshotLayer");
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
                m_ListBoardLayer.MaskElements(m_Frame.mainContainer);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_Frame.mainContainer.visible = false;
                LayerManager.RemoveLayer("MainContainerSnapshotLayer");
                m_ListBoardLayer.UnmaskElements(m_Frame.mainContainer);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            })
        });

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                var videoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip, "Player");
                videoPlayer.Play();
                var layer = (Layer)LayerManager.GetLayer("MainContainerSnapshotLayer");
                var snapshot = layer.rootVisualElement.Q("snapshot");
                snapshot.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                VideoPlayerManager.RemovePlayer("Player");
                var layer = (Layer)LayerManager.GetLayer("MainContainerSnapshotLayer");
                var snapshot = layer.rootVisualElement.Q("snapshot");
                snapshot.style.backgroundImage = StyleKeyword.Null;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            })
        });

        m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("D")
        {
            setter = alpha => LayerManager.GetLayer("MainContainerSnapshotLayer").alpha = alpha,
            from = 0f,
            to = 1f,
            duration = m_FadeTime,
        })
        .DelayBackward(() => m_Player[1]["E"].progress <= 0.5f);

        m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("E")
        {
            setter = blur => ((Layer)LayerManager.GetLayer("MainContainerSnapshotLayer")).blurSize = blur,
            from = Layer.DefaultBlurSize,
            to = 0f,
            duration = m_FadeTime,
        })
        .DelayForward(() => m_Player[0]["D"].progress >= 0.5f)
        .DelayBackward(() => m_Player[0]["F"].progress <= 0f);

        m_Player[0].AddKeyframe(new KeyframeDescriptor("F")
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                var videoPlayer = VideoPlayerManager.GetPlayer("Player");
                m_VideoElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                m_Frame.mainContainer.visible = true;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_VideoElement.style.backgroundImage = StyleKeyword.Null;
                m_Frame.mainContainer.visible = false;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            })
        })
        .DelayForward(() => m_Player[1]["E"].progress >= 1f);

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction((IKeyframe keyframe) =>
            {
                m_ListBoardLayer.UnmaskElements(m_Frame.mainContainer);
                LayerManager.RemoveLayer("MainContainerSnapshotLayer");
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                // Don't have to check whether layer already exist, as forward will be executed as a rollback method
                // if player is paused or stopped, and will remove created layer anyway.
                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame.mainContainer, "MainContainerSnapshotLayer");
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, m_TaskScheduler.token);

                var videoPlayer = VideoPlayerManager.GetPlayer("Player");
                var snapshot = layer.rootVisualElement.Q("snapshot");
                snapshot.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
                m_ListBoardLayer.MaskElements(m_Frame.mainContainer);
            })
        });

        m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>()
        {
            setter = flex => m_FrameSpacer.style.flexGrow = flex,
            from = 0.5f,
            to = 1f,
            duration = m_FlexTime,
            timingFunction = TimingFunction.EaseOutCubic,
        });

        // TODO: Fix labels.
        // TITLE ANIMATION
        // m_Player[0].AddKeyframe(new KeyframeDescriptor()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title, TitleSnapshotLayerName);
        //         layer.alpha = 0f;
        //         layer.blurSize = Layer.DefaultBlurSize;
        //         await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
        //     }),
        //     backward = new KeyframeAction((IKeyframe keyframe) =>
        //     {
        //         LayerManager.RemoveLayer(TitleSnapshotLayerName);
        //     })
        // });

        // m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("F")
        // {
        //     setter = alpha => ((Layer)LayerManager.GetLayer(TitleSnapshotLayerName)).alpha = alpha,
        //     from = 0f,
        //     to = 1f,
        //     duration = m_FadeTime,
        // })
        // .DelayBackward(() => m_Player[1]["G"].progress <= 0.5f);

        // m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("G")
        // {
        //     setter = blur => ((Layer)LayerManager.GetLayer(TitleSnapshotLayerName)).blurSize = blur,
        //     from = Layer.DefaultBlurSize,
        //     to = 0,
        //     duration = m_FadeTime,
        // })
        // .DelayForward(() => m_Player[0]["F"].progress >= 0.5f)
        // .DelayBackward(() => m_Player[0]["H"].progress <= 0f);

        // m_Player[0].AddKeyframe(new KeyframeDescriptor("H")
        // {
        //     forward = new KeyframeAction((IKeyframe keyframe) =>
        //     {
        //         LayerManager.RemoveLayer(TitleSnapshotLayerName);
        //         m_ListBoardLayer.UnmaskElements(m_Title);
        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title, TitleSnapshotLayerName);
        //         m_ListBoardLayer.MaskElements(m_Title);
        //         await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
        //     })
        // })
        // .DelayForward(() => m_Player[1]["G"].progress >= 1f);

        // m_Player[0].AddKeyframe(new KeyframeDescriptor()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         await m_Title.Unfold(cancellationToken);
        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         await m_Title.Fold(cancellationToken);
        //     })
        // });

        // m_Player[0].AddKeyframe(new KeyframeDescriptor()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         m_Title.label.visible = true;
        //         var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title.label, TitleLabelSnapshotLayerName);
        //         layer.alpha = 0f;
        //         layer.blurSize = Layer.DefaultBlurSize;
        //         m_ListBoardLayer.MaskElements(m_Title.label);
        //         await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         m_Title.label.visible = false;
        //         LayerManager.RemoveLayer(TitleLabelSnapshotLayerName);
        //         m_ListBoardLayer.UnmaskElements(m_Title.label);
        //         await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
        //     })
        // });

        // m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("I")
        // {
        //     setter = alpha => ((Layer)LayerManager.GetLayer(TitleLabelSnapshotLayerName)).alpha = alpha,
        //     from = 0f,
        //     to = 1f,
        //     duration = m_FadeTime,
        // })
        // .DelayBackward(() => m_Player[1]["J"].progress <= 0.5f);

        // m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("J")
        // {
        //     setter = blur => ((Layer)LayerManager.GetLayer(TitleLabelSnapshotLayerName)).blurSize = blur,
        //     from = Layer.DefaultBlurSize,
        //     to = 0f,
        //     duration = m_FadeTime,
        // })
        // .DelayForward(() => m_Player[0]["I"].progress >= 0.5f)
        // .DelayBackward(() => m_Player[0]["K"].progress <= 0f);

        // m_Player[0].AddKeyframe(new KeyframeDescriptor("K")
        // {
        //     forward = new KeyframeAction((IKeyframe keyframe) =>
        //     {
        //         LayerManager.RemoveLayer(TitleLabelSnapshotLayerName);
        //         m_ListBoardLayer.UnmaskElements(m_Title.label);
        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title.label, TitleLabelSnapshotLayerName);
        //         await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
        //         m_ListBoardLayer.MaskElements(m_Title.label);
        //     })
        // })
        // .DelayForward(() => m_Player[1]["J"].progress >= 1f);
        // TITLE ANIMATION END

        #region ScrollBox Animation

        m_Player[0].AddKeyframe(new KeyframeDescriptor()
        {
            forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_ScrollBox.visible = true;
                m_ListBoardLayer.MaskElements(m_ScrollBox);
                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_ScrollBox, ScrollBoxSnapshotLayerName);
                layer.alpha = 0f;
                layer.blurSize = Layer.DefaultBlurSize;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_ScrollBox.visible = false;
                m_ListBoardLayer.MaskElements(m_ScrollBox);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            })
        });

        m_Player[0].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("M")
        {
            setter = alpha => ((Layer)LayerManager.GetLayer(ScrollBoxSnapshotLayerName)).alpha = alpha,
            from = 0f,
            to = 1f,
            duration = m_FadeTime,
        })
        .DelayBackward(() => m_Player[0]["N"].progress <= 0.5f);

        m_Player[1].AddAnimationKeyframe(new AnimationKeyframeDescriptor<float>("N")
        {
            setter = blur => ((Layer)LayerManager.GetLayer(ScrollBoxSnapshotLayerName)).blurSize = blur,
            from = Layer.DefaultBlurSize,
            to = 0f,
            duration = m_FadeTime,
        })
        .DelayForward(() => m_Player[0]["M"].progress >= 0.5f)
        .DelayBackward(() => m_Player[0]["O"].progress <= 0f);

        m_Player[0].AddKeyframe(new KeyframeDescriptor("O")
        {
            forward = new KeyframeAction((IKeyframe keyframe) =>
            {
                LayerManager.RemoveLayer(ScrollBoxSnapshotLayerName);
                m_ListBoardLayer.UnmaskElements(m_ScrollBox);
            }),
            backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
            {
                m_ListBoardLayer.CreateSnapshotLayer(m_ScrollBox, ScrollBoxSnapshotLayerName);
                m_ListBoardLayer.MaskElements(m_ScrollBox);
                await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
            })
        })
        .DelayForward(() => m_Player[1]["N"].progress >= 1f);

        #endregion

        #region List Bullet Animation

        // m_Player[0].AddKeyframe(new KeyframeDescriptor()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         foreach (var listElement in m_ListElements)
        //         {
        //             listElement.visible = true;
        //         }
        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {
        //         foreach (var listElement in m_ListElements)
        //         {

        //         }
        //     })
        // });

        #endregion

        // m_Player[0].AddKeyframe(new KeyframeDescriptor()
        // {
        //     forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {

        //     }),
        //     backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
        //     {

        //     })
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
