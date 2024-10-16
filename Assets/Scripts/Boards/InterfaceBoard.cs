using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Controls;
using Cysharp.Threading.Tasks;
using InputHelper;
using KeyframeSystem;
using Layers;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Boards
{
    public class InterfaceBoard : Board
    {
        public const int DisplaySortOrder = 1000;
        const string k_ShowHideAnimationName = "ShowHideAnimation";

        [SerializeField] VisualTreeAsset m_InterfaceBoardVisualTreeAsset;

        UILayer m_Layer;
        AnimationPlayer m_ShowHideAnimationPlayer;
        DialogBox m_InfoDialogBox;
        DialogBox m_QuitDialogBox;
        ButtonControl m_LeftButton;
        ButtonControl m_RightButton;
        ButtonControl m_InfoButton;
        ButtonControl m_SettingsButton;
        ButtonControl m_QuitButton;

        public override bool interactable
        {
            get => m_Layer.interactable;
            set => m_Layer.interactable = value;
        }

        public override bool blocksRaycasts
        {
            get => m_Layer.blocksRaycasts;
            set => m_Layer.blocksRaycasts = value;
        }

        public override void Init()
        {
            m_ShowHideAnimationPlayer = new AnimationPlayer();
            m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
            m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

            m_Layer = LayerManager.CreateUILayer("Interface");
            m_Layer.AddTemplateFromVisualTreeAsset(m_InterfaceBoardVisualTreeAsset);
            m_Layer.displaySortOrder = DisplaySortOrder;

            m_LeftButton = m_Layer.rootVisualElement.Q<ButtonControl>("left-button");
            m_RightButton = m_Layer.rootVisualElement.Q<ButtonControl>("right-button");
            m_InfoButton = m_Layer.rootVisualElement.Q<ButtonControl>("info-button");
            m_SettingsButton = m_Layer.rootVisualElement.Q<ButtonControl>("settings-button");
            m_QuitButton = m_Layer.rootVisualElement.Q<ButtonControl>("quit-button");

            m_LeftButton.clicked += OnLeftButtonClicked;
            m_RightButton.clicked += OnRightButtonClicked;
            m_InfoButton.clicked += OnInfoButtonClicked;
            m_SettingsButton.clicked += OnSettingsButtonClicked;
            m_QuitButton.clicked += OnQuitButtonClicked;

            HideImmediate();
            interactable = false;
            blocksRaycasts = false;
        }

        void OnLeftButtonClicked()
        {
            BoardManager.BoardStateContext.Any();
            BoardManager.BoardStateContext.Left();
        }

        void OnRightButtonClicked()
        {
            BoardManager.BoardStateContext.Any();
            BoardManager.BoardStateContext.Right();
        }

        void OnInfoButtonClicked()
        {
            BoardManager.BoardStateContext.Any();
            BoardManager.BoardStateContext.Info();
        }

        void OnSettingsButtonClicked()
        {
            BoardManager.BoardStateContext.Any();
            BoardManager.BoardStateContext.Settings();
        }

        void OnQuitButtonClicked()
        {
            BoardManager.BoardStateContext.Any();
            BoardManager.BoardStateContext.Cancel();
        }

        public override void Show(Action onCompleted = null)
        {
            base.Show(onCompleted);
            m_ShowHideAnimationPlayer.playbackSpeed = 1f;
            m_ShowHideAnimationPlayer.Play();
        }

        public override void ShowImmediate()
        {
            m_ShowHideAnimationPlayer.Stop();
            m_ShowHideAnimationPlayer.FastForward();
            m_Layer.visible = true;
            // m_Layer.blocksRaycasts = true;
            // m_Layer.interactable = true;
            m_Layer.alpha = 1f;
            m_Layer.blurSize = 0f;
            m_IsVisible = true;
        }

        public override void Hide(Action onCompleted = null)
        {
            base.Hide(onCompleted);
            m_ShowHideAnimationPlayer.playbackSpeed = -1f;
            m_ShowHideAnimationPlayer.Play();
        }

        public override void HideImmediate()
        {
            m_ShowHideAnimationPlayer.Stop();
            m_Layer.visible = false;
            m_IsVisible = false;
        }

        KeyframeAnimation CreateShowHideAnimation()
        {
            var animation = new KeyframeAnimation();
            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.visible = true;
                }
                else
                {
                    m_IsVisible = false;
                    m_HideCompletedCallback?.Invoke();
                }
            });

            var t1 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
            t2.AddKeyframe(10, UILayer.DefaultBlurSize);
            t2.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_IsVisible = true;
                    m_ShowCompletedCallback?.Invoke();
                }
            });

            return animation;
        }
    }
}
