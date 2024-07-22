using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace UI.Boards
{
    public enum ButtonsDisplay
    {
        Both,
        Left,
    }

    public class DialogBoard : Board, IBoard
    {
        public const int DisplayOrder = InterfaceBoard.DisplayOrder + 1000; // In front of InterfaceBoard as Layer
        public const int SortingOrder = InterfaceBoard.SortingOrder + 1000; // In front of InterfaceBoard as UIDocument

        const string k_SingleButtonVariantButtonContainerUssClassName = "dialog__box__button-container--single-button";

        public event Action leftButtonClicked;
        public event Action rightButtonClicked;
        public event Action backgroundClicked;

        [SerializeField] VisualTreeAsset m_DialogBoxVta;
        [SerializeField] float m_PopupTime;
        [SerializeField] float m_FadeTime;
        [SerializeField] float m_PopupScale;
        [SerializeField] Color m_EffectLayerColor;

        Layer m_DialogBoxLayer;
        EffectLayer m_EffectLayer;
        ScrollBox m_ScrollBox;
        DiamondTitle m_Title;
        Button m_LeftButton;
        Label m_LeftButtonLabel;
        Button m_RightButton;
        Label m_RightButtonLabel;
        VisualElement m_DialogContainer;
        VisualElement m_BoxShadow;
        VisualElement m_ButtonContainer;
        ButtonsDisplay m_ButtonsDisplay;
        CancellationTokenSource m_Cts;
        TaskStatus m_Status;
        int m_StateIndex;
        TaskPool m_HideTaskPool;
        TaskPool m_ShowTaskPool;

        public ButtonsDisplay buttonsDisplay
        {
            get => m_ButtonsDisplay;
            set
            {
                if (value != m_ButtonsDisplay)
                {
                    m_ButtonsDisplay = value;
                    switch (m_ButtonsDisplay)
                    {
                        case ButtonsDisplay.Both:
                            m_ButtonContainer.RemoveFromClassList(k_SingleButtonVariantButtonContainerUssClassName);
                            m_RightButton.SetEnabled(true);
                            break;
                        case ButtonsDisplay.Left:
                            m_ButtonContainer.AddToClassList(k_SingleButtonVariantButtonContainerUssClassName);
                            m_RightButton.SetEnabled(false);
                            break;
                    }
                }
            }
        }

        public string title
        {
            get => m_Title.text;
            set => m_Title.text = value;
        }

        public string leftButtonText
        {
            get => m_LeftButtonLabel.text;
            set => m_LeftButtonLabel.text = value;
        }

        public string rightButtonText
        {
            get => m_RightButtonLabel.text;
            set => m_RightButtonLabel.text = value;
        }

        public VisualElement contentContainer
        {
            get => m_ScrollBox;
        }

        CancellationToken token
        {
            get => m_Cts.Token;
        }

        public void Init()
        {
            m_HideTaskPool = new TaskPool();
            m_ShowTaskPool = new TaskPool();

            m_DialogBoxLayer = LayerManager.CreateLayer(m_DialogBoxVta);
            m_DialogBoxLayer.alpha = 0f;
            m_DialogBoxLayer.displayOrder = DisplayOrder;
            m_DialogBoxLayer.panelSortingOrder = SortingOrder;
            m_DialogBoxLayer.interactable = false;
            m_DialogBoxLayer.blocksRaycasts = false;

            m_EffectLayer = LayerManager.CreateEffectLayer("DialogBackgroundBlur");
            m_EffectLayer.color = Color.white;
            m_EffectLayer.alpha = 0f;
            m_EffectLayer.displayOrder = DisplayOrder - 1;

            m_ScrollBox = m_DialogBoxLayer.rootVisualElement.Q<ScrollBox>("scroll-box");
            m_Title = m_DialogBoxLayer.rootVisualElement.Q<DiamondTitle>("title");

            m_LeftButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("left-button");
            m_LeftButton.RegisterCallback<ClickEvent>(evt =>
            {
                leftButtonClicked?.Invoke();
            });

            m_LeftButtonLabel = m_LeftButton.Q<Label>("text");

            m_RightButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("right-button");
            m_RightButton.RegisterCallback<ClickEvent>(evt =>
            {
                rightButtonClicked?.Invoke();
            });

            m_RightButtonLabel = m_LeftButton.Q<Label>("text");

            m_DialogContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-container");
            m_DialogContainer.RegisterCallback<ClickEvent>(evt =>
            {
                backgroundClicked?.Invoke();
            });

            m_BoxShadow = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-shadow");
            m_BoxShadow.style.scale = Vector2.one * m_PopupScale;
            m_ButtonContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("button-container");

            m_ShowTaskPool.Add(() =>
            {
                m_EffectLayer.alpha = 1f;
                m_DialogBoxLayer.blocksRaycasts = true;
                m_StateIndex++;
            });

            m_HideTaskPool.Add(() =>
            {
                m_EffectLayer.alpha = 0f;
                m_DialogBoxLayer.blocksRaycasts = false;
            });

            m_ShowTaskPool.Add(async () =>
            {
                var anim1 = AnimationManager.Animate(m_EffectLayer, AnimationDescriptor.BlurDefault);
                anim1.time = m_FadeTime;

                var anim2 = AnimationManager.Animate(m_EffectLayer, nameof(LayerBase.color), m_EffectLayerColor);
                anim2.time = m_FadeTime;

                await (anim1.AsTask(token), anim2.AsTask(token));
                m_StateIndex++;
            });

            m_HideTaskPool.Add(async () =>
            {
                var anim1 = AnimationManager.Animate(m_EffectLayer, AnimationDescriptor.BlurZero);
                anim1.time = m_FadeTime;

                var anim2 = AnimationManager.Animate(m_EffectLayer, nameof(LayerBase.color), Color.white);
                anim2.time = m_FadeTime;

                await (anim1.AsTask(token), anim2.AsTask(token));
                m_StateIndex--;
            });

            m_ShowTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_DialogBoxLayer, AnimationDescriptor.AlphaOne);
                animation.time = m_PopupTime;

                m_BoxShadow.style.AddTransition("scale", m_PopupTime, EasingMode.EaseOutCubic);
                m_BoxShadow.style.scale = Vector2.one;
                var task = UniTask.WaitUntil(() => m_BoxShadow.resolvedStyle.scale != Vector2.one, cancellationToken: token);

                await (animation.AsTask(token), task);
                m_StateIndex++;
            });

            m_HideTaskPool.Add(async () =>
            {
                var animation = AnimationManager.Animate(m_DialogBoxLayer, AnimationDescriptor.AlphaZero);
                animation.time = m_PopupTime;

                m_BoxShadow.style.AddTransition("scale", m_PopupTime, EasingMode.EaseInCubic);
                m_BoxShadow.style.scale = Vector2.one * m_PopupScale;
                var task = UniTask.WaitUntil(() => m_BoxShadow.resolvedStyle.scale != Vector2.one * m_PopupScale, cancellationToken: token);

                await (animation.AsTask(token), task);
                m_StateIndex--;
            });

            m_ShowTaskPool.Add(() =>
            {
                m_DialogBoxLayer.interactable = true;
            });


            m_HideTaskPool.Add(() =>
            {
                m_DialogBoxLayer.interactable = false;
                m_StateIndex--;
            });
        }

        void Stop()
        {
            if (m_Cts != null)
            {
                m_Cts.Cancel();
                m_Cts.Dispose();
                m_Cts = null;
            }
        }

        public void ShowImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = m_ShowTaskPool.length - 1;

                m_BoxShadow.style.RemoveTransition("scale");
                m_BoxShadow.style.scale = Vector2.one;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);

                m_EffectLayer.alpha = 1f;
                m_EffectLayer.blurSize = EffectLayer.DefaultBlurSize;
                m_EffectLayer.color = m_EffectLayerColor;

                m_DialogBoxLayer.alpha = 1f;
                m_DialogBoxLayer.interactable = true;
                m_DialogBoxLayer.blocksRaycasts = true;

                m_Status.SetCompleted();
            })();
        }

        public void HideImmediate()
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            UniTask.Action(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                m_StateIndex = 0;

                m_BoxShadow.style.RemoveTransition("scale");
                m_BoxShadow.style.scale = Vector2.one * m_PopupScale;
                await UniTask.NextFrame(PlayerLoopTiming.Initialization);

                m_EffectLayer.alpha = 0f;
                m_EffectLayer.blurSize = 0f;
                m_EffectLayer.color = Color.white;

                m_DialogBoxLayer.alpha = 0f;
                m_DialogBoxLayer.interactable = false;
                m_DialogBoxLayer.blocksRaycasts = false;

                m_Status.SetCompleted();
            })();
        }

        public UniTask Show(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                try
                {
                    await UniTask.NextFrame(m_Cts.Token).Chain(m_ShowTaskPool.GetRange(m_StateIndex, m_ShowTaskPool.length - m_StateIndex));
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });

            return task;
        }

        public UniTask Hide(CancellationToken cancellationToken = default)
        {
            Stop();
            m_Cts = cancellationToken != default ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : new CancellationTokenSource();
            var task = UniTask.Create(async () =>
            {
                if (!m_Status.IsCompleted())
                {
                    await UniTask.WaitUntil(() => m_Status.IsCompleted(), cancellationToken: token);
                }

                m_Status.SetPending();
                var functions = m_HideTaskPool.GetRange(0, m_StateIndex + 1);
                functions.Reverse();

                try
                {
                    await UniTask.NextFrame(m_Cts.Token).Chain(functions);
                }
                finally
                {
                    m_Status.SetCompleted();
                }
            });

            return task;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Show(default(CancellationToken));
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Hide(default(CancellationToken));
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                ShowImmediate();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                HideImmediate();
            }
        }
    }
}
