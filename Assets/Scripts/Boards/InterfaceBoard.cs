using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Controls;
using Cysharp.Threading.Tasks;
using FSM;
using InputHelper;
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
        DialogBox m_InfoDialogBox;
        DialogBox m_QuitDialogBox;
        InputActionMap m_ActionMap;
        InputAction m_HelpInputAction;
        InputAction m_CancelInputAction;

        public override void Init()
        {
            m_ActionMap = BoardManager.InputActions.FindActionMap("InterfaceBoard");
            m_ActionMap["Any"].GetHelper().performed += OnAny;
            m_ActionMap["Left"].GetHelper().performed += OnLeft;
            m_ActionMap["Right"].GetHelper().performed += OnRight;
            m_ActionMap["Confirm"].GetHelper().performed += OnConfirm;

            m_HelpInputAction = m_ActionMap["Help"];
            m_HelpInputAction.GetHelper().performed += OnHelp;

            m_CancelInputAction = m_ActionMap["Cancel"];
            m_CancelInputAction.GetHelper().performed += OnCancel;

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
            m_ActionMap.Enable();
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
            m_ActionMap.Disable();
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
                    m_ActionMap.Enable();
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
                }
                else
                {
                    m_Layer.interactable = false;
                    m_ActionMap.Disable();
                }
            });

            return animation;
        }

        void OnAny(InputAction.CallbackContext callbackContext)
        {
            // Cancel input action has priority over this action callback as it allows to open quit
            // dialog box.
            if (m_CancelInputAction.WasPerformedThisFrame())
            {
                return;
            }

            if (m_ShowHideAnimationPlayer.status.IsPlaying())
            {
                ShowImmediate();

                // Because diamond bar board is being shown in parallel with interface board also
                // skip its show animation if possible.
                var diamondBarBoard = BoardManager.GetBoard<DiamondBarBoard>();
                if (diamondBarBoard.isShowing)
                {
                    diamondBarBoard.ShowImmediate();
                }
            }
        }

        void OnLeft(InputAction.CallbackContext callbackContext)
        {
            if (!m_Layer.interactable)
            {
                return;
            }
        }

        void OnRight(InputAction.CallbackContext callbackContext)
        {
            if (!m_Layer.interactable)
            {
                return;
            }
        }

        void OnConfirm(InputAction.CallbackContext callbackContext)
        {
            if (m_InfoDialogBox != null)
            {
                m_InfoDialogBox.Hide();
            }
            else if (m_QuitDialogBox != null)
            {
                m_QuitDialogBox.PerformClick(DialogBox.ButtonIndex.Left);
            }
        }

        void OnCancel(InputAction.CallbackContext callbackContext)
        {
            if (m_InfoDialogBox != null)
            {
                m_InfoDialogBox.PerformClick(DialogBox.ButtonIndex.Left);
            }
            else if (m_QuitDialogBox != null)
            {
                m_QuitDialogBox.PerformClick(DialogBox.ButtonIndex.Right);
            }
            else if (m_QuitDialogBox == null)
            {
                m_QuitDialogBox = DialogBox.CreateQuitDialogBox();
                m_QuitDialogBox.displaySortOrder = DisplaySortOrder + 100;
                m_QuitDialogBox.onHide += m_QuitDialogBox.Dispose;
                m_QuitDialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, m_QuitDialogBox.Hide);
                m_QuitDialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, m_QuitDialogBox.Hide);
                m_QuitDialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, () =>
                {
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                });

                m_QuitDialogBox.Show();
            }
        }

        void OnHelp(InputAction.CallbackContext callbackContext)
        {
            if (m_QuitDialogBox != null || m_ShowHideAnimationPlayer.status.IsPlaying())
            {
                return;
            }

            if (m_InfoDialogBox == null)
            {
                m_InfoDialogBox = DialogBox.CreateInfoDialogBox();
                m_InfoDialogBox.displaySortOrder = DisplaySortOrder + 100;
                m_InfoDialogBox.onHide += m_InfoDialogBox.Dispose;
                m_InfoDialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, m_InfoDialogBox.Hide);
                m_InfoDialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, m_InfoDialogBox.Hide);
                m_InfoDialogBox.Show();
            }
            else
            {
                m_InfoDialogBox.PerformClick(DialogBox.ButtonIndex.Left);
            }
        }

        void OnDestroy()
        {
            LayerManager.RemoveLayer(m_Layer);
            if (m_InfoDialogBox != null)
            {
                m_InfoDialogBox.Dispose();
            }

            if (m_QuitDialogBox != null)
            {
                m_QuitDialogBox.Dispose();
            }
        }
    }
}
