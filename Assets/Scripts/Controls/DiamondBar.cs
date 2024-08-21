using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controls.Raw;
using Extensions;
using KeyframeSystem;
using Layers;
using Templates;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controls
{
    public class DiamondBar : DisposableObject
    {
        const int k_DefaultDisplaySortOrder = 100;
        const string k_HideShowAnimationName = "HideShowAnimation";
        const string k_ActiveIndexAnimationName = "ActiveIndexAnimation";
        const string k_BarElementAnimationName = "BarElementAnimation";

        class ElementHandler
        {
            const string k_LoopingAnimationName = "LoopingAnimation";
            const string k_TileScaleAnimationName = "TileScaleAnimation";

            DiamondBarElement m_Element;
            AnimationPlayer m_LoopingAnimationPlayer;
            AnimationPlayer m_TileScaleAnimationPlayer;

            public ElementHandler(DiamondBarElement element)
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

            void OnTileScaleAnimationFirstFrame()
            {
                // Stop (reset) looping animation player when tile scale is one to avoid 'snappy'
                // animation rewind effect.
                if (m_TileScaleAnimationPlayer.playbackSpeed < 0)
                {
                    m_LoopingAnimationPlayer.Stop();
                }
            }

            KeyframeAnimation CreateTileScaleAnimation()
            {
                var animation = new KeyframeAnimation();
                animation.AddEvent(0, OnTileScaleAnimationFirstFrame);
                var t1 = animation.AddTrack(t => m_Element.diamond.targetTileScale = Mathf.Lerp(1f, DiamondTiled.DefaultTargetTileScale, t));
                t1.AddKeyframe(0, 0f);
                t1.AddKeyframe(20, 1f);
                return animation;
            }
        }

        Layer m_Layer;
        AnimationPlayer m_HideShowAnimationPlayer;
        AnimationPlayer m_ActiveIndexAnimationPlayer;
        Raw.DiamondBar m_Bar;
        int m_TargetActiveIndex;
        List<ElementHandler> m_BarElementHandlers;

        public int displaySortOrder
        {
            get => m_Layer.displaySortOrder;
            set => m_Layer.displaySortOrder = value;
        }

        public int size
        {
            get => m_Bar.size;
            set
            {
                m_Bar.size = value;
                m_BarElementHandlers.Clear();
                foreach (var element in m_Bar.elements)
                {
                    m_BarElementHandlers.Add(new ElementHandler(element));
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
                if (0 <= m_Bar.activeIndex && m_Bar.activeIndex < m_Bar.size)
                {
                    var elementHandler = m_BarElementHandlers[m_Bar.activeIndex];
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

        public DiamondBar()
        {
            m_BarElementHandlers = new List<ElementHandler>();

            m_Layer = LayerManager.CreateLayer();
            displaySortOrder = k_DefaultDisplaySortOrder;

            m_Bar = new Raw.DiamondBar();
            m_Layer.rootVisualElement.Add(m_Bar);

            m_HideShowAnimationPlayer = new AnimationPlayer();
            m_HideShowAnimationPlayer.AddAnimation(CreateHideShowAnimation(), k_HideShowAnimationName);
            m_HideShowAnimationPlayer.animation = m_HideShowAnimationPlayer[k_HideShowAnimationName];

            m_ActiveIndexAnimationPlayer = new AnimationPlayer();
            m_ActiveIndexAnimationPlayer.AddAnimation(CreateActiveIndexAnimation(), k_ActiveIndexAnimationName);
            m_ActiveIndexAnimationPlayer.animation = m_ActiveIndexAnimationPlayer[k_ActiveIndexAnimationName];

            size = Raw.DiamondBar.DefaultSize;
            activeIndex = -1;

            HideImmediate();
        }

        void UpdateActiveIndexAndStartAnimation()
        {
            m_Bar.activeIndex = m_TargetActiveIndex;

            // Start switch animation only if there actually is an element to switch to.
            if (m_TargetActiveIndex >= 0)
            {
                m_ActiveIndexAnimationPlayer.playbackSpeed = 1f;
                m_ActiveIndexAnimationPlayer.Play();
            }
        }

        void OnActiveIndexAnimationFirstFrame()
        {
            if (m_Bar.activeIndex != m_TargetActiveIndex)
            {
                UpdateActiveIndexAndStartAnimation();
            }
        }

        void OnActiveIndexAnimationLastFrame()
        {
            // Last frame reached while playing forwards.
            if (m_ActiveIndexAnimationPlayer.playbackSpeed >= 0f)
            {
                // 'Enable' element by starting it's animation.
                var elementHandler = m_BarElementHandlers.ElementAtOrDefault(m_Bar.activeIndex);
                if (elementHandler != null)
                {
                    // Change element's target tile scale over time for smooth transition.
                    elementHandler.TileScaleShrink();
                    elementHandler.PlayLoopingAnimation();
                }
            }
        }

        public void Show()
        {
            m_HideShowAnimationPlayer.playbackSpeed = 1f;
            m_HideShowAnimationPlayer.Play();
        }

        public void Hide()
        {
            m_HideShowAnimationPlayer.playbackSpeed = -1f;
            m_HideShowAnimationPlayer.Play();
        }

        public void HideImmediate()
        {
            m_HideShowAnimationPlayer.Stop();
            m_Layer.visible = false;
        }

        KeyframeAnimation CreateActiveIndexAnimation()
        {
            var animation = new KeyframeAnimation();
            animation.AddEvent(0, OnActiveIndexAnimationFirstFrame);

            var t1 = animation.AddTrack(animationProgress => m_Bar.animationProgress = animationProgress);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            animation.AddEvent(20, OnActiveIndexAnimationLastFrame);

            return animation;
        }

        KeyframeAnimation CreateHideShowAnimation()
        {
            var animation = new KeyframeAnimation();
            animation.AddEvent(0, () =>
            {
                if (m_HideShowAnimationPlayer.playbackSpeed >= 0)
                {
                    m_Layer.visible = true;
                }
                else
                {
                    m_Layer.visible = false;
                }
            });

            var t1 = animation.AddTrack(alpha => m_Layer.alpha = alpha);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var t2 = animation.AddTrack(blurSize => m_Layer.blurSize = blurSize);
            t2.AddKeyframe(10, PostProcessingLayer.DefaultBlurSize);
            t2.AddKeyframe(30, 0f);

            return animation;
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            m_HideShowAnimationPlayer.Stop();

            LayerManager.RemoveLayer(m_Layer);

            m_Disposed = true;
        }
    }
}
