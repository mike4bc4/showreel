using System;
using System.Collections;
using System.Collections.Generic;
using Controls.Raw;
using KeyframeSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialScene
{
    public class LoadingScreen : MonoBehaviour
    {
        const string k_DiamondName = "diamond";
        const string k_DiamondShowAnimationName = "DiamondShowAnimation";
        const string k_DiamondTilesAnimationName = "DiamondTilesAnimation";
        const string k_HideAnimationName = "HideAnimation";

        UIDocument m_Document;
        DiamondTiled m_Diamond;
        AnimationPlayer m_DiamondShowAnimationPlayer;
        AnimationPlayer m_DiamondTilesAnimationPlayer;
        AnimationPlayer m_HideAnimationPlayer;

        Action m_DiamondShowCompletedCallback;
        Action m_HideCompletedCallback;

        void Awake()
        {
            m_Document = GetComponent<UIDocument>();
            m_Diamond = m_Document.rootVisualElement.Q<DiamondTiled>(k_DiamondName);

            m_DiamondShowAnimationPlayer = new AnimationPlayer();
            m_DiamondShowAnimationPlayer.AddAnimation(CreateDiamondShowAnimation(), k_DiamondShowAnimationName);
            m_DiamondShowAnimationPlayer.animation = m_DiamondShowAnimationPlayer[k_DiamondShowAnimationName];

            m_DiamondTilesAnimationPlayer = new AnimationPlayer();
            m_DiamondTilesAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;
            m_DiamondTilesAnimationPlayer.AddAnimation(CreateDiamondTilesAnimation(), k_DiamondTilesAnimationName);
            m_DiamondTilesAnimationPlayer.animation = m_DiamondTilesAnimationPlayer[k_DiamondTilesAnimationName];

            m_HideAnimationPlayer = new AnimationPlayer();
            m_HideAnimationPlayer.AddAnimation(CreateHideAnimation(), k_HideAnimationName);
            m_HideAnimationPlayer.animation = m_HideAnimationPlayer[k_HideAnimationName];

            HideDiamondImmediate();
        }

        public void HideDiamondImmediate()
        {
            m_Diamond.style.opacity = 0f;
        }

        public void ShowDiamond(Action onCompleted = null)
        {
            m_DiamondShowCompletedCallback = onCompleted;
            m_DiamondShowAnimationPlayer.playbackSpeed = 1f;
            m_DiamondShowAnimationPlayer.Play();
        }

        public void Hide(Action onCompleted = null)
        {
            m_HideCompletedCallback = onCompleted;
            m_HideAnimationPlayer.playbackSpeed = 1f;
            m_HideAnimationPlayer.Play();
        }

        public void StartDiamondAnimation()
        {
            m_DiamondTilesAnimationPlayer.playbackSpeed = 1;
            m_DiamondTilesAnimationPlayer.Play();
        }

        KeyframeAnimation CreateHideAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(opacity => m_Document.rootVisualElement.style.opacity = opacity);
            t1.AddKeyframe(0, 1f);
            t1.AddKeyframe(30, 0f);

            animation.AddEvent(30, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_HideCompletedCallback?.Invoke();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateDiamondShowAnimation()
        {
            var animation = new KeyframeAnimation();

            // Child opacity does not override parent's setting.
            var t1 = animation.AddTrack(opacity => m_Diamond.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(15, 0f);
            t1.AddKeyframe(45, 1f);

            animation.AddEvent(45, () =>
            {
                if (animation.player.isPlayingForward)
                {
                    m_DiamondShowCompletedCallback?.Invoke();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateDiamondTilesAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(animationProgress => m_Diamond.animationProgress = animationProgress);
            t1.AddKeyframe(0, 0f, Easing.Linear);
            t1.AddKeyframe(240, 1f);

            return animation;
        }

        void OnDestroy()
        {
            // This object is a MonoBehaviour thus is not getting disposed immediately after destroy
            // is called. This means that animation players are still alive when this object and any
            // objects referenced by animation players have been unloaded. This leads to exception,
            // that's why we have to stop all animation players.
            m_HideAnimationPlayer.Stop();
            m_DiamondShowAnimationPlayer.Stop();
            m_DiamondTilesAnimationPlayer.Stop();
        }
    }
}
