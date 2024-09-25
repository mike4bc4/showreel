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
    const string k_TileScaleAnimationName = "TileScaleAnimation";
    const string k_LoopingAnimationName = "LoopingAnimation";

    [SerializeField] VisualTreeAsset m_DiamondBarBoardVisualTreeAsset;

    UILayer m_Layer;
    DiamondBar m_DiamondBar;

    AnimationPlayer m_ShowHideAnimationPlayer;
    AnimationPlayer m_ActiveIndexAnimationPlayer;
    AnimationPlayer m_LoopingAnimationPlayer;

    Action m_ActiveIndexGrowCompletedCallback;
    Action m_ActiveIndexShrinkCompletedCallback;

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
        set => m_DiamondBar.size = value;
    }

    public int activeIndex
    {
        get => m_TargetActiveIndex;
        set
        {
            m_TargetActiveIndex = Mathf.Clamp(value, -1, m_DiamondBar.size - 1);
            if (value == m_DiamondBar.activeIndex)
            {
                return;
            }

            if (m_DiamondBar.activeDiamond != null)
            {
                ShrinkActiveIndex(() =>
                {
                    m_LoopingAnimationPlayer.Stop();
                    m_DiamondBar.activeDiamond.animationProgress = 0;

                    m_DiamondBar.activeIndex = value;
                    if (m_DiamondBar.activeIndex >= 0)
                    {
                        GrowActiveIndex(() => m_LoopingAnimationPlayer.Play());
                    }
                });
            }
            else
            {
                m_DiamondBar.activeIndex = value;
                if (m_DiamondBar.activeIndex >= 0)
                {
                    GrowActiveIndex(() => m_LoopingAnimationPlayer.Play());
                }
            }
        }
    }

    public override void Init()
    {
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

        m_LoopingAnimationPlayer = new AnimationPlayer();
        m_LoopingAnimationPlayer.wrapMode = KeyframeSystem.WrapMode.Loop;
        m_LoopingAnimationPlayer.AddAnimation(CreateLoopingAnimation(), k_LoopingAnimationName);
        m_LoopingAnimationPlayer.animation = m_LoopingAnimationPlayer[k_LoopingAnimationName];

        size = m_DiamondBar.size;
        m_TargetActiveIndex = m_DiamondBar.activeIndex;
        if (m_DiamondBar.activeDiamond != null)
        {
            m_ActiveIndexAnimationPlayer.animationTime = m_ActiveIndexAnimationPlayer.duration;
            m_DiamondBar.animationProgress = 1f;
            m_LoopingAnimationPlayer.Play();
        }

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

    void GrowActiveIndex(Action onCompleted = null)
    {
        m_ActiveIndexGrowCompletedCallback = onCompleted;
        m_ActiveIndexAnimationPlayer.playbackSpeed = 1f;
        m_ActiveIndexAnimationPlayer.Play();
    }

    void ShrinkActiveIndex(Action onCompleted = null)
    {
        m_ActiveIndexShrinkCompletedCallback = onCompleted;
        m_ActiveIndexAnimationPlayer.playbackSpeed = -1f;
        m_ActiveIndexAnimationPlayer.Play();
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

    KeyframeAnimation CreateLoopingAnimation()
    {
        var animation = new KeyframeAnimation();

        var t1 = animation.AddTrack(animationProgress => m_DiamondBar.activeDiamond.animationProgress = animationProgress);
        t1.AddKeyframe(0, 0f, Easing.Linear);
        t1.AddKeyframe(240, 1f);

        return animation;
    }

    KeyframeAnimation CreateActiveIndexAnimation()
    {
        var animation = new KeyframeAnimation();
        animation.AddEvent(0, () =>
        {
            if (!animation.player.isPlayingForward)
            {
                m_ActiveIndexShrinkCompletedCallback?.Invoke();
            }
        });

        var t1 = animation.AddTrack(t =>
        {
            m_DiamondBar.animationProgress = t;
            m_DiamondBar.activeDiamond.targetTileScale = Mathf.Lerp(1, DiamondTiled.DefaultTargetTileScale, t);
        });
        t1.AddKeyframe(0, 0f);
        t1.AddKeyframe(20, 1f);

        animation.AddEvent(20, () =>
        {
            if (animation.player.isPlayingForward)
            {
                m_ActiveIndexGrowCompletedCallback?.Invoke();
            }
        });

        return animation;
    }
}
