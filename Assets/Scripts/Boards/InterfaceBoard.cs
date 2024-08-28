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
        Button m_LeftButton;
        Button m_RightButton;
        Button m_InfoButton;
        Button m_QuitButton;

        public bool isVisible{
            get => m_ShowHideAnimationPlayer.animationTime == m_ShowHideAnimationPlayer.duration;
        }

        public override void Init()
        {
            m_ShowHideAnimationPlayer = new AnimationPlayer();
            m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
            m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

            m_Layer = LayerManager.CreateLayer("Interface");
            m_Layer.AddTemplateFromVisualTreeAsset(m_InterfaceBoardVisualTreeAsset);
            m_Layer.displaySortOrder = DisplaySortOrder;

            HideImmediate();
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
            m_Layer.blocksRaycasts = true;
            m_Layer.interactable = true;
            m_Layer.alpha = 1f;
            m_Layer.blurSize = 0f;
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
            m_Layer.blocksRaycasts = false;
            m_Layer.interactable = false;
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
                    m_HideCompletedCallback?.Invoke();
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
                    m_ShowCompletedCallback?.Invoke();
                }
                else
                {
                    m_Layer.interactable = false;
                }
            });

            return animation;
        }
    }
}
