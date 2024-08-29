using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using Boards;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Layers;
using Controls.Raw;
using Utility;

namespace Boards
{
    public class ListBoard : Board
    {
        public const int DisplaySortOrder = 0;

        const int k_PostProcessingLayersCount = 2;
        const string k_ShowAnimationName = "ShowAnimation";
        const string k_HideAnimationName = "HideAnimation";
        const string k_TitleAnimationName = "TitleAnimation";
        const string k_ListElementsShowAnimationName = "ListElementsAnimation";
        const string k_ScrollBoxShowAnimationName = "ScrollBoxAnimation";
        const string k_VideoSwapAnimationName = "VideoSwapAnimation";
        const int k_BulletAnimationInterval = 20;
        const int k_BulletAnimationDuration = 80;
        const int k_ButtonShowDelay = 60;

        [SerializeField] VisualTreeAsset m_ListBoardVisualTreeAsset;
        [SerializeField] List<VideoClip> m_VideoClips;

        Layer m_Layer;
        List<PostProcessingLayer> m_PostProcessingLayers;

        AnimationPlayer m_AnimationPlayer;
        AnimationPlayer m_TitleAnimationPlayer;
        AnimationPlayer m_VideoSwapAnimationPlayer;

        VisualElement m_FrameViewport;
        DiamondFrameVertical m_Frame;
        VisualElement m_VideoContainer;
        List<VisualElement> m_VideoContainers;
        VisualElement m_FrameSpacerLeft;
        ScrollBox m_ScrollBox;
        List<ListElement> m_ListElements;
        DiamondTitle m_Title;

        List<MoviePlayer> m_MoviePlayers;
        VideoClip m_QueuedVideoClip;

        public bool isVisible
        {
            get => m_AnimationPlayer.animation == m_AnimationPlayer[k_ListElementsShowAnimationName] && m_AnimationPlayer.animationTime == m_AnimationPlayer.duration;
        }

        public override void Init()
        {
            m_Layer = LayerManager.CreateLayer("ListBoard");
            m_Layer.AddTemplateFromVisualTreeAsset(m_ListBoardVisualTreeAsset);
            m_Layer.displaySortOrder = DisplaySortOrder;

            m_PostProcessingLayers = new List<PostProcessingLayer>();
            for (int i = 0; i < k_PostProcessingLayersCount; i++)
            {
                var postProcessingLayer = LayerManager.CreatePostProcessingLayer("ListBoard");
                m_PostProcessingLayers.Add(postProcessingLayer);
                postProcessingLayer.displaySortOrder = DisplaySortOrder + 1;
            }

            m_MoviePlayers = new List<MoviePlayer>() { new MoviePlayer(), null };

            m_FrameViewport = m_Layer.rootVisualElement.Q("frame-viewport");
            m_Frame = m_Layer.rootVisualElement.Q<DiamondFrameVertical>("frame");
            m_VideoContainer = m_FrameViewport.Q("video-container");
            m_VideoContainers = new List<VisualElement>() { m_FrameViewport.Q("video-container-1"), m_FrameViewport.Q("video-container-2") };
            m_FrameSpacerLeft = m_FrameViewport.Q("spacer-left");
            m_ScrollBox = m_Layer.rootVisualElement.Q<ScrollBox>("scroll-box");
            m_ListElements = m_Layer.rootVisualElement.Query<ListElement>().ToList();
            m_Title = m_Layer.rootVisualElement.Q<DiamondTitle>("title");

            m_AnimationPlayer = new AnimationPlayer();
            m_AnimationPlayer.AddAnimation(CreateShowAnimation(), k_ShowAnimationName);
            m_AnimationPlayer.AddAnimation(CreateHideAnimation(), k_HideAnimationName);
            m_AnimationPlayer.AddAnimation(CreateListElementsShowAnimation(), k_ListElementsShowAnimationName);
            m_AnimationPlayer.AddAnimation(CreateScrollBoxShowAnimation(), k_ScrollBoxShowAnimationName);
            m_AnimationPlayer.animation = m_AnimationPlayer[k_ShowAnimationName];

            m_TitleAnimationPlayer = new AnimationPlayer();
            m_TitleAnimationPlayer.AddAnimation(CreateTitleAnimation(), k_TitleAnimationName);
            m_TitleAnimationPlayer.animation = m_TitleAnimationPlayer[k_TitleAnimationName];

            m_VideoSwapAnimationPlayer = new AnimationPlayer();
            m_VideoSwapAnimationPlayer.AddAnimation(CreateVideoSwapAnimation(), k_VideoSwapAnimationName);
            m_VideoSwapAnimationPlayer.animation = m_VideoSwapAnimationPlayer[k_VideoSwapAnimationName];

            HideImmediate();
        }

