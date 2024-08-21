using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Controls;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
using Layers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Boards
{
    public class InterfaceBoard : Board
    {
        public const int InputSortOrder = 1000;   // Sorting order affects UI element picking.
        public const int DisplaySortOrder = 1000;   // Display order affects Layer sorting

        // const string k_ShowHideAnimationName = "ShowHideAnimation";

        // [SerializeField] VisualTreeAsset m_InterfaceBoardVisualTreeAsset;

        // Layer m_Layer;
        // VisualElement m_LayerContent;
        // AnimationPlayer m_ShowHideAnimationPlayer;
        // bool m_Visible;


        // public InputActions.ActionMapsWrapper.InterfaceBoardActions inputActions
        // {
        //     get => BoardManager.ActionMapsWrapper.InterfaceBoard;
        // }

        // // public override bool visible
        // // {
        // //     get => m_Visible;
        // //     set
        // //     {
        // //         if (value != m_Visible)
        // //         {
        // //             m_Visible = value;
        // //             m_Layer.visible = m_Visible;
        // //             if (m_Visible)
        // //             {
        // //                 m_Layer.rootVisualElement.Add(m_LayerContent);
        // //             }
        // //             else
        // //             {
        // //                 m_Layer.rootVisualElement.Remove(m_LayerContent);
        // //             }
        // //         }
        // //     }
        // // }

        // public override void Init()
        // {
        //     inputActions.Any.performed += OnAny;
        //     inputActions.Left.performed += OnLeft;
        //     inputActions.Right.performed += OnRight;
        //     inputActions.Confirm.performed += OnConfirm;
        //     inputActions.Cancel.performed += OnCancel;
        //     inputActions.Help.performed += OnHelp;

        //     m_ShowHideAnimationPlayer = new AnimationPlayer();
        //     m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
        //     m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

        //     m_LayerContent = m_InterfaceBoardVisualTreeAsset.Instantiate();

        //     m_Layer = LayerManager.CreateLayer("Interface");
        //     m_Layer.visible = false;
        //     m_Layer.displaySortOrder = DisplaySortOrder;
        //     m_Layer.inputSortOrder = InputSortOrder;
        //     // m_Layer.interactable = false;
        //     // m_Layer.blocksRaycasts = false;

        //     visible = false;
        // }

        // public override void Show()
        // {
        //     m_ShowHideAnimationPlayer.playbackSpeed = 1f;
        //     m_ShowHideAnimationPlayer.Play();
        // }

        // public override void ShowImmediate()
        // {
        //     m_Layer.interactable = true;
        //     m_Layer.blocksRaycasts = true;
        //     m_Layer.alpha = 1f;
        //     m_Layer.blurSize = 0f;
        // }

        // public override void Hide()
        // {
        //     m_ShowHideAnimationPlayer.playbackSpeed = -1f;
        //     m_ShowHideAnimationPlayer.Play();
        // }

        // public override void HideImmediate()
        // {

        // }

        // KeyframeAnimation CreateShowHideAnimation()
        // {
        //     var animation = new KeyframeAnimation();
        //     animation.AddEvent(0, () => OnShowHideAnimationFirstFrame(animation));

        //     var t1 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
        //     t1.AddKeyframe(0, 0f);
        //     t1.AddKeyframe(20, 1f);

        //     var t2 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
        //     t2.AddKeyframe(10, Layer.DefaultBlurSize);
        //     t2.AddKeyframe(30, 0f);

        //     animation.AddEvent(30, () => OnShowHideAnimationLastFrame(animation));

        //     return animation;
        // }

        // void OnShowHideAnimationFirstFrame(KeyframeAnimation animation)
        // {
        //     if (animation.player.playbackSpeed >= 0)
        //     {
        //         m_Layer.visible = true;
        //         m_Layer.blocksRaycasts = true;
        //     }
        //     else
        //     {
        //         m_Layer.visible = false;
        //         m_Layer.blocksRaycasts = false;
        //     }
        // }

        // void OnShowHideAnimationLastFrame(KeyframeAnimation animation)
        // {
        //     if (animation.player.playbackSpeed >= 0)
        //     {
        //         m_Layer.interactable = true;
        //     }
        //     else
        //     {
        //         m_Layer.interactable = false;
        //     }
        // }


        // void OnAny(InputAction.CallbackContext callbackContext)
        // {
        //     if (m_ShowHideAnimationPlayer.status.IsPlaying())
        //     {
        //         ShowImmediate();
        //     }
        // }

        // void OnLeft(InputAction.CallbackContext callbackContext)
        // {
        //     if (!m_Layer.interactable)
        //     {
        //         return;
        //     }
        // }

        // void OnRight(InputAction.CallbackContext callbackContext)
        // {
        //     if (!m_Layer.interactable)
        //     {
        //         return;
        //     }
        // }

        // void OnConfirm(InputAction.CallbackContext callbackContext) { }

        // void OnCancel(InputAction.CallbackContext callbackContext) { }

        // void OnHelp(InputAction.CallbackContext callbackContext) { }
    }
}
