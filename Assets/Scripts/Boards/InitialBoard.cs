using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Layers;
using Controls.Raw;
using Controls;
using FSM;

namespace Boards
{
    public class InitialBoard : Board
    {
        public const int DisplaySortOrder = 0;
        const string k_ShowAnimationName = "ShowAnimation";
        const string k_HideAnimationName = "HideAnimation";
        const string k_SubtitleAnimationName = "SubtitleAnimation";

        [SerializeField] VisualTreeAsset m_InitialBoardVisualTreeAsset;

        public Action onHideFinished;

        AnimationPlayer m_AnimationPlayer;
        AnimationPlayer m_SubtitleAnimationPlayer;

        Layer m_Layer;
        PostProcessingLayer m_PostProcessingLayer;
        DiamondTitle m_Title;
        Subtitle m_Subtitle;
        DialogBox m_QuitDialogBox;
        InputActionMap m_ActionMap;

        public override void Init()
        {
            m_ActionMap = InputSystem.actions.FindActionMap("InitialBoard");
            m_ActionMap.FindAction("Any").performed += OnAny;
            m_ActionMap.FindAction("Confirm").performed += OnConfirm;
            m_ActionMap.FindAction("Cancel").performed += OnCancel;

            m_Layer = LayerManager.CreateLayer("Initial");
            m_Layer.displaySortOrder = DisplaySortOrder;
            m_Layer.AddTemplateFromVisualTreeAsset(m_InitialBoardVisualTreeAsset);
            m_Layer.blocksRaycasts = false;
            m_Layer.interactable = false;

            m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer("Initial");
            m_PostProcessingLayer.displaySortOrder = DisplaySortOrder + 1;

            m_Title = m_Layer.rootVisualElement.Q<DiamondTitle>("title");
            m_Subtitle = m_Layer.rootVisualElement.Q<Subtitle>("subtitle");

            m_AnimationPlayer = new AnimationPlayer();
            m_AnimationPlayer.AddAnimation(CreateShowAnimation(), k_ShowAnimationName);
            m_AnimationPlayer.AddAnimation(CreateHideAnimation(), k_HideAnimationName);
            m_AnimationPlayer.animation = m_AnimationPlayer[k_ShowAnimationName];

            m_SubtitleAnimationPlayer = new AnimationPlayer();
            m_SubtitleAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;
            m_SubtitleAnimationPlayer.AddAnimation(CreateSubtitleAnimation(), k_SubtitleAnimationName);
            m_SubtitleAnimationPlayer.animation = m_SubtitleAnimationPlayer[k_SubtitleAnimationName];

            HideImmediate();
        }

        public override void Show()
        {
            m_AnimationPlayer.animation = m_AnimationPlayer[k_ShowAnimationName];
            m_AnimationPlayer.playbackSpeed = 1f;
            m_AnimationPlayer.Play();
        }

        public override void ShowImmediate()
        {
            m_AnimationPlayer.Stop();

            m_PostProcessingLayer.visible = false;
            m_Layer.blurSize = 0f;
            m_Layer.alpha = 1f;

            m_Title.animationProgress = 1f;
            m_Title.style.opacity = 1f;
            m_Title.label.style.opacity = 1f;

            m_Subtitle.style.opacity = 1f;

            m_SubtitleAnimationPlayer.Stop();
            m_SubtitleAnimationPlayer.Play();

            m_ActionMap.Enable();
        }

        public override void Hide()
        {
            m_AnimationPlayer.animation = m_AnimationPlayer[k_HideAnimationName];
            m_AnimationPlayer.playbackSpeed = 1f;
            m_AnimationPlayer.Play();
        }

        public override void HideImmediate()
        {
            m_AnimationPlayer.Stop();
            m_SubtitleAnimationPlayer.Stop();

            m_Layer.visible = false;
            m_PostProcessingLayer.visible = false;

            m_ActionMap.Disable();
        }

