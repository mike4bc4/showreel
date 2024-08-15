using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public const string FrameSnapshotLayerName = "FrameSnapshotLayer";
    public const string FrameContentContainerSnapshotLayerName = "FrameContentContainerSnapshotLayer";
    public const string TitleSnapshotLayerName = "TitleSnapshotLayer";
    public const string TitleLabelSnapshotLayerName = "TitleLabelSnapshotLayer";
    public const string ScrollBoxSnapshotLayerName = "ScrollBoxSnapshotLayer";
    public const string ListElementSnapshotLayerName = "ListElementSnapshotLayerName";

    const int k_DisplaySortOrder = 0;
    const string k_VideoPlayerName = "VideoPlayer";

    [SerializeField] VisualTreeAsset m_ListBoard;
    [SerializeField] VideoClip m_VideoClip;
    [SerializeField] int m_ListElementCount;

    Layer m_ListBoardLayer;
    PostProcessingLayer m_PostProcessingLayer;
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
        m_ListElements = new List<ListElement>();
        m_Player = new KeyframeTrackPlayer();
        m_Player.sampling = 60;

        m_Player.AddEvent(0, () =>
        {
            m_ListBoardLayer = LayerManager.CreateLayer(m_ListBoard, displaySortOrder: k_DisplaySortOrder);
            m_ListBoardLayer.blocksRaycasts = true;
            m_ListBoardLayer.interactable = false;

            m_FrameViewport = m_ListBoardLayer.rootVisualElement.Q("frame-viewport");
            m_TitleViewport = m_ListBoardLayer.rootVisualElement.Q("title-viewport");
            m_ListViewport = m_ListBoardLayer.rootVisualElement.Q("list-viewport");

            m_Frame = m_FrameViewport.Q<DiamondFrameVertical>();
            m_Frame.animationProgress = 0f;
            m_Frame.style.opacity = 0f;
            m_Frame.contentContainer.style.opacity = 0f;
            m_VideoElement = m_Frame.contentContainer.Q("video");

            m_FrameSpacer = m_FrameViewport.Q("spacer-left");
            m_FrameSpacer.style.flexGrow = 0.5f;

            m_Title = m_TitleViewport.Q<DiamondTitle>();
            m_Title.animationProgress = 0f;
            m_Title.style.opacity = 0f;
            m_Title.label.style.opacity = 0f;

            m_ScrollBox = m_ListViewport.Query<ScrollBox>();
            m_ScrollBox.style.opacity = 0f;

            m_ListElements = m_ListViewport.Query<ListElement>().ToList();
            foreach (var element in m_ListElements)
            {
                element.bullet.animationProgress = 0f;
                element.bullet.visible = false;
                element.button.style.opacity = 0f;
            }

            m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: k_DisplaySortOrder + 1);
            m_PostProcessingLayer.maskElement = m_Frame;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(0, () =>
        {
            LayerManager.RemoveLayer(m_ListBoardLayer);
            LayerManager.RemoveLayer(m_PostProcessingLayer);
        }, EventInvokeFlags.Backward);

        var t1 = m_Player.AddKeyframeTrack((float opacity) =>
        {
            if (m_Frame != null)
            {
                m_Frame.style.opacity = opacity;
            }
        });
        t1.AddKeyframe(0, 0f);
        t1.AddKeyframe(20, 1f);

        var t2 = m_Player.AddKeyframeTrack((float blurSize) =>
        {
            if (m_PostProcessingLayer != null)
            {
                m_PostProcessingLayer.blurSize = blurSize;
            }
        });
        t2.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
        t2.AddKeyframe(30, 0f);

        m_Player.AddEvent(30, () =>
        {
            m_PostProcessingLayer.maskElement = null;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(30, () =>
        {
            m_PostProcessingLayer.maskElement = m_Frame;
        }, EventInvokeFlags.Backward);

        var t3 = m_Player.AddKeyframeTrack((float animationProgress) =>
        {
            if (m_Frame != null)
            {
                m_Frame.animationProgress = animationProgress;
            }
        });
        t3.AddKeyframe(30, 0f);
        t3.AddKeyframe(120, 1f);

        m_Player.AddEvent(120, () =>
        {
            var videoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip, k_VideoPlayerName);
            videoPlayer.Play();
            m_VideoElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);

            m_PostProcessingLayer.maskElement = m_VideoElement;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(120, () =>
        {
            VideoPlayerManager.RemovePlayer(k_VideoPlayerName);
            m_VideoElement.style.backgroundImage = null;

            m_PostProcessingLayer.maskElement = null;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = 0f;
        }, EventInvokeFlags.Backward);

        var t4 = m_Player.AddKeyframeTrack((float opacity) =>
        {
            if (m_Frame != null)
            {
                m_Frame.contentContainer.style.opacity = opacity;
            }
        });
        t4.AddKeyframe(120, 0f);
        t4.AddKeyframe(140, 1f);

        var t5 = m_Player.AddKeyframeTrack((float blurSize) =>
        {
            if (m_PostProcessingLayer != null)
            {
                m_PostProcessingLayer.blurSize = blurSize;
            }
        });
        t5.AddKeyframe(130, PostProcessingLayer.DefaultBlurSize);
        t5.AddKeyframe(150, 0f);

        var t6 = m_Player.AddKeyframeTrack((float flexGrow) =>
        {
            if (m_FrameSpacer != null)
            {
                m_FrameSpacer.style.flexGrow = flexGrow;
            }
        });
        t6.AddKeyframe(150, 0.5f);
        t6.AddKeyframe(195, 1f);

        m_Player.AddEvent(195, () =>
        {
            m_PostProcessingLayer.maskElement = m_ScrollBox;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(195, () =>
        {
            m_PostProcessingLayer.maskElement = m_VideoElement;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = 0f;
        }, EventInvokeFlags.Backward);

        var t7 = m_Player.AddKeyframeTrack((float opacity) =>
        {
            if (m_ScrollBox != null)
            {
                m_ScrollBox.style.opacity = opacity;
            }
        });
        t7.AddKeyframe(195, 0f);
        t7.AddKeyframe(215, 1f);

        var t8 = m_Player.AddKeyframeTrack((float blurSize) =>
        {
            if (m_PostProcessingLayer != null)
            {
                m_PostProcessingLayer.blurSize = blurSize;
            }
        });
        t8.AddKeyframe(205, PostProcessingLayer.DefaultBlurSize);
        t8.AddKeyframe(225, 0f);

        int startFrameIndex = 225;
        int bulletAnimationDelay = 20;
        int bulletAnimationDuration = 75;
        int fadeDelay = 45;
        for (int i = 0; i < m_ListElementCount; i++)
        {
            int index = i;
            int bulletAnimationStartFrameIndex = startFrameIndex + i * bulletAnimationDelay;
            m_Player.AddEvent(bulletAnimationStartFrameIndex, () =>
            {
                m_ListElements[index].bullet.visible = true;
            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(bulletAnimationStartFrameIndex, () =>
            {
                m_ListElements[index].bullet.visible = false;
            }, EventInvokeFlags.Backward);

            var tt1 = m_Player.AddKeyframeTrack((float animationProgress) =>
            {
                if (m_ListElements.ElementAtOrDefault(index) != null)
                {
                    m_ListElements[index].bullet.animationProgress = animationProgress;
                }
            });
            tt1.AddKeyframe(bulletAnimationStartFrameIndex, 0f);
            tt1.AddKeyframe(bulletAnimationStartFrameIndex + bulletAnimationDuration, 1);

            int buttonAnimationStartFrameIndex = bulletAnimationStartFrameIndex + fadeDelay;
            m_Player.AddEvent(buttonAnimationStartFrameIndex, () =>
            {
                m_PostProcessingLayer.maskElement = m_ListElements[index].button;
                m_PostProcessingLayer.overscan = 8f;
                m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(buttonAnimationStartFrameIndex, () =>
            {
                m_PostProcessingLayer.maskElement = index > 0 ? m_ListElements[index - 1].button : m_ScrollBox;
                m_PostProcessingLayer.overscan = 8f;
                m_PostProcessingLayer.blurSize = 0f;
            }, EventInvokeFlags.Backward);

            var tt2 = m_Player.AddKeyframeTrack((float opacity) =>
            {
                if (m_ListElements.ElementAtOrDefault(index) != null)
                {
                    m_ListElements[index].button.style.opacity = opacity;
                }
            });
            tt2.AddKeyframe(buttonAnimationStartFrameIndex, 0f);
            tt2.AddKeyframe(buttonAnimationStartFrameIndex + 20, 1);

            var tt3 = m_Player.AddKeyframeTrack((float blurSize) =>
            {
                if (m_PostProcessingLayer != null)
                {
                    m_PostProcessingLayer.blurSize = blurSize;
                }
            });
            tt3.AddKeyframe(buttonAnimationStartFrameIndex + 10, PostProcessingLayer.DefaultBlurSize);
            tt3.AddKeyframe(buttonAnimationStartFrameIndex + 30, 0f);
        }

        int titleAnimationStartFrameIndex = startFrameIndex + (m_ListElementCount - 1) * bulletAnimationDelay + fadeDelay + 30;
        m_Player.AddEvent(titleAnimationStartFrameIndex, () =>
        {
            m_ListBoardLayer.interactable = true;
            m_PostProcessingLayer.maskElement = m_Title;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(titleAnimationStartFrameIndex, () =>
        {
            m_ListBoardLayer.interactable = false;
            m_PostProcessingLayer.maskElement = m_ListElements[m_ListElementCount - 1].button;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = 0f;
        }, EventInvokeFlags.Backward);

        var t9 = m_Player.AddKeyframeTrack((float opacity) =>
        {
            if (m_Title != null)
            {
                m_Title.style.opacity = opacity;
            }
        });
        t9.AddKeyframe(titleAnimationStartFrameIndex, 0f);
        t9.AddKeyframe(titleAnimationStartFrameIndex + 20, 1f);

        var t10 = m_Player.AddKeyframeTrack((float blurSize) =>
        {
            if (m_PostProcessingLayer != null)
            {
                m_PostProcessingLayer.blurSize = blurSize;
            }
        });
        t10.AddKeyframe(titleAnimationStartFrameIndex + 10, PostProcessingLayer.DefaultBlurSize);
        t10.AddKeyframe(titleAnimationStartFrameIndex + 30, 0f);

        m_Player.AddEvent(titleAnimationStartFrameIndex + 30, () =>
        {
            m_PostProcessingLayer.maskElement = null;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(titleAnimationStartFrameIndex + 30, () =>
        {
            m_PostProcessingLayer.maskElement = m_Title;
        }, EventInvokeFlags.Backward);

        var t11 = m_Player.AddKeyframeTrack((float animationProgress) =>
        {
            if (m_Title != null)
            {
                m_Title.animationProgress = animationProgress;
            }
        });
        t11.AddKeyframe(titleAnimationStartFrameIndex + 30, 0f);
        t11.AddKeyframe(titleAnimationStartFrameIndex + 90, 1f);

        m_Player.AddEvent(titleAnimationStartFrameIndex + 90, () =>
        {
            m_PostProcessingLayer.maskElement = m_Title.label;
            m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
            m_PostProcessingLayer.blurSize = PostProcessingLayer.DefaultBlurSize;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(titleAnimationStartFrameIndex + 90, () =>
        {
            m_PostProcessingLayer.maskElement = null;
            m_PostProcessingLayer.overscan = 8f;
            m_PostProcessingLayer.blurSize = 0f;
        }, EventInvokeFlags.Backward);

        var t12 = m_Player.AddKeyframeTrack((float opacity) =>
        {
            if (m_Title != null)
            {
                m_Title.label.style.opacity = opacity;
            }
        });
        t12.AddKeyframe(titleAnimationStartFrameIndex + 90, 0f);
        t12.AddKeyframe(titleAnimationStartFrameIndex + 110, 1f);

        var t13 = m_Player.AddKeyframeTrack((float blurSize) =>
        {
            if (m_PostProcessingLayer != null)
            {
                m_PostProcessingLayer.blurSize = blurSize;
            }
        });
        t13.AddKeyframe(titleAnimationStartFrameIndex + 100, PostProcessingLayer.DefaultBlurSize);
        t13.AddKeyframe(titleAnimationStartFrameIndex + 120, 0f);

        m_Player.AddEvent(titleAnimationStartFrameIndex + 120, () =>
        {
            LayerManager.RemoveLayer(m_PostProcessingLayer);
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(titleAnimationStartFrameIndex + 120, () =>
        {
            m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: k_DisplaySortOrder + 1);
            m_PostProcessingLayer.maskElement = m_Title.label;
            m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
        }, EventInvokeFlags.Backward);
    }

    public void ShowImmediate()
    {

    }

    public void HideImmediate()
    {

    }

    public UniTask Show(CancellationToken cancellationToken = default)
    {
        return UniTask.CompletedTask;
    }

    public UniTask Hide(CancellationToken cancellationToken = default)
    {
        return UniTask.CompletedTask;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_Player.playbackSpeed = 1f;
            m_Player.Play();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            m_Player.playbackSpeed = -1f;
            m_Player.Play();
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
