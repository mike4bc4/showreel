using System;
using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        [SerializeField] VisualTreeAsset m_BackgroundVta;
        [SerializeField] VisualTreeAsset m_TextVta;
        [SerializeField] VisualTreeAsset m_DiamondLineVta;
        [SerializeField] VisualTreeAsset m_SecondaryTextVta;
        [SerializeField] float m_ShineWidth;
        [SerializeField] Color m_ShineColor;

        Layer m_BackgroundLayer;
        Layer m_TextLayer;
        Layer m_DiamondLineLayer;
        Layer m_SecondaryTextLayer;
        Coroutine m_Coroutine;
        WaitForSeconds m_WaitOneSecond;
        WaitForSeconds m_WaitHalfSecond;
        AnimationDescriptor<float> m_BlurAnimDescriptor;
        AnimationDescriptor<float> m_BlurZeroAnimDescriptor;
        AnimationDescriptor<float> m_AlphaZeroAnimDescriptor;
        AnimationDescriptor<float> m_AlphaOneAnimDescriptor;

        List<Layer> layers
        {
            get => new List<Layer>() {
                m_BackgroundLayer,
                m_TextLayer,
                m_DiamondLineLayer,
                m_SecondaryTextLayer
            };
        }

        public Coroutine Hide()
        {
            IEnumerator Coroutine()
            {
                if (m_SecondaryTextLayer.filter is ShineFilter)
                {
                    var anim1 = AnimationManager.Animate(m_SecondaryTextLayer.filter, nameof(ShineFilter.color), new Color(1f, 1f, 1f, 0f));
                    anim1.time = 0.25f;
                    yield return anim1.coroutine;
                }

                AnimationManager.Animate(m_TextLayer.filter, m_BlurAnimDescriptor);
                AnimationManager.Animate(m_DiamondLineLayer.filter, m_BlurAnimDescriptor);
                m_SecondaryTextLayer.filter = new BlurFilter() { size = 0f };
                AnimationManager.Animate(m_SecondaryTextLayer.filter, m_BlurAnimDescriptor);

                yield return m_WaitHalfSecond;

                AnimationManager.Animate(m_TextLayer, m_AlphaZeroAnimDescriptor);
                AnimationManager.Animate(m_DiamondLineLayer, m_AlphaZeroAnimDescriptor);
                var anim2 = AnimationManager.Animate(m_SecondaryTextLayer, m_AlphaZeroAnimDescriptor);

                yield return anim2.coroutine;

                foreach (var layer in layers)
                {
                    LayerManager.RemoveLayer(layer);
                }
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public Coroutine Show()
        {
            m_BackgroundLayer = LayerManager.AddNewLayer(m_BackgroundVta);

            m_TextLayer = LayerManager.AddNewLayer(m_TextVta);
            m_TextLayer.filter = new BlurFilter();
            m_TextLayer.alpha = 0f;

            m_DiamondLineLayer = LayerManager.AddNewLayer(m_DiamondLineVta);
            m_DiamondLineLayer.filter = new BlurFilter();
            m_DiamondLineLayer.alpha = 0f;

            m_SecondaryTextLayer = LayerManager.AddNewLayer(m_SecondaryTextVta);
            m_SecondaryTextLayer.filter = new BlurFilter();
            m_SecondaryTextLayer.alpha = 0f;

            IEnumerator Coroutine()
            {
                AnimationManager.Animate(m_DiamondLineLayer, m_AlphaOneAnimDescriptor);
                yield return m_WaitHalfSecond;

                var anim1 = AnimationManager.Animate(m_DiamondLineLayer.filter, m_BlurZeroAnimDescriptor);
                yield return anim1.coroutine;

                var diamondLine = m_DiamondLineLayer.rootVisualElement.Q<DiamondLineHorizontal>();
                diamondLine.Fold(immediate: true);
                diamondLine.Unfold();
                yield return m_WaitOneSecond;

                AnimationManager.Animate(m_TextLayer, m_AlphaOneAnimDescriptor);
                yield return m_WaitHalfSecond;

                var anim2 = AnimationManager.Animate(m_TextLayer.filter, m_BlurZeroAnimDescriptor);
                yield return anim2.coroutine;

                AnimationManager.Animate(m_SecondaryTextLayer, m_AlphaOneAnimDescriptor);
                yield return m_WaitHalfSecond;

                var anim3 = AnimationManager.Animate(m_SecondaryTextLayer.filter, m_BlurZeroAnimDescriptor);
                yield return anim3.coroutine;

                float normalizedLabelWidth = m_SecondaryTextLayer.rootVisualElement.Q<Label>().resolvedStyle.width / Screen.width;
                float initialOffset = 0.5f - (normalizedLabelWidth / 2f) - (m_ShineWidth / 2f);
                var shineFilter = new ShineFilter()
                {
                    offset = initialOffset,
                    width = m_ShineWidth,
                    color = m_ShineColor
                };

                m_SecondaryTextLayer.filter = shineFilter;
                while (true)
                {
                    shineFilter.offset = initialOffset;
                    var anim4 = AnimationManager.Animate(shineFilter, nameof(shineFilter.offset), 0.5f + (normalizedLabelWidth / 2f) + (m_ShineWidth / 2f));
                    anim4.time = 5f;
                    yield return anim4.coroutine;
                }
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
            m_WaitOneSecond = new WaitForSeconds(1f);
            m_WaitHalfSecond = new WaitForSeconds(0.5f);
            m_BlurAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(BlurFilter.size),
                targetValue = BlurFilter.DefaultSize,
                time = 1f,
            };

            m_BlurZeroAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(BlurFilter.size),
                targetValue = 0f,
                time = 1f,
            };

            m_AlphaZeroAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(Layer.alpha),
                targetValue = 0f,
                time = 1f,
            };

            m_AlphaOneAnimDescriptor = new AnimationDescriptor<float>()
            {
                property = nameof(Layer.alpha),
                targetValue = 1f,
                time = 1f,
            };
        }

        // void Update()
        // {
        //     if (Input.anyKeyDown)
        //     {
        //         Hide();
        //     }
        // }

        public void Init()
        {
            var state = BoardManager.StateMachine.AddState(StateID);
            BoardManager.InitialState.AddConnection(state.id, () =>
            {
                Show();
            });
        }
    }
}