        public override void Show(Action onCompleted = null)
        {
            base.Show(onCompleted);
            m_AnimationPlayer.animation = m_AnimationPlayer[k_ShowAnimationName];
            m_AnimationPlayer.playbackSpeed = 1f;
            m_AnimationPlayer.Play();
        }

        public override void ShowImmediate()
        {
            m_AnimationPlayer.Stop();
            m_AnimationPlayer.animation = m_AnimationPlayer[k_ListElementsShowAnimationName];
            m_AnimationPlayer.FastForward();

            m_TitleAnimationPlayer.Stop();

            m_Frame.style.opacity = 1f;
            m_Frame.animationProgress = 1f;
            m_Frame.contentContainer.style.opacity = 1f;

            m_FrameSpacerLeft.style.flexGrow = 1f;

            m_MoviePlayers[0].PlayClip(m_VideoClips[0]);
            m_VideoContainers[0].style.backgroundImage = Background.FromRenderTexture(m_MoviePlayers[0].renderTexture);

            m_Layer.visible = true;
            m_Layer.interactable = true;
            m_Layer.blocksRaycasts = true;
            m_Layer.alpha = 1f;
            m_Layer.blurSize = 0f;

            SetPostProcessingLayersVisible(false);

            m_Title.style.opacity = 1f;
            m_Title.label.style.opacity = 1f;
            m_Title.animationProgress = 1f;

            m_ScrollBox.style.opacity = 1f;

            foreach (var listElement in m_ListElements)
            {
                listElement.bullet.visible = true;
                listElement.bullet.animationProgress = 1f;
                listElement.button.style.opacity = 1f;
            }
        }

        public override void Hide(Action onCompleted = null)
        {
            base.Hide(onCompleted);
            m_AnimationPlayer.animation = m_AnimationPlayer[k_HideAnimationName];
            m_AnimationPlayer.playbackSpeed = 1f;
            m_AnimationPlayer.Play();
        }

        public override void HideImmediate()
        {
            m_AnimationPlayer.Stop();
            m_AnimationPlayer.animation = m_AnimationPlayer[k_HideAnimationName];
            m_TitleAnimationPlayer.Stop();

            m_Layer.visible = false;
            m_Layer.interactable = false;
            m_Layer.blocksRaycasts = false;

            SetPostProcessingLayersVisible(false);

            m_Title.style.opacity = 0f;
            m_ScrollBox.style.opacity = 0f;
            foreach (var listElement in m_ListElements)
            {
                listElement.bullet.visible = false;
                listElement.button.style.opacity = 0f;
            }
        }

        public void SwapVideo(VideoClip videoClip)
        {
            if (m_VideoSwapAnimationPlayer.isPlaying)
            {
                m_QueuedVideoClip = videoClip;
                return;
            }

            SwapVideoInternal(videoClip);
        }

        void SwapVideoInternal(VideoClip videoClip)
        {
            if (m_MoviePlayers[0].videoClip == videoClip)
            {
                return;
            }

            m_MoviePlayers[1] = new MoviePlayer();
            m_MoviePlayers[1].PlayClip(videoClip);
            m_VideoContainers[1].style.backgroundImage = Background.FromRenderTexture(m_MoviePlayers[1].renderTexture);

            m_VideoSwapAnimationPlayer.Rewind();
            m_VideoSwapAnimationPlayer.Play();
        }

        void SetPostProcessingLayersVisible(bool visible)
        {
            foreach (var postProcessingLayer in m_PostProcessingLayers)
            {
                postProcessingLayer.visible = visible;
            }
        }

        void SetPostProcessingLayersOverscan(Overscan overscan)
        {
            foreach (var postProcessingLayer in m_PostProcessingLayers)
            {
                postProcessingLayer.overscan = overscan;
            }
        }