        KeyframeAnimation CreateShowAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                m_Layer.visible = true;
                m_PostProcessingLayer.visible = true;
                m_PostProcessingLayer.maskElement = m_Title;
                m_PostProcessingLayer.overscan = 8f;
                m_ActionMap.Enable();
            });

            var t1 = animation.AddTrack(opacity => m_Title.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack = animation.AddTrack(blurSize => m_PostProcessingLayer.blurSize = blurSize);
            blurTrack.AddKeyframe(10, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(30, 0f, Easing.StepOut);

            animation.AddEvent(30, () =>
            {
                m_PostProcessingLayer.maskElement = null;
            });

            var t2 = animation.AddTrack(animationProgress => m_Title.animationProgress = animationProgress);
            t2.AddKeyframe(30, 0f);
            t2.AddKeyframe(90, 1f);

            animation.AddEvent(90, () =>
            {
                m_PostProcessingLayer.overscan = new Overscan(8f, 8f, 0f, 8f);
                m_PostProcessingLayer.maskElement = m_Title.label;
            });

            var t3 = animation.AddTrack(opacity => m_Title.label.style.opacity = opacity);
            t3.AddKeyframe(90, 0f);
            t3.AddKeyframe(110, 1f);

            blurTrack.AddKeyframe(90, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(100, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(120, 0f, Easing.StepOut);

            animation.AddEvent(120, () =>
            {
                m_PostProcessingLayer.overscan = 8f;
                m_PostProcessingLayer.maskElement = m_Subtitle;
            });

            var t4 = animation.AddTrack(opacity => m_Subtitle.style.opacity = opacity);
            t4.AddKeyframe(120, 0f);
            t4.AddKeyframe(140, 1f);

            blurTrack.AddKeyframe(121, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(130, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(150, 0f, Easing.StepOut);

            animation.AddEvent(150, () =>
            {
                m_PostProcessingLayer.visible = false;
                m_SubtitleAnimationPlayer.Play();
            });

            return animation;
        }

        KeyframeAnimation CreateSubtitleAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(animationProgress => m_Subtitle.animationProgress = animationProgress);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(120, 1f);
            t1.AddKeyframe(180, 1f);    // Add one second of idle time.

            return animation;
        }

        KeyframeAnimation CreateHideAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                m_ActionMap.Disable();
            });

            var t1 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, Layer.DefaultBlurSize);

            var t2 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
            t2.AddKeyframe(10, 1f);
            t2.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                m_SubtitleAnimationPlayer.Stop();

                m_Layer.visible = false;
                m_PostProcessingLayer.visible = false;

                onHideFinished?.Invoke();
            });

            return animation;
        }

        public void OnAny(InputAction.CallbackContext ctx)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || m_QuitDialogBox != null)
            {
                return;
            }

            if (m_AnimationPlayer.animation == m_AnimationPlayer[k_ShowAnimationName] && m_AnimationPlayer.status.IsPlaying())
            {
                ShowImmediate();
            }
            else
            {
                BoardManager.StateMachine.state = BoardManager.StateMachine[BMState.InterfaceBoard];
            }
        }

        public void OnCancel(InputAction.CallbackContext ctx)
        {
            if (m_QuitDialogBox == null)
            {
                m_QuitDialogBox = DialogBox.CreateQuitDialogBox();
                m_QuitDialogBox.displaySortOrder = DisplaySortOrder + 100;
                m_QuitDialogBox.onHide += m_QuitDialogBox.Dispose;
                m_QuitDialogBox.onRightButtonClicked += OnQuitDialogBoxBackgroundOrRightButtonClicked;
                m_QuitDialogBox.onBackgroundClicked += OnQuitDialogBoxBackgroundOrRightButtonClicked;
                m_QuitDialogBox.onLeftButtonClicked += OnQuitDialogBoxLeftButtonClicked;
                m_QuitDialogBox.Show();
            }
            else
            {
                m_QuitDialogBox.Hide();
            }
        }

        void OnQuitDialogBoxBackgroundOrRightButtonClicked()
        {
            m_QuitDialogBox.Hide();
        }

        void OnQuitDialogBoxLeftButtonClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void OnConfirm(InputAction.CallbackContext ctx)
        {
            if (m_QuitDialogBox != null)
            {
                OnQuitDialogBoxLeftButtonClicked();
            }
        }

        void OnDestroy()
        {
            if (m_QuitDialogBox != null)
            {
                m_QuitDialogBox.Dispose();
            }
        }
    }
}
