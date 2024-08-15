using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomControls;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        const int k_DisplaySortOrder = 1;
        const string k_TitleElementName = "title";
        const string k_SubtitleElementName = "subtitle";

        [SerializeField] VisualTreeAsset m_InitialBoardVisualTreeAsset;

        KeyframeTrackPlayer m_Player;
        KeyframeTrackPlayer m_SubtitleAnimationPlayer;

        Layer m_InitialBoardLayer;
        PostProcessingLayer m_PostProcessingLayer;
        DiamondTitle m_Title;
        Subtitle m_Subtitle;

        public void Init()
        {
            m_Player = new KeyframeTrackPlayer();
            m_Player.sampling = 60;

            m_SubtitleAnimationPlayer = new KeyframeTrackPlayer();
            m_SubtitleAnimationPlayer.sampling = 60;
            m_SubtitleAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;

            m_Player.AddEvent(0, () =>
            {
                if (m_Player.playbackSpeed > 0)
                {
                    m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVisualTreeAsset, displaySortOrder: k_DisplaySortOrder);

                    m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>(k_TitleElementName);
                    m_Title.style.opacity = 0f;
                    m_Title.label.style.opacity = 0f;
                    m_Title.animationProgress = 0f;

                    m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>(k_SubtitleElementName);
                    m_Subtitle.style.opacity = 0f;
                    m_Subtitle.animationProgress = 0f;

                    m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: k_DisplaySortOrder + 1);
                    m_PostProcessingLayer.overscan = 8f;
                    m_PostProcessingLayer.maskElement = m_Title;
                    m_PostProcessingLayer.blurSize = BaseLayer.DefaultBlurSize;
                }
                else
                {
                    LayerManager.RemoveLayer(m_InitialBoardLayer);
                    LayerManager.RemoveLayer(m_PostProcessingLayer);
                }
            });

            var t1 = m_Player.AddKeyframeTrack((float opacity) =>
            {
                if (m_Title != null)
                {
                    m_Title.style.opacity = opacity;
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
            t2.AddKeyframe(10, Layer.DefaultBlurSize);
            t2.AddKeyframe(30, 0f);

            m_Player.AddEvent(30, () =>
            {
                if (m_Player.playbackSpeed > 0)
                {
                    m_PostProcessingLayer.maskElement = null;
                }
                else
                {
                    m_PostProcessingLayer.maskElement = m_Title;
                    m_PostProcessingLayer.overscan = 8f;
                }
            });

            var t3 = m_Player.AddKeyframeTrack((float animationProgress) =>
            {
                if (m_Title != null)
                {
                    m_Title.animationProgress = animationProgress;
                }
            });
            t3.AddKeyframe(30, 0f);
            t3.AddKeyframe(90, 1f);

            m_Player.AddEvent(90, () =>
            {
                if (m_Player.playbackSpeed > 0)
                {
                    m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
                    m_PostProcessingLayer.maskElement = m_Title.label;
                    m_PostProcessingLayer.blurSize = BaseLayer.DefaultBlurSize;
                }
                else
                {
                    m_PostProcessingLayer.maskElement = null;
                    m_PostProcessingLayer.blurSize = 0f;
                }
            });

            var t4 = m_Player.AddKeyframeTrack((float opacity) =>
            {
                if (m_Title?.label != null)
                {
                    m_Title.label.style.opacity = opacity;
                }
            });
            t4.AddKeyframe(90, 0f);
            t4.AddKeyframe(110, 1f);

            var t5 = m_Player.AddKeyframeTrack((float blurSize) =>
            {
                if (m_PostProcessingLayer != null)
                {
                    m_PostProcessingLayer.blurSize = blurSize;
                }
            });
            t5.AddKeyframe(100, Layer.DefaultBlurSize);
            t5.AddKeyframe(120, 0f);

            m_Player.AddEvent(120, () =>
            {
                if (m_Player.playbackSpeed > 0)
                {
                    m_PostProcessingLayer.overscan = 8f;
                    m_PostProcessingLayer.maskElement = m_Subtitle;
                    m_PostProcessingLayer.blurSize = BaseLayer.DefaultBlurSize;
                }
                else
                {
                    m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
                    m_PostProcessingLayer.maskElement = m_Title.label;
                    m_PostProcessingLayer.blurSize = 0f;
                }
            });

            var t6 = m_Player.AddKeyframeTrack((float opacity) =>
            {
                if (m_Subtitle != null)
                {
                    m_Subtitle.style.opacity = opacity;
                }
            });
            t6.AddKeyframe(120, 0f);
            t6.AddKeyframe(140, 1f);

            var t7 = m_Player.AddKeyframeTrack((float blurSize) =>
            {
                if (m_PostProcessingLayer != null)
                {
                    m_PostProcessingLayer.blurSize = blurSize;
                }
            });
            t7.AddKeyframe(130, Layer.DefaultBlurSize);
            t7.AddKeyframe(150, 0f);

            m_Player.AddEvent(150, () =>
            {
                if (m_Player.playbackSpeed > 0)
                {
                    LayerManager.RemoveLayer(m_PostProcessingLayer);
                    m_SubtitleAnimationPlayer.Play();
                }
                else
                {
                    m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: k_DisplaySortOrder + 1);
                    m_PostProcessingLayer.overscan = 8f;
                    m_PostProcessingLayer.maskElement = m_Subtitle;
                    m_SubtitleAnimationPlayer.Stop();
                }
            });

            var t8 = m_SubtitleAnimationPlayer.AddKeyframeTrack((float progress) => m_Subtitle?.SetAnimationProgress(progress));
            t8.AddKeyframe(0, 0f);
            t8.AddKeyframe(120, 1f);
            t8.AddKeyframe(180, 1f);
        }

        public void ShowImmediate()
        {
            m_Player.Stop();
            m_Player.time = m_Player.duration;

            if (m_InitialBoardLayer == null)
            {
                m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVisualTreeAsset, displaySortOrder: k_DisplaySortOrder);
            }

            if (m_PostProcessingLayer != null)
            {
                LayerManager.RemoveLayer(m_PostProcessingLayer);
            }

            m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>(k_TitleElementName);
            m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>(k_SubtitleElementName);
            m_Player.Update();

            if (m_SubtitleAnimationPlayer.isPlaying)
            {
                m_SubtitleAnimationPlayer.Stop();
            }

            m_SubtitleAnimationPlayer.Play();
        }

        public void HideImmediate()
        {
            m_Player.Stop();
            m_Player.time = 0;

            if (m_InitialBoardLayer != null)
            {
                LayerManager.RemoveLayer(m_InitialBoardLayer);
            }

            if (m_PostProcessingLayer != null)
            {
                LayerManager.RemoveLayer(m_PostProcessingLayer);
            }

            if (m_SubtitleAnimationPlayer.isPlaying)
            {
                m_SubtitleAnimationPlayer.Stop();
            }
        }

        public void Show()
        {
            m_Player.playbackSpeed = 1f;
            m_Player.Play();
        }

        public void Hide()
        {
            m_Player.playbackSpeed = -1;
            m_Player.Play();
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
}
