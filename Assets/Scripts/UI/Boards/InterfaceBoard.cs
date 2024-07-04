using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Boards
{
    public class InterfaceBoard : Board, IBoard
    {
        public const int SortingOrder = 1000;    // Sorting order affects UI element picking.
        public const int DisplayOrder = 1000;

        [SerializeField] VisualTreeAsset m_ControlsVta;

        Layer m_ControlsLayer;
        Coroutine m_Coroutine;
        WaitForSeconds m_WaitHalfSecond;

        public void Init()
        {
            m_ControlsLayer = LayerManager.AddNewLayer(m_ControlsVta, "InterfaceControls");
            m_ControlsLayer.displayOrder = DisplayOrder;
            m_ControlsLayer.alpha = 0f;
            m_ControlsLayer.filter = new BlurFilter();
            m_ControlsLayer.panelSortingOrder = SortingOrder;
        }

        public Coroutine Show()
        {
            IEnumerator Coroutine()
            {
                AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.AlphaOne);
                yield return m_WaitHalfSecond;

                AnimationManager.Animate(m_ControlsLayer.filter, AnimationDescriptor.BlurZero);
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        public Coroutine Hide()
        {
            IEnumerator Coroutine()
            {
                AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.BlurDefault);
                yield return m_WaitHalfSecond;

                AnimationManager.Animate(m_ControlsLayer.filter, AnimationDescriptor.AlphaZero);
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
        }

        void Start()
        {
            Show();
        }
    }
}
