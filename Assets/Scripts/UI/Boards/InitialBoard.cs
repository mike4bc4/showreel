using System;
using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Boards
{
    public class InitialBoard : Board, IBoard
    {
        public static readonly string StateID = Guid.NewGuid().ToString();

        [SerializeField] VisualTreeAsset m_InitialBoardVta;
        [SerializeField] VisualTreeAsset m_EmptyVta;
        [SerializeField] VisualTreeAsset m_BackgroundVta;
        [SerializeField] VisualTreeAsset m_TextVta;
        [SerializeField] VisualTreeAsset m_DiamondLineVta;
        [SerializeField] VisualTreeAsset m_SecondaryTextVta;
        [SerializeField] float m_ShineWidth;
        [SerializeField] Color m_ShineColor;

        Layer m_InitialLayer;
        Layer m_BackgroundLayer;
        Layer m_TextLayer;
        Layer m_DiamondLineLayer;
        Layer m_SecondaryTextLayer;
        Coroutine m_Coroutine;
        WaitForSeconds m_WaitOneSecond;
        WaitForSeconds m_WaitHalfSecond;
        AnimationDescriptor<float> m_BlurDefaultAnimDescriptor;
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

                AnimationManager.Animate(m_TextLayer.filter, m_BlurDefaultAnimDescriptor);
                AnimationManager.Animate(m_DiamondLineLayer.filter, m_BlurDefaultAnimDescriptor);
                m_SecondaryTextLayer.filter = new BlurFilter() { size = 0f };
                AnimationManager.Animate(m_SecondaryTextLayer.filter, m_BlurDefaultAnimDescriptor);

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

        DiamondTitle m_Title;
        Subtitle m_Subtitle;
        Layer m_SnapshotLayer;

        public Coroutine Show()
        {
            IEnumerator Coroutine()
            {
                bool createLayer = LayerManager.IsRemoved(m_InitialLayer);
                bool createSnapshotLayer = LayerManager.IsRemoved(this.m_SnapshotLayer);

                if (createLayer)
                {
                    m_InitialLayer = LayerManager.AddNewLayer(m_InitialBoardVta);
                    m_InitialLayer.filter = new MaskFilter();

                    m_Title = m_InitialLayer.rootVisualElement.Q<DiamondTitle>("title");
                    m_Subtitle = m_InitialLayer.rootVisualElement.Q<Subtitle>("subtitle");

                    m_Title.Fold(immediate: true);
                    yield return null;
                }

                var dynamicMask = new DynamicMask(m_InitialLayer.rootVisualElement.layout.size, invert: true);
                dynamicMask.dirtied += () => ((MaskFilter)m_InitialLayer.filter).alphaTexture = dynamicMask.texture;
                dynamicMask.AddElements(m_Title, m_Subtitle);

                if (createSnapshotLayer)
                {
                    m_SnapshotLayer = LayerManager.AddNewLayer(m_EmptyVta, "SnapshotLayer");
                    m_SnapshotLayer.filter = new BlurFilter();
                    m_SnapshotLayer.alpha = 0f;
                }

                // yield return null;  // Wait until title is folded.



                var titleSnapshot = m_InitialLayer.MakeSnapshot(m_Title);
                m_SnapshotLayer.rootVisualElement.Add(titleSnapshot);

                AnimationManager.Animate(m_SnapshotLayer, AnimationDescriptor.AlphaOne);
                yield return m_WaitHalfSecond;

                var anim1 = AnimationManager.Animate(m_SnapshotLayer.filter, AnimationDescriptor.BlurZero);
                yield return anim1.coroutine;

                dynamicMask.RemoveElements(m_Title);
                m_SnapshotLayer.rootVisualElement.Clear();

                m_Title.label.style.visibility = Visibility.Hidden;
                yield return m_Title.Unfold();

                dynamicMask.AddElements(m_Subtitle, m_Title.label);

                m_Title.label.style.visibility = StyleKeyword.Null;
                yield return null; // Wait until title label is visible;

                var titleLabelSnapshot = m_InitialLayer.MakeSnapshot(m_Title.label);
                m_SnapshotLayer.rootVisualElement.Add(titleLabelSnapshot);
                ((BlurFilter)m_SnapshotLayer.filter).size = BlurFilter.DefaultSize;
                m_SnapshotLayer.alpha = 0f;

                AnimationManager.Animate(m_SnapshotLayer, AnimationDescriptor.AlphaOne);
                yield return m_WaitHalfSecond;

                var anim2 = AnimationManager.Animate(m_SnapshotLayer.filter, AnimationDescriptor.BlurZero);
                yield return anim2.coroutine;

                dynamicMask.RemoveElements(m_Title.label);
                m_SnapshotLayer.rootVisualElement.Clear();
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
            return m_Coroutine;
        }

        void Start()
        {
            Show();
        }

        void Awake()
        {
            m_WaitOneSecond = new WaitForSeconds(1f);
            m_WaitHalfSecond = new WaitForSeconds(0.5f);
            m_BlurDefaultAnimDescriptor = new AnimationDescriptor<float>()
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
