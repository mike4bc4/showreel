using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Controls;
using Cysharp.Threading.Tasks;
using FSM;
using KeyframeSystem;
using Layers;
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

        Layer m_Layer;
        AnimationPlayer m_ShowHideAnimationPlayer;

        public InputActions.ActionMapsWrapper.InterfaceBoardActions inputActions
        {
            get => BoardManager.ActionMapsWrapper.InterfaceBoard;
        }

        public override void Init()
        {
            inputActions.Any.performed += OnAny;
            inputActions.Left.performed += OnLeft;
            inputActions.Right.performed += OnRight;
            inputActions.Confirm.performed += OnConfirm;
            inputActions.Cancel.performed += OnCancel;
            inputActions.Help.performed += OnHelp;

            m_ShowHideAnimationPlayer = new AnimationPlayer();
            m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
            m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

            m_Layer = LayerManager.CreateLayer("Interface");
            m_Layer.AddTemplateFromVisualTreeAsset(m_InterfaceBoardVisualTreeAsset);
            m_Layer.displaySortOrder = DisplaySortOrder;

            HideImmediate();
        }

        public override void Show()
        {
            m_ShowHideAnimationPlayer.playbackSpeed = 1f;
            m_ShowHideAnimationPlayer.Play();
        }

        public override void ShowImmediate()
        {
            m_ShowHideAnimationPlayer.Stop();
            m_Layer.visible = true;
            m_Layer.blocksRaycasts = true;
            m_Layer.interactable = true;
            m_Layer.alpha = 1f;
            m_Layer.blurSize = 0f;
            inputActions.Enable();
        }

        public override void Hide()
        {
            m_ShowHideAnimationPlayer.playbackSpeed = -1f;
            m_ShowHideAnimationPlayer.Play();
        }

        public override void HideImmediate()
        {
            m_ShowHideAnimationPlayer.Stop();
            m_Layer.visible = false;
            m_Layer.blocksRaycasts = false;
            m_Layer.interactable = false;
            inputActions.Disable();
        }

        KeyframeAnimation CreateShowHideAnimation()
        {
            var animation = new KeyframeAnimation();
            animation.AddEvent(0, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.visible = true;
                    m_Layer.blocksRaycasts = true;
                }
                else
                {
                    m_Layer.visible = false;
                    m_Layer.blocksRaycasts = false;
                }
            });

            var t1 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
            t2.AddKeyframe(10, Layer.DefaultBlurSize);
            t2.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_Layer.interactable = true;
                    inputActions.Enable();
                }
                else
                {
                    m_Layer.interactable = false;
                    inputActions.Disable();
                }
            });

            return animation;
        }

        void OnAny(InputAction.CallbackContext callbackContext) { }

        void OnLeft(InputAction.CallbackContext callbackContext) { }

        void OnRight(InputAction.CallbackContext callbackContext) { }

        void OnConfirm(InputAction.CallbackContext callbackContext) { }

        void OnCancel(InputAction.CallbackContext callbackContext) { }

        void OnHelp(InputAction.CallbackContext callbackContext) { }

        void OnDestroy()
        {
            LayerManager.RemoveLayer(m_Layer);
        }
    }
}
