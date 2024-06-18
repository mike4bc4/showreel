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
        [SerializeField] float m_DiamondLineFadeTime;

        Layer m_BackgroundLayer;
        Layer m_TextLayer;
        Layer m_DiamondLineLayer;
        Coroutine m_Coroutine;

        List<Layer> layers
        {
            get => new List<Layer>() { m_BackgroundLayer, m_TextLayer, m_DiamondLineLayer };
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

                yield return anim2.coroutine;

                var diamondLine = m_DiamondLineLayer.rootVisualElement.Q<DiamondLineHorizontal>();
                diamondLine.Unfold();
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

            m_TextLayer = LayerManager.AddNewLayer("Text");
            m_TextLayer.alpha = 0;

            m_DiamondLineLayer = LayerManager.AddNewLayer(m_DiamondLineVta);
            m_DiamondLineLayer.filter = new BlurFilter();
            m_DiamondLineLayer.alpha = 0;

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