        KeyframeAnimation CreateVideoSwapAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = true;
                    m_PostProcessingLayers[0].overscan = 8f;
                    m_PostProcessingLayers[0].maskElement = m_VideoContainer;
                }
            });

            var t1 = animation.AddTrack(blurSize => m_PostProcessingLayers[0].blurSize = blurSize);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(30, PostProcessingLayer.DefaultBlurSize);
            t1.AddKeyframe(60, 0f);
            // t1.AddKeyframe(60, 0f);

            // var t2 = animation.AddTrack(opacity => m_PrimaryVideoContainer.style.opacity = opacity);
            // t2.AddKeyframe(0, 1f);
            // t2.AddKeyframe(60, 0f);

            var t2 = animation.AddTrack(opacity => m_VideoContainers[1].style.opacity = opacity);
            t2.AddKeyframe(10, 0f);
            t2.AddKeyframe(50, 1f);

            animation.AddEvent(60, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = false;

                    m_VideoContainers[0].style.backgroundImage = m_VideoContainers[1].resolvedStyle.backgroundImage;
                    m_VideoContainers[1].style.backgroundImage = null;

                    m_VideoContainers[0].style.opacity = 1f;
                    m_VideoContainers[1].style.opacity = 0f;

                    m_MoviePlayers[0].Dispose();
                    m_MoviePlayers[0] = m_MoviePlayers[1];
                    m_MoviePlayers[1] = null;

                    if (m_QueuedVideoClip != null)
                    {
                        SwapVideoInternal(m_QueuedVideoClip);
                        m_QueuedVideoClip = null;
                    }
                }
            });

            return animation;
        }

        KeyframeAnimation CreateShowAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.visible = true;
                    m_Layer.alpha = 1f;
                    m_Layer.blurSize = 0f;

                    m_ScrollBox.style.opacity = 0f;
                    foreach (var listElement in m_ListElements)
                    {
                        listElement.bullet.visible = false;
                        listElement.button.style.opacity = 0f;
                    }

                    m_Title.style.opacity = 0f;
                    m_Title.animationProgress = 0f;
                    m_Title.label.style.opacity = 0f;

                    m_PostProcessingLayers[0].visible = true;
                    m_PostProcessingLayers[0].overscan = 8f;
                    m_PostProcessingLayers[0].maskElement = m_Frame;
                }
            });

            var t1 = animation.AddTrack(opacity => m_Frame.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack1 = animation.AddTrack(blurSize => m_PostProcessingLayers[0].blurSize = blurSize);
            blurTrack1.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
            blurTrack1.AddKeyframe(30, 0f, Easing.StepOut);

            var t2 = animation.AddTrack(animationProgress => m_Frame.animationProgress = animationProgress);
            t2.AddKeyframe(30, 0f);
            t2.AddKeyframe(120, 1f);

            animation.AddEvent(120, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_MoviePlayers[0].PlayClip(m_VideoClips[0]);
                    m_VideoContainers[0].style.backgroundImage = Background.FromRenderTexture(m_MoviePlayers[0].renderTexture);
                    m_PostProcessingLayers[0].maskElement = m_Frame.contentContainer;
                }
            });

            var t3 = animation.AddTrack(opacity => m_Frame.contentContainer.style.opacity = opacity);
            t3.AddKeyframe(120, 0f);
            t3.AddKeyframe(140, 1f);

            blurTrack1.AddKeyframe(120, PostProcessingLayer.DefaultBlurSize);
            blurTrack1.AddKeyframe(130, PostProcessingLayer.DefaultBlurSize);
            blurTrack1.AddKeyframe(150, 0f, Easing.StepOut);

            var t4 = animation.AddTrack(flexGrow => m_FrameSpacerLeft.style.flexGrow = flexGrow);
            t4.AddKeyframe(150, 0.5f);
            t4.AddKeyframe(210, 1f);

            animation.AddEvent(210, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = false;
                    if (m_ScrollBox.isScrollBarDisplayed)
                    {
                        m_AnimationPlayer.animation = m_AnimationPlayer[k_ScrollBoxShowAnimationName];
                        m_AnimationPlayer.playbackSpeed = 1f;
                        m_AnimationPlayer.Play();
                    }
                    else
                    {
                        m_AnimationPlayer.animation = m_AnimationPlayer[k_ListElementsShowAnimationName];
                        m_AnimationPlayer.playbackSpeed = 1f;
                        m_AnimationPlayer.Play();
                    }
                }
            });

            return animation;
        }

        KeyframeAnimation CreateScrollBoxShowAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = true;
                    m_PostProcessingLayers[0].overscan = 8f;
                    m_PostProcessingLayers[0].maskElement = m_ScrollBox;
                }
            });

            var t1 = animation.AddTrack(opacity => m_ScrollBox.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack = animation.AddTrack(blurSize => m_PostProcessingLayers[0].blurSize = blurSize);
            blurTrack.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(30, 0, Easing.StepOut);

            animation.AddEvent(30, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = false;
                    m_AnimationPlayer.animation = m_AnimationPlayer[k_ListElementsShowAnimationName];
                    m_AnimationPlayer.playbackSpeed = 1f;
                    m_AnimationPlayer.Play();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateListElementsShowAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_ScrollBox.style.opacity = 1f;
                    SetPostProcessingLayersVisible(true);
                    SetPostProcessingLayersOverscan(8f);
                }
            });

            var blurTrack1 = animation.AddTrack(blurSize => m_PostProcessingLayers[0].blurSize = blurSize);
            blurTrack1.AddKeyframe(0, 0f, Easing.StepOut);

            var blurTrack2 = animation.AddTrack(blurSize => m_PostProcessingLayers[1].blurSize = blurSize);
            blurTrack2.AddKeyframe(0, 0f, Easing.StepOut);

            for (int i = 0; i < m_ListElements.Count; i++)
            {
                int idx = i;    // Cache idx, because i is loop variable.
                var listElement = m_ListElements[i];
                int bulletAnimationStartFrame = k_BulletAnimationInterval * i;
                animation.AddEvent(bulletAnimationStartFrame, () =>
                {
                    if (animation.player.isPlayingForward)
                    {
                        listElement.bullet.visible = true;
                    }
                });

                var track1 = animation.AddTrack(animationProgress => listElement.bullet.animationProgress = animationProgress);
                track1.AddKeyframe(bulletAnimationStartFrame, 0f);
                track1.AddKeyframe(bulletAnimationStartFrame + k_BulletAnimationDuration, 1f);

                int buttonAnimationStartFrame = bulletAnimationStartFrame + k_ButtonShowDelay;
                animation.AddEvent(buttonAnimationStartFrame, () =>
                {
                    if (animation.player.isPlayingForward)
                    {
                        // We are swapping post processing layers, as otherwise their blur tracks
                        // would overlap. In other words there are two post processing layers responsible
                        // for handling list element's button blur and they are animated in parallel
                        // with separate animation tracks.
                        var postProcessingLayer = m_PostProcessingLayers[idx % 2];
                        postProcessingLayer.maskElement = listElement.button;
                    }
                });

                var track2 = animation.AddTrack(opacity => listElement.button.style.opacity = opacity);
                track2.AddKeyframe(buttonAnimationStartFrame, 0f);
                track2.AddKeyframe(buttonAnimationStartFrame + 20, 1f);

                var blurTrack = idx % 2 == 0 ? blurTrack1 : blurTrack2;
                blurTrack.AddKeyframe(buttonAnimationStartFrame, PostProcessingLayer.DefaultBlurSize);
                blurTrack.AddKeyframe(buttonAnimationStartFrame + 10, PostProcessingLayer.DefaultBlurSize);
                blurTrack.AddKeyframe(buttonAnimationStartFrame + 30, 0f, Easing.StepOut);
            }

            int finalEventFrameIndex = (m_ListElements.Count - 1) * k_BulletAnimationInterval + k_ButtonShowDelay + 30;
            animation.AddEvent(finalEventFrameIndex, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.interactable = true;
                    m_Layer.blocksRaycasts = true;
                    SetPostProcessingLayersVisible(false);
                    m_TitleAnimationPlayer.Play();
                    m_ShowCompletedCallback?.Invoke();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateHideAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.blocksRaycasts = false;
                    m_Layer.interactable = false;
                }
            });

            var t1 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, Layer.DefaultBlurSize);

            var t2 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
            t2.AddKeyframe(10, 1f);
            t2.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.visible = false;
                    SetPostProcessingLayersVisible(false);
                    m_HideCompletedCallback?.Invoke();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateTitleAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = true;
                    m_PostProcessingLayers[0].overscan = 8f;
                    m_PostProcessingLayers[0].maskElement = m_Title;
                }
            });

            var t1 = animation.AddTrack(opacity => m_Title.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack = animation.AddTrack(blurSize => m_PostProcessingLayers[0].blurSize = blurSize);
            blurTrack.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(30, 0f, Easing.StepOut);

            var t2 = animation.AddTrack(animationProgress => m_Title.animationProgress = animationProgress);
            t2.AddKeyframe(30, 0f);
            t2.AddKeyframe(90, 1f);

            animation.AddEvent(90, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].maskElement = m_Title.label;
                    m_PostProcessingLayers[0].overscan = new Overscan(8f, 8f, 0f, 8f);
                }
            });

            var t3 = animation.AddTrack(opacity => m_Title.label.style.opacity = opacity);
            t3.AddKeyframe(90, 0f);
            t3.AddKeyframe(110, 1f);

            blurTrack.AddKeyframe(90, PostProcessingLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(100, PostProcessingLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(120, 0f, Easing.StepOut);

            animation.AddEvent(120, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_PostProcessingLayers[0].visible = false;
                }
            });

            return animation;
        }

        void OnDestroy()
        {
            foreach (var moviePlayer in m_MoviePlayers)
            {
                if (moviePlayer != null)
                {
                    moviePlayer.Dispose();
                }
            }
        }
    }
}
