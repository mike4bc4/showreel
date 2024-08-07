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
    public const string VideoPlayerName = "VideoPlayer";
    public const string ListElementSnapshotLayerName = "ListElementSnapshotLayerName";

    [SerializeField] VisualTreeAsset m_ListBoard;
    [SerializeField] VideoClip m_VideoClip;
    [SerializeField] int m_ListElementCount;

    Layer m_ListBoardLayer;
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
        Application.targetFrameRate = 120;

        m_Player.AddEvent(0, () =>
        {
            m_ListBoardLayer = LayerManager.CreateLayer(m_ListBoard, "ListBoardLayer");
            m_ListBoardLayer.alpha = 0f;
            m_ListBoardLayer.blocksRaycasts = true;
            m_ListBoardLayer.interactable = false;

            m_FrameViewport = m_ListBoardLayer.rootVisualElement.Q("frame-viewport");
            m_TitleViewport = m_ListBoardLayer.rootVisualElement.Q("title-viewport");

            m_ListViewport = m_ListBoardLayer.rootVisualElement.Q("list-viewport");
            m_ListViewport.visible = false;

            m_FrameSpacer = m_FrameViewport.Q("spacer-left");
            m_FrameSpacer.style.flexGrow = 0.5f;

            m_Frame = m_FrameViewport.Q<DiamondFrameVertical>();
            m_Frame.animationProgress = 0f;
            m_Frame.contentContainer.visible = false;
            m_VideoElement = m_Frame.contentContainer.Q("video");

            m_Title = m_TitleViewport.Q<DiamondTitle>();
            m_Title.animationProgress = 0f;
            m_Title.label.visible = false;

            m_ScrollBox = m_ListViewport.Query<ScrollBox>();
            m_ScrollBox.visible = false;

            m_ListElements = m_ListViewport.Query<ListElement>().ToList();
            foreach (var element in m_ListElements)
            {
                element.bullet.animationProgress = 0f;
                element.bullet.visible = false;
                element.button.visible = false;
            }
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(0, () =>
        {
            m_ListBoardLayer.MaskElements(m_Title, m_Frame);
            m_ListBoardLayer.alpha = 1f;
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame, FrameSnapshotLayerName);
            layer.alpha = 0f;
            layer.blur = Layer.DefaultBlur;
        }, EventInvokeFlags.Forward, 1);

        m_Player.AddEvent(0, () =>
        {
            LayerManager.RemoveLayer(m_ListBoardLayer);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(0, () =>
        {
            m_ListBoardLayer.UnmaskElements();
            m_ListBoardLayer.alpha = 0f;
            LayerManager.RemoveLayer(FrameSnapshotLayerName);
        }, EventInvokeFlags.Backward);

        var t1 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(FrameSnapshotLayerName)?.SetAlpha(alpha));
        t1.AddKeyframe(0, 0f);
        t1.AddKeyframe(20, 1f);

        var t2 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(FrameSnapshotLayerName)?.SetBlur(blur));
        t2.AddKeyframe(10, Layer.DefaultBlur);
        t2.AddKeyframe(30, 0f);

        m_Player.AddEvent(30, () =>
        {
            LayerManager.RemoveLayer(FrameSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_Frame);
        }, EventInvokeFlags.Forward);

        m_Player.AddEvent(30, () =>
        {
            m_ListBoardLayer.CreateSnapshotLayer(m_Frame, FrameSnapshotLayerName);
        }, EventInvokeFlags.Backward);
        m_Player.AddEvent(30, () =>
        {
            m_ListBoardLayer.MaskElements(m_Frame);
        }, EventInvokeFlags.Backward, 1);

        var t3 = m_Player.AddKeyframeTrack((float progress) => m_Frame?.SetAnimationProgress(progress));
        t3.AddKeyframe(30, 0f);
        t3.AddKeyframe(120, 1f);

        m_Player.AddEvent(120, () =>
        {
            m_ListBoardLayer.MaskElements(m_Frame.contentContainer);
            m_Frame.contentContainer.visible = true;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(120, () =>
        {
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Frame.contentContainer, FrameContentContainerSnapshotLayerName);
            layer.alpha = 0f;
            layer.blur = Layer.DefaultBlur;
        }, EventInvokeFlags.Forward, 1);
        m_Player.AddEvent(120, () =>
        {
            var videoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip, VideoPlayerName);
            videoPlayer.Play();
            var layer = (Layer)LayerManager.GetLayer(FrameContentContainerSnapshotLayerName);
            var snapshot = layer.rootVisualElement.Q("snapshot");
            snapshot.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
        }, EventInvokeFlags.Forward, 2);

        m_Player.AddEvent(120, () =>
        {
            m_ListBoardLayer.UnmaskElements(m_Frame.contentContainer);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(120, () =>
        {
            VideoPlayerManager.RemovePlayer(VideoPlayerName);
            LayerManager.RemoveLayer(FrameContentContainerSnapshotLayerName);
            m_Frame.contentContainer.visible = false;
        }, EventInvokeFlags.Backward);

        var t4 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(FrameContentContainerSnapshotLayerName)?.SetAlpha(alpha));
        t4.AddKeyframe(120, 0f);
        t4.AddKeyframe(140, 1f);

        var t5 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(FrameContentContainerSnapshotLayerName)?.SetBlur(blur));
        t5.AddKeyframe(130, Layer.DefaultBlur);
        t5.AddKeyframe(150, 0f);

        m_Player.AddEvent(150, () =>
        {
            var videoPlayer = VideoPlayerManager.GetPlayer(VideoPlayerName);
            m_VideoElement.style.backgroundImage = Background.FromRenderTexture(videoPlayer.targetTexture);
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(150, () =>
        {
            LayerManager.RemoveLayer(FrameContentContainerSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_Frame.contentContainer);
        }, EventInvokeFlags.Forward, 1);

        m_Player.AddEvent(150, () =>
        {
            m_ListBoardLayer.MaskElements(m_Frame.contentContainer);
            m_VideoElement.style.backgroundImage = null;
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(150, () =>
        {
            m_ListBoardLayer.CreateSnapshotLayer(m_Frame.contentContainer, FrameContentContainerSnapshotLayerName);
        }, EventInvokeFlags.Backward);

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
            m_ScrollBox.visible = true;
            m_ListBoardLayer.MaskElements(m_ScrollBox);
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(195, () =>
        {
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_ScrollBox, ScrollBoxSnapshotLayerName);
            layer.alpha = 0f;
            layer.blur = Layer.DefaultBlur;
        }, EventInvokeFlags.Forward, 1);

        m_Player.AddEvent(195, () =>
        {
            LayerManager.RemoveLayer(ScrollBoxSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_ScrollBox);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(195, () =>
        {
            m_ScrollBox.visible = false;
        }, EventInvokeFlags.Backward);

        var t7 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(ScrollBoxSnapshotLayerName)?.SetAlpha(alpha));
        t7.AddKeyframe(195, 0f);
        t7.AddKeyframe(215, 1f);

        var t8 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(ScrollBoxSnapshotLayerName)?.SetBlur(blur));
        t8.AddKeyframe(205, Layer.DefaultBlur);
        t8.AddKeyframe(225, 0f);

        m_Player.AddEvent(225, () =>
        {
            LayerManager.RemoveLayer(ScrollBoxSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_ScrollBox);
        }, EventInvokeFlags.Forward);

        m_Player.AddEvent(225, () =>
        {
            m_ListBoardLayer.MaskElements(m_ScrollBox);
        }, EventInvokeFlags.Backward, 2);
        m_Player.AddEvent(225, () =>
        {
            m_ListBoardLayer.CreateSnapshotLayer(m_ScrollBox, ScrollBoxSnapshotLayerName);
        }, EventInvokeFlags.Backward, 1);

        var bulletAnimationStartFrameIndex = 225;
        var bulletAnimationDuration = 75;
        var bulletAnimationDelay = 20;
        var fadeDelay = 45;
        var fadeDuration = 30;
        for (int i = 0; i < m_ListElementCount; i++)
        {
            var idx = i;
            var startFrameIndex = bulletAnimationDelay * i + bulletAnimationStartFrameIndex;

            // Actions related to manipulation of list element snapshots could be batched into
            // one event, but this would be very inefficient because of multiple mask/unmask and
            // create/remove snapshot layer calls per frame, thus we are separating it here into 
            // multiple events.
            m_Player.AddEvent(startFrameIndex, () =>
            {
                m_ListElements[idx].button.visible = true;
                m_ListElements[idx].bullet.visible = true;
                m_ListBoardLayer.MaskElements(m_ListElements[idx].button);
            }, EventInvokeFlags.Forward);
            m_Player.AddEvent(startFrameIndex, () =>
            {
                var layer = m_ListBoardLayer.CreateSnapshotLayer(m_ListElements[idx].button, ListElementSnapshotLayerName + idx);
                layer.alpha = 0f;
                layer.blur = Layer.DefaultBlur;
            }, EventInvokeFlags.Forward, 1);

            m_Player.AddEvent(startFrameIndex, () =>
            {
                LayerManager.RemoveLayer(ListElementSnapshotLayerName + idx);
                m_ListBoardLayer.UnmaskElements(m_ListElements[idx].button);
            }, EventInvokeFlags.Backward, 1);
            m_Player.AddEvent(startFrameIndex, () =>
            {
                m_ListElements[idx].button.visible = false;
                m_ListElements[idx].bullet.visible = false;
            }, EventInvokeFlags.Backward);

            var track = m_Player.AddKeyframeTrack((float progress) => m_ListElements.ElementAtOrDefault(idx)?.bullet?.SetAnimationProgress(progress));
            track.AddKeyframe(startFrameIndex, 0f);
            track.AddKeyframe(startFrameIndex + bulletAnimationDuration, 1f);

            var track2 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(ListElementSnapshotLayerName + idx)?.SetAlpha(alpha));
            track2.AddKeyframe(startFrameIndex + fadeDelay, 0f);
            track2.AddKeyframe(startFrameIndex + fadeDelay + (int)(fadeDuration * 0.666f), 1f);

            var track3 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(ListElementSnapshotLayerName + idx)?.SetBlur(blur));
            track3.AddKeyframe(startFrameIndex + fadeDelay + (int)(fadeDuration * 0.333f), Layer.DefaultBlur);
            track3.AddKeyframe(startFrameIndex + fadeDelay + fadeDuration, 0f);
        }

        var i0 = bulletAnimationDelay * m_ListElementCount + bulletAnimationStartFrameIndex + fadeDelay + fadeDuration;
        m_Player.AddEvent(i0, () =>
        {
            m_ListBoardLayer.interactable = true;
            for (int i = 0; i < m_ListElements.Count; i++)
            {
                LayerManager.RemoveLayer(ListElementSnapshotLayerName + i);
                m_ListBoardLayer.UnmaskElements(m_ListElements[i].button);
            }
        }, EventInvokeFlags.Forward);

        m_Player.AddEvent(i0, () =>
        {
            for (int i = 0; i < m_ListElements.Count; i++)
            {
                m_ListBoardLayer.MaskElements(m_ListElements[i].button);
            }
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(i0, () =>
        {
            m_ListBoardLayer.interactable = false;
            for (int i = 0; i < m_ListElements.Count; i++)
            {
                m_ListBoardLayer.CreateSnapshotLayer(m_ListElements[i].button, ListElementSnapshotLayerName + i);
            }
        }, EventInvokeFlags.Backward);

        // Title label animation
        m_Player.AddEvent(i0, () =>
        {
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title, TitleSnapshotLayerName);
            layer.alpha = 0f;
            layer.blur = Layer.DefaultBlur;
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(i0, () =>
        {
            LayerManager.RemoveLayer(TitleSnapshotLayerName);
        }, EventInvokeFlags.Backward);

        var t9 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(TitleSnapshotLayerName)?.SetAlpha(alpha));
        t9.AddKeyframe(i0, 0f);
        t9.AddKeyframe(i0 + 20, 1f);

        var t10 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(TitleSnapshotLayerName)?.SetBlur(blur));
        t10.AddKeyframe(i0 + 10, Layer.DefaultBlur);
        t10.AddKeyframe(i0 + 30, 0f);

        m_Player.AddEvent(i0 + 30, () =>
        {
            LayerManager.RemoveLayer(TitleSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_Title);
        }, EventInvokeFlags.Forward);

        m_Player.AddEvent(i0 + 30, () =>
        {
            m_ListBoardLayer.MaskElements(m_Title);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(i0 + 30, () =>
        {
            m_ListBoardLayer.CreateSnapshotLayer(m_Title, TitleSnapshotLayerName);
        }, EventInvokeFlags.Backward);

        var t11 = m_Player.AddKeyframeTrack((float progress) => m_Title?.SetAnimationProgress(progress));
        t11.AddKeyframe(i0 + 30, 0f);
        t11.AddKeyframe(i0 + 90, 1f);

        m_Player.AddEvent(i0 + 90, () =>
        {
            m_Title.label.visible = true;
            m_ListBoardLayer.MaskElements(m_Title.label);
        }, EventInvokeFlags.Forward);
        m_Player.AddEvent(i0 + 90, () =>
        {
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title.label, TitleLabelSnapshotLayerName);
            layer.alpha = 0f;
            layer.blur = Layer.DefaultBlur;
        }, EventInvokeFlags.Forward, 1);

        m_Player.AddEvent(i0 + 90, () =>
        {
            LayerManager.RemoveLayer(TitleLabelSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_Title.label);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(i0 + 90, () =>
        {
            m_Title.label.visible = false;
        }, EventInvokeFlags.Backward);

        var t12 = m_Player.AddKeyframeTrack((float alpha) => LayerManager.GetLayer(TitleLabelSnapshotLayerName)?.SetAlpha(alpha));
        t12.AddKeyframe(i0 + 90, 0f);
        t12.AddKeyframe(i0 + 110, 1f);

        var t13 = m_Player.AddKeyframeTrack((float blur) => LayerManager.GetLayer(TitleLabelSnapshotLayerName)?.SetBlur(blur));
        t13.AddKeyframe(i0 + 100, Layer.DefaultBlur);
        t13.AddKeyframe(i0 + 120, 0f);

        m_Player.AddEvent(i0 + 120, () =>
        {
            LayerManager.RemoveLayer(TitleLabelSnapshotLayerName);
            m_ListBoardLayer.UnmaskElements(m_Title.label);
        }, EventInvokeFlags.Forward);

        m_Player.AddEvent(i0 + 120, () =>
        {
            m_ListBoardLayer.MaskElements(m_Title.label);
        }, EventInvokeFlags.Backward, 1);
        m_Player.AddEvent(i0 + 120, () =>
        {
            var layer = m_ListBoardLayer.CreateSnapshotLayer(m_Title.label, TitleLabelSnapshotLayerName);
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
