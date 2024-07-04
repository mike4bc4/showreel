using System;
using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Boards
{
    public enum ButtonsDisplay
    {
        Both,
        Left,
    }

    public class DialogBoard : Board, IBoard
    {
        public const int DisplayOrder = InterfaceBoard.DisplayOrder + 1000;
        public const int SortingOrder = InterfaceBoard.SortingOrder + 1000;

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
        Coroutine m_Coroutine;
        ButtonsDisplay m_ButtonsDisplay;

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

        public void Init()
        {
            m_DialogBoxLayer = LayerManager.AddNewLayer(m_DialogBoxVta);
            m_DialogBoxLayer.alpha = 0f;
            m_DialogBoxLayer.displayOrder = DisplayOrder;
            m_DialogBoxLayer.panelSortingOrder = -1;
            m_DialogBoxLayer.receivesInput = false;

            m_EffectLayer = LayerManager.AddNewEffectLayer();
            m_EffectLayer.effect = new BlurEffect() { size = 0f };
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
        }

        void StopAnimations()
        {
            AnimationManager.StopAnimation(m_EffectLayer, nameof(Layer.alpha));
            AnimationManager.StopAnimation(m_EffectLayer.effect, nameof(BlurEffect.size));
            AnimationManager.StopAnimation(m_DialogBoxLayer, nameof(Layer.alpha));

            m_BoxShadow.style.scale = m_BoxShadow.resolvedStyle.scale;
            m_BoxShadow.style.RemoveTransition("scale");
        }

        public Coroutine Show()
        {
            StopAnimations();
            IEnumerator Coroutine()
            {
                var blur = ((BlurEffect)m_EffectLayer.effect).size != BlurEffect.DefaultSize;
                var changeColor = m_EffectLayer.color != m_EffectLayerColor;
                var showDialogBox = m_DialogBoxLayer.alpha != 1f;
                var scaleDialogBox = m_BoxShadow.resolvedStyle.scale != Vector2.one;
                var coroutines = new List<Coroutine>();

                m_EffectLayer.alpha = 1f;

                if (blur)
                {
                    var animation = AnimationManager.Animate(m_EffectLayer.effect, AnimationDescriptor.BlurDefault);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                if (changeColor)
                {
                    var animation = AnimationManager.Animate(m_EffectLayer, nameof(EffectLayer.color), m_EffectLayerColor);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                if (blur || changeColor)
                {
                    yield return new WaitForSeconds(m_PopupTime * 0.5f);
                }

                if (showDialogBox)
                {
                    var animation = AnimationManager.Animate(m_DialogBoxLayer, AnimationDescriptor.AlphaOne);
                    animation.time = m_PopupTime;
                    coroutines.Add(animation.coroutine);
                }

                if (scaleDialogBox)
                {
                    m_BoxShadow.style.AddTransition("scale", m_PopupTime, EasingMode.EaseOutCubic);
                    m_BoxShadow.style.scale = Vector2.one;
                    while (m_BoxShadow.resolvedStyle.scale != Vector2.one)
                    {
                        yield return null;
                    }
                }

                // Wait for all animations to finish before exit.
                foreach (var coroutine in coroutines)
                {
                    yield return coroutine;
                }

                m_DialogBoxLayer.panelSortingOrder = SortingOrder;
                m_DialogBoxLayer.receivesInput = true;
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
                var hideDialogBox = m_DialogBoxLayer.alpha != 0f;
                var scaleDialogBox = m_BoxShadow.resolvedStyle.scale != Vector2.one * m_PopupScale;
                var changeColor = m_EffectLayer.color != Color.white;
                var focus = ((BlurEffect)m_EffectLayer.effect).size != 0f;
                var coroutines = new List<Coroutine>();

                if (hideDialogBox)
                {
                    var animation = AnimationManager.Animate(m_DialogBoxLayer, AnimationDescriptor.AlphaZero);
                    animation.time = m_PopupTime;
                    coroutines.Add(animation.coroutine);
                }

                if (scaleDialogBox)
                {
                    m_BoxShadow.style.AddTransition("scale", m_PopupTime, EasingMode.EaseInCubic);
                    yield return null; // Have to wait because transition would be removed (by StopAnimations) and added in the same frame.
                    m_BoxShadow.style.scale = Vector2.one * m_PopupScale;
                }

                if (hideDialogBox || scaleDialogBox)
                {
                    yield return new WaitForSeconds(m_PopupTime * 0.5f);
                }

                if (changeColor)
                {
                    var animation = AnimationManager.Animate(m_EffectLayer, nameof(EffectLayer.color), Color.white);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                if (focus)
                {
                    var animation = AnimationManager.Animate(m_EffectLayer.effect, AnimationDescriptor.BlurZero);
                    animation.time = m_FadeTime;
                    coroutines.Add(animation.coroutine);
                }

                if (scaleDialogBox)
                {
                    while (m_BoxShadow.resolvedStyle.scale != Vector2.one * m_PopupScale)
                    {
                        yield return null;
                    }
                }

                foreach (var coroutine in coroutines)
                {
                    yield return coroutine;
                }

                m_EffectLayer.alpha = 0f;
            }

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
            }

            m_Coroutine = StartCoroutine(Coroutine());
            return m_Coroutine;
        }
    }
}
