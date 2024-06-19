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

        Layer m_BackgroundLayer;
        Layer m_TextLayer;
        Layer m_DiamondLineLayer;
        Layer m_SecondaryTextLayer;
        Coroutine m_Coroutine;

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

                yield return new WaitForSeconds(0.5f);

                var anim2 = AnimationManager.Animate(m_DiamondLineLayer.filter, nameof(BlurFilter.size), 0f);
                anim2.time = 1f;

                yield return anim2.coroutine;

                var diamondLine = m_DiamondLineLayer.rootVisualElement.Q<DiamondLineHorizontal>();
                diamondLine.Unfold();

                yield return new WaitForSeconds(1f);

                var anim3 = AnimationManager.Animate(m_TextLayer, nameof(m_TextLayer.alpha), 1f);
                anim3.time = 1f;

                yield return new WaitForSeconds(0.5f);

                var anim4 = AnimationManager.Animate(m_TextLayer.filter, nameof(BlurFilter.size), 0f);
                anim4.time = 1f;

                yield return anim4.coroutine;

                var anim5 = AnimationManager.Animate(m_SecondaryTextLayer, nameof(m_SecondaryTextLayer.alpha), 1f);
                anim5.time = 1f;

                yield return new WaitForSeconds(0.5f);

                var anim6 = AnimationManager.Animate(m_SecondaryTextLayer.filter, nameof(BlurFilter.size), 0f);
                anim6.time = 1f;
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
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
