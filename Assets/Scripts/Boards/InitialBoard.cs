using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
// using Utils;
using Layers;
using Controls.Raw;
using Controls;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateName = Guid.NewGuid().ToString("N");

        const int k_DisplaySortOrder = 1;
        const string k_TitleElementName = "title";
        const string k_SubtitleElementName = "subtitle";
        const string k_MainAnimationName = "MainAnimation";
        const string k_HideSmoothAnimationName = "HideSmooth";
        const string k_SubtitleAnimationName = "SubtitleAnimation";

        [SerializeField] VisualTreeAsset m_InitialBoardVisualTreeAsset;

        AnimationPlayer m_Player;
        AnimationPlayer m_SubtitleAnimationPlayer;

        Layer m_InitialBoardLayer;
        PostProcessingLayer m_PostProcessingLayer;
        DiamondTitle m_Title;
        Subtitle m_Subtitle;

        public void Init()
        {
            var state = BoardManager.StateMachine.AddState(StateName);
            BoardManager.ActionGroup.InitialBoard.Any.performed += OnAny;
            BoardManager.ActionGroup.InitialBoard.Cancel.performed += OnCancel;
            BoardManager.ActionGroup.InitialBoard.Confirm.performed += OnConfirm;

            m_Player = new AnimationPlayer();
            var anim = new KeyframeAnimation();
            m_Player.AddAnimation(anim, k_MainAnimationName);
            m_Player.AddAnimation(CreateHideSmoothAnimation(), k_HideSmoothAnimationName);
            m_Player.animation = anim;

            m_SubtitleAnimationPlayer = new AnimationPlayer();
            m_SubtitleAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;
            m_SubtitleAnimationPlayer.AddAnimation(CreateSubtitleAnimation(), k_SubtitleAnimationName);
            m_SubtitleAnimationPlayer.animation = m_SubtitleAnimationPlayer[k_SubtitleAnimationName];

            anim.AddEvent(0, () =>
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

            var t1 = anim.AddTrack((float opacity) =>
            {
                if (m_Title != null)
                {
                    m_Title.style.opacity = opacity;
                }
            });
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack1 = anim.AddTrack((float blurSize) =>
            {
                if (m_PostProcessingLayer != null)
                {
                    m_PostProcessingLayer.blurSize = blurSize;
                }
            });
            blurTrack1.AddKeyframe(10, Layer.DefaultBlurSize);
            blurTrack1.AddKeyframe(30, 0f, Easing.StepOut);

            anim.AddEvent(30, () =>
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

            var t3 = anim.AddTrack((float animationProgress) =>
            {
                if (m_Title != null)
                {
                    m_Title.animationProgress = animationProgress;
                }
            });
            t3.AddKeyframe(30, 0f);
            t3.AddKeyframe(90, 1f);

            anim.AddEvent(90, () =>
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

            var t4 = anim.AddTrack((float opacity) =>
            {
                if (m_Title?.label != null)
                {
                    m_Title.label.style.opacity = opacity;
                }
            });
            t4.AddKeyframe(90, 0f);
            t4.AddKeyframe(110, 1f);

            blurTrack1.AddKeyframe(90, Layer.DefaultBlurSize);
            blurTrack1.AddKeyframe(100, Layer.DefaultBlurSize);
            blurTrack1.AddKeyframe(120, 0f, Easing.StepOut);

            anim.AddEvent(120, () =>
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

            var t6 = anim.AddTrack((float opacity) =>
            {
                if (m_Subtitle != null)
                {
                    m_Subtitle.style.opacity = opacity;
                }
            });
            t6.AddKeyframe(120, 0f);
            t6.AddKeyframe(140, 1f);

            blurTrack1.AddKeyframe(121, Layer.DefaultBlurSize);
            blurTrack1.AddKeyframe(130, Layer.DefaultBlurSize);
            blurTrack1.AddKeyframe(150, 0f, Easing.StepOut);

            anim.AddEvent(150, () =>
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
        }

        void Clear()
        {
            LayerManager.RemoveLayer(m_InitialBoardLayer);
            LayerManager.RemoveLayer(m_PostProcessingLayer);

            m_Title = null;
            m_Subtitle = null;
        }

        KeyframeAnimation CreateSubtitleAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack((float animationProgress) =>
            {
                if (m_Subtitle != null)
                {
                    m_Subtitle.animationProgress = animationProgress;
                }
            });
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(120, 1f);
            t1.AddKeyframe(180, 1f);    // Add one second of idle time.

            return animation;
        }

        KeyframeAnimation CreateHideSmoothAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack((float blurSize) =>
            {
                if (m_InitialBoardLayer != null)
                {
                    m_InitialBoardLayer.blurSize = blurSize;
                }
            });
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, Layer.DefaultBlurSize);

            var t2 = animation.AddTrack((float alpha) =>
            {
                if (m_InitialBoardLayer != null)
                {
                    m_InitialBoardLayer.alpha = alpha;
                }
            });
            t2.AddKeyframe(10, 1f);
            t2.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                Clear();
                m_SubtitleAnimationPlayer.Stop();
            });

            return animation;
        }

        public void ShowImmediate()
        {
            m_Player.Stop();

            LayerManager.RemoveLayer(m_PostProcessingLayer);
            if (m_InitialBoardLayer == null)
            {
                m_InitialBoardLayer = LayerManager.CreateLayer(m_InitialBoardVisualTreeAsset, displaySortOrder: k_DisplaySortOrder);
            }

            m_InitialBoardLayer.blurSize = 0f;
            m_InitialBoardLayer.alpha = 1f;

            m_Title = m_InitialBoardLayer.rootVisualElement.Q<DiamondTitle>(k_TitleElementName);
            m_Title.animationProgress = 1f;
            m_Title.style.opacity = 1f;
            m_Title.label.style.opacity = 1f;

            m_Subtitle = m_InitialBoardLayer.rootVisualElement.Q<Subtitle>(k_SubtitleElementName);
            m_Subtitle.style.opacity = 1f;

            m_SubtitleAnimationPlayer.animationTime = 0f;
            m_SubtitleAnimationPlayer.playbackSpeed = 1f;
            m_SubtitleAnimationPlayer.Play();
        }

        public void Show()
        {
            m_Player.animation = m_Player[k_MainAnimationName];
            m_Player.playbackSpeed = 1f;
            m_Player.Play();
        }

        public void Hide()
        {
            m_Player.animation = m_Player[k_MainAnimationName];
            m_Player.playbackSpeed = -1;
            m_Player.Play();
        }

        public void HideSmooth()
        {
            m_Player.animation = m_Player[k_HideSmoothAnimationName];
            m_Player.playbackSpeed = 1f;
            m_Player.Play();
        }

        public void OnAny(InputAction.CallbackContext ctx)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (m_Player.animation == m_Player[k_MainAnimationName] && m_Player.status.IsPlaying())
            {
                ShowImmediate();
            }
            else
            {
                Debug.Log("Continue");
            }
        }

        public void OnCancel(InputAction.CallbackContext ctx)
        {
            // TODO: Display quit dialog board.
            // Dialog box created via new?
            // Debug.Log("OnCancel");
            
        }

        public void OnConfirm(InputAction.CallbackContext ctx)
        {
            Debug.Log("OnConfirm");
        }


        void OnDestroy()
        {
            dialogBoxWrapper?.Dispose();
        }

        DialogBox dialogBoxWrapper;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                dialogBoxWrapper ??= new DialogBox();
                dialogBoxWrapper.Show();

                // UniTask.Create(async () =>
                // {
                //     await UniTask.WaitForSeconds(2f);
                //     dialogBox.Dispose();
                // });
            }
            // else if (Input.GetKeyDown(KeyCode.D))
            // {
            //     Hide();
            // }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                dialogBoxWrapper.Hide();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                dialogBoxWrapper.HideImmediate();
            }
        }
    }
}
