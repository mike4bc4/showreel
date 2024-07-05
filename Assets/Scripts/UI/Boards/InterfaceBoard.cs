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
        [SerializeField] float m_FadeTime;

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

        void StopAnimations()
        {
            AnimationManager.StopAnimation(m_ControlsLayer, nameof(Layer.alpha));
            AnimationManager.StopAnimation(m_ControlsLayer.filter, nameof(BlurFilter.size));
        }

        public Coroutine Show()
        {
            StopAnimations();
            IEnumerator Coroutine()
            {
                var show = m_ControlsLayer.alpha != 1f;
                var focus = ((BlurFilter)m_ControlsLayer.filter).size != 0f;
                var coroutines = new List<Coroutine>();

                if (show)
                {
                    var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.AlphaOne);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                    yield return new WaitForSeconds(m_FadeTime * 0.5f);
                }

                if (focus)
                {
                    var animation = AnimationManager.Animate(m_ControlsLayer.filter, AnimationDescriptor.BlurZero);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                foreach (var coroutine in coroutines)
                {
                    yield return coroutine;
                }
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
            StopAnimations();
            IEnumerator Coroutine()
            {
                var blur = ((BlurFilter)m_ControlsLayer.filter).size != BlurFilter.DefaultSize;
                var hide = m_ControlsLayer.alpha != 0f;
                var coroutines = new List<Coroutine>();

                if (blur)
                {
                    var animation = AnimationManager.Animate(m_ControlsLayer.filter, AnimationDescriptor.BlurDefault);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                    yield return new WaitForSeconds(m_FadeTime * 0.5f);
                }

                if (hide)
                {
                    var animation = AnimationManager.Animate(m_ControlsLayer, AnimationDescriptor.AlphaZero);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                foreach (var coroutine in coroutines)
                {
                    yield return coroutine;
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
            m_WaitHalfSecond = new WaitForSeconds(0.5f);
        }

        void Start()
        {
            Show();
        }

        [ContextMenu("A")]
        void A()
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(0.5f);
                Show();
            }

            StartCoroutine(Coroutine());
        }

        [ContextMenu("B")]
        void B()
        {
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(0.5f);
                Hide();
            }

            StartCoroutine(Coroutine());
        }
    }
}
