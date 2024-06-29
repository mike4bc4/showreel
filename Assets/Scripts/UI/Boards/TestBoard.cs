using System;
using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace UI.Boards
{
    public class TestBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        [SerializeField] VisualTreeAsset m_BackgroundVta;
        [SerializeField] VisualTreeAsset m_DiamondFrameVta;
        [SerializeField] VisualTreeAsset m_EmptyVta;
        [SerializeField] VideoClip m_VideoClip;

        Layer m_BackgroundLayer;
        Layer m_DiamondFrameLayer;
        Layer m_VideoClipLayer;
        Coroutine m_Coroutine;
        WaitForSeconds m_WaitHalfSecond;
        AnimationDescriptor<float> m_AlphaOneAnimDescriptor;
        AnimationDescriptor<float> m_BlurZeroAnimDescriptor;
        VideoPlayer m_VideoPlayer;

        public Coroutine Hide()
        {
            throw new System.NotImplementedException();
        }

        public void Init()
        {
            m_VideoPlayer = VideoPlayerManager.CreatePlayer(m_VideoClip);
            var state = BoardManager.StateMachine.AddState(StateID);
            BoardManager.InitialState.AddConnection(state.id, () =>
            {
                Show();
            });
        }

        public Coroutine Show()
        {
            m_BackgroundLayer = LayerManager.AddNewLayer(m_BackgroundVta);

            m_DiamondFrameLayer = LayerManager.AddNewLayer(m_DiamondFrameVta);
            m_DiamondFrameLayer.filter = new BlurFilter();
            m_DiamondFrameLayer.alpha = 0f;

            m_VideoClipLayer = LayerManager.AddNewLayer(m_EmptyVta);
            m_VideoClipLayer.filter = new BlurFilter();
            m_VideoClipLayer.alpha = 0f;

            IEnumerator Coroutine()
            {
                var diamondFrame = m_DiamondFrameLayer.rootVisualElement.Q<DiamondFrameVertical>();
                diamondFrame.Fold(immediate: true);

                // Hide video container, so diamond frame unfolds as empty.
                var videoContainer = diamondFrame.Q<VisualElement>("video-container");
                videoContainer.style.visibility = Visibility.Hidden;

                AnimationManager.Animate(m_DiamondFrameLayer, m_AlphaOneAnimDescriptor);
                yield return m_WaitHalfSecond;

                var anim1 = AnimationManager.Animate(m_DiamondFrameLayer.filter, m_BlurZeroAnimDescriptor);
                yield return anim1.coroutine;

                yield return diamondFrame.Unfold();

                // Start rendering video to render texture and set it as background for blurred overlay to show.
                m_VideoPlayer.Play();

                var snapshot = videoContainer.CreateSnapshot();
                snapshot.style.visibility = Visibility.Visible;
                snapshot.style.backgroundImage = Background.FromRenderTexture(m_VideoPlayer.targetTexture);

                m_VideoClipLayer.rootVisualElement.Clear();
                m_VideoClipLayer.rootVisualElement.Add(snapshot);

                AnimationManager.Animate(m_VideoClipLayer, m_AlphaOneAnimDescriptor);
                yield return m_WaitHalfSecond;

                var anim2 = AnimationManager.Animate(m_VideoClipLayer.filter, m_BlurZeroAnimDescriptor);
                yield return anim2.coroutine;

                // Display video on diamond frame layer.
                videoContainer.style.backgroundImage = Background.FromRenderTexture(m_VideoPlayer.targetTexture);
                videoContainer.style.visibility = Visibility.Visible;

                // Wait for changes to apply and hide overlay layer.
                yield return null;
                m_VideoClipLayer.alpha = 0f;
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        void Awake()
        {
            m_WaitHalfSecond = new WaitForSeconds(0.5f);
            m_AlphaOneAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(Layer.alpha),
                targetValue = 1f,
                time = 1f,
            };

            m_BlurZeroAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(BlurFilter.size),
                targetValue = 0f,
                time = 1f,
            };
        }

        void Destroy()
        {
            // Remove video players to release render textures.
            if (m_VideoPlayer != null)
            {
                VideoPlayerManager.RemovePlayer(m_VideoPlayer);
            }
        }
    }
}