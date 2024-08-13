using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CustomControls;
using Cysharp.Threading.Tasks;
using KeyframeSystem;
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
        public const int DisplaySortOrder = InterfaceBoard.DisplayOrder + 1000; // In front of InterfaceBoard as Layer
        public const int InputSortOrder = InterfaceBoard.SortingOrder + 1000; // In front of InterfaceBoard as UIDocument

        const string k_SingleButtonVariantButtonContainerUssClassName = "dialog__box__button-container--single-button";
        const float k_PopupScale = 0.95f;
        static readonly Color s_EffectLayerColor = Color.white * 0.7f;
        static readonly string s_EffectLayerName = $"EffectLayer({Guid.NewGuid().ToString("N")})";

        public event Action leftButtonClicked;
        public event Action rightButtonClicked;
        public event Action backgroundClicked;

        [SerializeField] VisualTreeAsset m_DialogBoxVta;

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
        KeyframeTrackPlayer m_ShowPlayer;
        KeyframeTrackPlayer m_HidePlayer;

        string m_TitleText;
        string m_LeftButtonLabelText;
        string m_RightButtonLabelText;
        VisualElement m_ContentContainer;

        public ButtonsDisplay buttonsDisplay
        {
            get => m_ButtonsDisplay;
            set
            {
                if (value != m_ButtonsDisplay)
                {
                    m_ButtonsDisplay = value;
                    ApplyButtonsDisplaySettings();
                }
            }
        }

        public string title
        {
            get => m_TitleText;
            set
            {
                m_TitleText = value;
                if (m_DialogBoxLayer != null && m_DialogBoxLayer.active)
                {
                    m_Title.text = m_TitleText;
                }
            }
        }

        public string leftButtonText
        {
            get => m_LeftButtonLabelText;
            set
            {
                m_LeftButtonLabelText = value;
                if (m_DialogBoxLayer != null && m_DialogBoxLayer.active)
                {
                    m_LeftButtonLabel.text = m_LeftButtonLabelText;
                }
            }
        }

        public string rightButtonText
        {
            get => m_RightButtonLabelText;
            set
            {
                m_RightButtonLabelText = value;
                if (m_DialogBoxLayer != null && m_DialogBoxLayer.active)
                {
                    m_RightButtonLabel.text = m_RightButtonLabelText;
                }
            }
        }

        public VisualElement contentContainer
        {
            get => m_ContentContainer;
        }

        public void Init()
        {
            m_TitleText = "Title";
            m_LeftButtonLabelText = "Confirm";
            m_RightButtonLabelText = "Cancel";
            m_ContentContainer = new VisualElement() { name = "dialog-content-container" };

            m_ShowPlayer = new KeyframeTrackPlayer();
            m_ShowPlayer.sampling = 60;

            m_ShowPlayer.AddEvent(0, () =>
            {
                m_DialogBoxLayer = LayerManager.CreateLayer(m_DialogBoxVta);
                m_DialogBoxLayer.alpha = 0f;
                m_DialogBoxLayer.displaySortOrder = DisplaySortOrder;
                m_DialogBoxLayer.inputSortOrder = InputSortOrder;
                m_DialogBoxLayer.interactable = false;
                m_DialogBoxLayer.blocksRaycasts = true;

                m_EffectLayer = LayerManager.CreateEffectLayer(s_EffectLayerName);
                m_EffectLayer.color = Color.white;
                m_EffectLayer.alpha = 1f;
                m_EffectLayer.displaySortOrder = DisplaySortOrder - 1;

                m_ScrollBox = m_DialogBoxLayer.rootVisualElement.Q<ScrollBox>("scroll-box");
                m_ScrollBox.Clear();
                m_ScrollBox.Add(contentContainer);

                m_Title = m_DialogBoxLayer.rootVisualElement.Q<DiamondTitle>("title");
                m_Title.visible = false;
                m_Title.label.visible = false;
                m_Title.animationProgress = 0f;
                m_Title.text = m_TitleText;

                m_LeftButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("left-button");
                m_LeftButton.RegisterCallback<ClickEvent>(evt => leftButtonClicked?.Invoke());

                m_LeftButtonLabel = m_LeftButton.Q<Label>("text");
                m_LeftButtonLabel.text = m_LeftButtonLabelText;

                m_RightButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("right-button");
                m_RightButton.RegisterCallback<ClickEvent>(evt => rightButtonClicked?.Invoke());

                m_RightButtonLabel = m_RightButton.Q<Label>("text");
                m_RightButtonLabel.text = m_RightButtonLabelText;

                m_DialogContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-container");
                m_DialogContainer.RegisterCallback<ClickEvent>(evt => backgroundClicked?.Invoke());

                m_BoxShadow = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-shadow");
                m_BoxShadow.style.scale = Vector2.one;
                m_ButtonContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("button-container");

                ApplyButtonsDisplaySettings();
            }, EventInvokeFlags.Forward);

            var t1 = m_ShowPlayer.AddKeyframeTrack((float t) => m_EffectLayer?.SetColor(Color.Lerp(Color.white, s_EffectLayerColor, t)));
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(30, 1f);

            var t2 = m_ShowPlayer.AddKeyframeTrack((float blur) => m_EffectLayer?.SetBlur(blur));
            t2.AddKeyframe(0, 0f);
            t2.AddKeyframe(30, Layer.DefaultBlur);

            var t3 = m_ShowPlayer.AddKeyframeTrack((float scaleMultiplier) =>
            {
                if (m_BoxShadow != null)
                {
                    m_BoxShadow.style.scale = Vector2.one * scaleMultiplier;
                }
            });
            t3.AddKeyframe(20, k_PopupScale);
            t3.AddKeyframe(35, 1f);

            var t4 = m_ShowPlayer.AddKeyframeTrack((float alpha) => m_DialogBoxLayer?.SetAlpha(alpha));
            t4.AddKeyframe(20, 0f);
            t4.AddKeyframe(35, 1f);

            m_ShowPlayer.AddEvent(35, () =>
            {
                m_DialogBoxLayer.interactable = true;
                var effectLayer = m_DialogBoxLayer.AddElementEffectLayer(m_Title);
                effectLayer.blurSize = Layer.DefaultBlur;
                effectLayer.overscan = new Overscan(8f);
                m_Title.style.opacity = 0f;
                m_Title.visible = true;
            }, EventInvokeFlags.Forward);

            var t5 = m_ShowPlayer.AddKeyframeTrack((float opacity) =>
            {
                if (m_Title != null)
                {
                    m_Title.style.opacity = opacity;
                }
            });
            t5.AddKeyframe(35, 0f);
            t5.AddKeyframe(55, 1f);

            var t6 = m_ShowPlayer.AddKeyframeTrack((float blur) => m_DialogBoxLayer?.GetElementEffectLayer(m_Title)?.SetBlurSize(blur));
            t6.AddKeyframe(45, Layer.DefaultBlur);
            t6.AddKeyframe(65, 0f);

            m_ShowPlayer.AddEvent(65, () =>
            {
                m_DialogBoxLayer.RemoveElementEffectLayer(m_Title);
            }, EventInvokeFlags.Forward);

            var t7 = m_ShowPlayer.AddKeyframeTrack((float progress) => m_Title?.SetAnimationProgress(progress));
            t7.AddKeyframe(65, 0f);
            t7.AddKeyframe(125, 1f);

            m_ShowPlayer.AddEvent(125, () =>
            {
                var effectLayer = m_DialogBoxLayer.AddElementEffectLayer(m_Title.label);
                effectLayer.blurSize = Layer.DefaultBlur;
                m_Title.label.style.opacity = 0f;
                m_Title.label.visible = true;
            }, EventInvokeFlags.Forward);

            var t8 = m_ShowPlayer.AddKeyframeTrack((float opacity) =>
            {
                if (m_Title?.label != null)
                {
                    m_Title.label.style.opacity = opacity;
                }
            });
            t8.AddKeyframe(125, 0f);
            t8.AddKeyframe(145, 1f);

            var t9 = m_ShowPlayer.AddKeyframeTrack((float blur) => m_DialogBoxLayer?.GetElementEffectLayer(m_Title.label)?.SetBlurSize(blur));
            t9.AddKeyframe(135, Layer.DefaultBlur);
            t9.AddKeyframe(155, 0f);

            m_ShowPlayer.AddEvent(155, () =>
            {
                m_DialogBoxLayer.RemoveElementEffectLayer(m_Title.label);
            }, EventInvokeFlags.Forward);

            m_HidePlayer = new KeyframeTrackPlayer();
            m_HidePlayer.sampling = 60;

            m_HidePlayer.AddEvent(0, () =>
            {
                m_ShowPlayer.Pause();
            }, EventInvokeFlags.Forward);

            var t10 = m_HidePlayer.AddKeyframeTrack((float alpha) => m_DialogBoxLayer?.SetAlpha(alpha));
            t10.AddKeyframe(0, 1f);
            t10.AddKeyframe(15, 0f);

            var t11 = m_HidePlayer.AddKeyframeTrack((float scaleMultiplier) =>
            {
                if (m_BoxShadow != null)
                {
                    m_BoxShadow.style.scale = Vector2.one * scaleMultiplier;
                }
            });
            t11.AddKeyframe(0, 1f);
            t11.AddKeyframe(15, k_PopupScale);

            var t12 = m_HidePlayer.AddKeyframeTrack((float t) => m_EffectLayer?.SetColor(Color.Lerp(Color.white, s_EffectLayerColor, t)));
            t12.AddKeyframe(5, 1f);
            t12.AddKeyframe(35, 0f);

            var t13 = m_HidePlayer.AddKeyframeTrack((float blur) => m_EffectLayer?.SetBlur(blur));
            t13.AddKeyframe(5, Layer.DefaultBlur);
            t13.AddKeyframe(35, 0f);

            m_HidePlayer.AddEvent(35, () =>
            {
                LayerManager.RemoveLayer(m_DialogBoxLayer);
                LayerManager.RemoveLayer(m_EffectLayer);
            }, EventInvokeFlags.Forward);
        }

        void ApplyButtonsDisplaySettings()
        {
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

        public void ShowImmediate()
        {

        }

        public void HideImmediate()
        {

        }

        public UniTask Show(CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Hide(CancellationToken cancellationToken = default)
        {
            return UniTask.CompletedTask;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                m_ShowPlayer.frameIndex = 0;
                m_ShowPlayer.playbackSpeed = 1f;
                m_ShowPlayer.Play();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                m_HidePlayer.frameIndex = 0;
                m_HidePlayer.playbackSpeed = 1f;
                m_HidePlayer.Play();
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
