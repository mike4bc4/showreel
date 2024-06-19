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

        List<Layer> layers
        {
            get => new List<Layer>() {
                m_BackgroundLayer,
                m_TextLayer,
                m_DiamondLineLayer,
                m_SecondaryTextLayer
            };
        }

        public void Hide()
        {
            throw new System.NotImplementedException();
        }

        public void Show()
        {
            SetLayersActive(true);

            IEnumerator Coroutine()
            {
                var anim = AnimationManager.Animate(m_DiamondLineLayer, nameof(m_DiamondLineLayer.alpha), 1f);
                anim.time = 1f;

                yield return m_WaitHalfSecond;

                var anim2 = AnimationManager.Animate(m_DiamondLineLayer.filter, nameof(BlurFilter.size), 0f);
                anim2.time = 1f;

                yield return anim2.coroutine;

                var diamondLine = m_DiamondLineLayer.rootVisualElement.Q<DiamondLineHorizontal>();
                diamondLine.Fold(immediate: true);
                diamondLine.Unfold();

                yield return m_WaitOneSecond;

                var anim3 = AnimationManager.Animate(m_TextLayer, nameof(m_TextLayer.alpha), 1f);
                anim3.time = 1f;

                yield return m_WaitHalfSecond;

                var anim4 = AnimationManager.Animate(m_TextLayer.filter, nameof(BlurFilter.size), 0f);
                anim4.time = 1f;

                yield return anim4.coroutine;

                var anim5 = AnimationManager.Animate(m_SecondaryTextLayer, nameof(m_SecondaryTextLayer.alpha), 1f);
                anim5.time = 1f;

                yield return m_WaitHalfSecond;

                var anim6 = AnimationManager.Animate(m_SecondaryTextLayer.filter, nameof(BlurFilter.size), 0f);
                anim6.time = 1f;

                yield return anim6.coroutine;

                float normalizedLabelWidth = m_SecondaryTextLayer.rootVisualElement.Q<Label>().resolvedStyle.width / Screen.width;
                float initialOffset = 0.5f - (normalizedLabelWidth / 2f) - (m_ShineWidth / 2f);
                var shineFilter = new ShineFilter(initialOffset, m_ShineWidth, m_ShineColor);
                m_SecondaryTextLayer.filter = shineFilter;

                while (true)
                {
                    shineFilter.offset = initialOffset;
                    var anim7 = AnimationManager.Animate(shineFilter, nameof(shineFilter.offset), 0.5f + (normalizedLabelWidth / 2f) + (m_ShineWidth / 2f));
                    anim7.time = 5f;
                    yield return anim7.coroutine;
                }
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
        }

        void Awake()
        {
            m_WaitOneSecond = new WaitForSeconds(1f);
            m_WaitHalfSecond = new WaitForSeconds(0.5f);
        }

        public void Init()
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

            SetLayersActive(false);

            var state = BoardManager.StateMachine.AddState(StateID);
            BoardManager.InitialState.AddConnection(state.id, () =>
            {
                Show();
            });
        }

        void SetLayersActive(bool active)
        {
            foreach (var layer in layers)
            {
                layer.gameObject.SetActive(active);
            }
        }
    }
}
