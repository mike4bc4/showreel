using System.Collections;
using System.Collections.Generic;
using Boards;
using KeyframeSystem;
using Layers;
using UnityEngine;
using UnityEngine.UIElements;
using Controls.Raw;
using System.Linq;
using System;

public class DiamondBarBoard : Board
{
    const int k_DisplaySortOrder = 100;
    const string k_ShowHideAnimationName = "ShowHideAnimation";
    const string k_ActiveIndexAnimationName = "ActiveIndexAnimation";

    class BarElementHandler
    {
        const string k_LoopingAnimationName = "LoopingAnimation";
        const string k_TileScaleAnimationName = "TileScaleAnimation";

        DiamondBarElement m_Element;
        AnimationPlayer m_LoopingAnimationPlayer;
        AnimationPlayer m_TileScaleAnimationPlayer;

        public BarElementHandler(DiamondBarElement element)
        {
            m_Element = element;

            m_LoopingAnimationPlayer = new AnimationPlayer();
            m_LoopingAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;
            m_LoopingAnimationPlayer.AddAnimation(CreateLoopingAnimation(), k_LoopingAnimationName);
            m_LoopingAnimationPlayer.animation = m_LoopingAnimationPlayer[k_LoopingAnimationName];

            m_LoopingAnimationPlayer.Sample();  // Sample animation to apply initial animation progress.

            m_TileScaleAnimationPlayer = new AnimationPlayer();
            m_TileScaleAnimationPlayer.AddAnimation(CreateTileScaleAnimation(), k_TileScaleAnimationName);
            m_TileScaleAnimationPlayer.animation = m_TileScaleAnimationPlayer[k_TileScaleAnimationName];

            m_TileScaleAnimationPlayer.Sample();    // Sample animation to apply initial tile scale.
        }

        public void PlayLoopingAnimation()
        {
            m_LoopingAnimationPlayer.Play();
        }

        public void PauseLoopingAnimation()
        {
            m_LoopingAnimationPlayer.Pause();
        }

        public void TileScaleShrink()
        {
            m_TileScaleAnimationPlayer.playbackSpeed = 1f;
            m_TileScaleAnimationPlayer.Play();
        }

        public void TileScaleGrow()
        {
            m_TileScaleAnimationPlayer.playbackSpeed = -1f;
            m_TileScaleAnimationPlayer.Play();
        }

        KeyframeAnimation CreateLoopingAnimation()
        {
            var animation = new KeyframeAnimation();

            var t1 = animation.AddTrack(animationProgress => m_Element.diamond.animationProgress = animationProgress);
            t1.AddKeyframe(0, 0f, Easing.Linear);
            t1.AddKeyframe(240, 1f);

            return animation;
        }

        KeyframeAnimation CreateTileScaleAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (!animation.player.isPlayingForward)
                {
                    // Stop (reset) looping animation player when tile scale is one to avoid 'snappy'
                    // animation rewind effect.
                    m_LoopingAnimationPlayer.Stop();
                }
            });

            var t1 = animation.AddTrack(t => m_Element.diamond.targetTileScale = Mathf.Lerp(1f, DiamondTiled.DefaultTargetTileScale, t));
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            return animation;
        }
    }

    [SerializeField] VisualTreeAsset m_DiamondBarBoardVisualTreeAsset;

    UILayer m_Layer;
    DiamondBar m_DiamondBar;
    List<BarElementHandler> m_BarElementHandlers;

    AnimationPlayer m_ShowHideAnimationPlayer;
    AnimationPlayer m_ActiveIndexAnimationPlayer;

    int m_TargetActiveIndex;

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

    public int size
    {
        get => m_DiamondBar.size;
        set
        {
            m_DiamondBar.size = value;
            m_BarElementHandlers.Clear();
            foreach (var element in m_DiamondBar.elements)
            {
                m_BarElementHandlers.Add(new BarElementHandler(element));
            }
        }
    }

    public int activeIndex
    {
        get => m_TargetActiveIndex;
        set
        {
            if (value == m_TargetActiveIndex)
            {
                return;
            }

            m_TargetActiveIndex = value;

            // Reset currently active bar element.
            if (0 <= m_DiamondBar.activeIndex && m_DiamondBar.activeIndex < m_DiamondBar.size)
            {
                var elementHandler = m_BarElementHandlers[m_DiamondBar.activeIndex];
                elementHandler.TileScaleGrow();
                elementHandler.PauseLoopingAnimation();
            }

            // Delay active element switch with animation.
            if (m_ActiveIndexAnimationPlayer.animationTime > 0f)
            {
                m_ActiveIndexAnimationPlayer.playbackSpeed = -1f;
                m_ActiveIndexAnimationPlayer.Play();
            }
            else
            {
                UpdateActiveIndexAndStartAnimation();
            }
        }
    }

    void UpdateActiveIndexAndStartAnimation()
    {
        m_DiamondBar.activeIndex = m_TargetActiveIndex;

        // Start switch animation only if there actually is an element to switch to.
        if (m_TargetActiveIndex >= 0)
        {
            m_ActiveIndexAnimationPlayer.playbackSpeed = 1f;
            m_ActiveIndexAnimationPlayer.Play();
        }
    }

    public override void Init()
    {
        m_BarElementHandlers = new List<BarElementHandler>();

        m_Layer = LayerManager.CreateUILayer("DiamondBar");
        m_Layer.AddTemplateFromVisualTreeAsset(m_DiamondBarBoardVisualTreeAsset);
        m_Layer.displaySortOrder = k_DisplaySortOrder;

        m_DiamondBar = m_Layer.rootVisualElement.Q<DiamondBar>();

        m_ShowHideAnimationPlayer = new AnimationPlayer();
        m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
        m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

        m_ActiveIndexAnimationPlayer = new AnimationPlayer();
        m_ActiveIndexAnimationPlayer.AddAnimation(CreateActiveIndexAnimation(), k_ActiveIndexAnimationName);
        m_ActiveIndexAnimationPlayer.animation = m_ActiveIndexAnimationPlayer[k_ActiveIndexAnimationName];

        size = m_DiamondBar.size;
        activeIndex = m_DiamondBar.activeIndex;

        HideImmediate();
        interactable = false;
        blocksRaycasts = false;
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
                m_Layer.visible = false;
                m_IsVisible = false;
                m_HideCompletedCallback?.Invoke();
            }
        });

        var t1 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
        t1.AddKeyframe(0, 0f);
        t1.AddKeyframe(20, 1f);

        var t2 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
        t2.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
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

    KeyframeAnimation CreateActiveIndexAnimation()
    {
        var animation = new KeyframeAnimation();
        animation.AddEvent(0, () =>
        {
            if (!animation.player.isPlayingForward && m_DiamondBar.activeIndex != m_TargetActiveIndex)
            {
                // Was playing backwards and reached first frame, so switch active index and play
                // bounce back animation.
                UpdateActiveIndexAndStartAnimation();
            }
        });

        var t1 = animation.AddTrack(animationProgress => m_DiamondBar.animationProgress = animationProgress);
        t1.AddKeyframe(0, 0f);
        t1.AddKeyframe(20, 1f);

        animation.AddEvent(20, () =>
        {
            if (animation.player.isPlayingForward)
            {
                // 'Enable' element by starting it's animation.
                var elementHandler = m_BarElementHandlers.ElementAtOrDefault(m_DiamondBar.activeIndex);
                if (elementHandler != null)
                {
                    // Change element's target tile scale over time for smooth transition.
                    elementHandler.TileScaleShrink();
                    elementHandler.PlayLoopingAnimation();
                }
            }
        });

        return animation;
    }
}
