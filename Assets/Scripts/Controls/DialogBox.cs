using System;
using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using Controls.Raw;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Layers;
using Templates;
using UnityEngine.AddressableAssets;

namespace Controls
{
    public class DialogBox : DisposableObject
    {
        const int k_DefaultDisplaySortOrder = 100;
        const string k_ShowHideAnimationName = "ShowHideAnimation";
        const string k_TitleAnimationName = "TitleAnimation";
        const float k_PopupScale = 0.95f;
        const string k_DialogBoxLabelQuitVariantUssClassName = "dialog-box__label--quit";
        static readonly Color s_BackgroundLayerColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        const string k_InfoDialogBoxContentVtaAddress = "InfoDialogBoxContentVisualTreeAsset";

        public event Action onHide;
        public event Action onRightButtonClicked;
        public event Action onLeftButtonClicked;
        public event Action onBackgroundClicked;

        Layer m_Layer;
        PostProcessingLayer m_BackgroundPostProcessingLayer;
        PostProcessingLayer m_PostProcessingLayer;
        Controls.Raw.DialogBox m_DialogBox;
        AnimationPlayer m_ShowHideAnimationPlayer;
        AnimationPlayer m_TitleAnimationPlayer;

        public VisualElement contentContainer
        {
            get => m_DialogBox.contentContainer;
        }

        public string rightButtonLabel
        {
            get => m_DialogBox.rightButtonLabel;
            set => m_DialogBox.rightButtonLabel = value;
        }

        public string leftButtonLabel
        {
            get => m_DialogBox.leftButtonLabel;
            set => m_DialogBox.leftButtonLabel = value;
        }

        public ButtonDisplay buttonDisplay
        {
            get => m_DialogBox.buttonDisplay;
            set => m_DialogBox.buttonDisplay = value;
        }

        public string titleLabel
        {
            get => m_DialogBox.titleLabel;
            set => m_DialogBox.titleLabel = value;
        }

        public int displaySortOrder
        {
            get => m_Layer.displaySortOrder;
            set
            {
                m_Layer.displaySortOrder = value;
                m_BackgroundPostProcessingLayer.displaySortOrder = value - 1;
                m_PostProcessingLayer.displaySortOrder = value + 1;
            }
        }

        public int inputSortOrder
        {
            get => m_Layer.inputSortOrder;
            set => m_Layer.inputSortOrder = value;
        }

        public DialogBox()
        {
            m_Layer = LayerManager.CreateLayer("DialogBox");
            m_BackgroundPostProcessingLayer = LayerManager.CreatePostProcessingLayer("DialogBoxBackground");
            m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer("DialogBox");
            displaySortOrder = k_DefaultDisplaySortOrder;

            m_DialogBox = new Controls.Raw.DialogBox();
            m_DialogBox.rightButton.clicked += () => onRightButtonClicked?.Invoke();
            m_DialogBox.leftButton.clicked += () => onLeftButtonClicked?.Invoke();
            m_DialogBox.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == m_DialogBox)
                {
                    onBackgroundClicked?.Invoke();
                }
            });

            m_Layer.rootVisualElement.Add(m_DialogBox);

            m_ShowHideAnimationPlayer = new AnimationPlayer();
            m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
            m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

            m_TitleAnimationPlayer = new AnimationPlayer();
            m_TitleAnimationPlayer.AddAnimation(CreateTitleAnimation(), k_TitleAnimationName);
            m_TitleAnimationPlayer.animation = m_TitleAnimationPlayer[k_TitleAnimationName];

            HideImmediate();
        }

        KeyframeAnimation CreateShowHideAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (m_ShowHideAnimationPlayer.playbackSpeed >= 0)
                {
                    m_BackgroundPostProcessingLayer.visible = true;
                    m_Layer.visible = true;
                    m_Layer.blocksRaycasts = true;
                    m_Layer.interactable = false;

                    m_DialogBox.title.animationProgress = 0f;
                    m_DialogBox.title.style.opacity = 0f;
                }
                else
                {
                    m_BackgroundPostProcessingLayer.visible = false;
                    m_Layer.visible = false;
                    m_Layer.blocksRaycasts = false;
                    m_Layer.interactable = false;

                    m_DialogBox.title.animationProgress = 1f;
                    m_DialogBox.title.style.opacity = 1f;

                    m_PostProcessingLayer.visible = false;
                    m_TitleAnimationPlayer.animationTime = 0f;

                    onHide?.Invoke();
                }
            });
            var t1 = animation.AddTrack((float t) => m_BackgroundPostProcessingLayer.tint = Color.Lerp(Color.white, s_BackgroundLayerColor, t));
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(30, 1f);

            var t2 = animation.AddTrack((float blurSize) => m_BackgroundPostProcessingLayer.blurSize = blurSize);
            t2.AddKeyframe(0, 0f);
            t2.AddKeyframe(30, PostProcessingLayer.DefaultBlurSize);

            var t3 = animation.AddTrack((float scaleMultiplier) => m_DialogBox.shadow.style.scale = Vector2.one * scaleMultiplier);
            t3.AddKeyframe(20, k_PopupScale);
            t3.AddKeyframe(35, 1f);

            var t4 = animation.AddTrack((float alpha) => m_Layer.alpha = alpha);
            t4.AddKeyframe(20, 0f);
            t4.AddKeyframe(35, 1f);
            animation.AddEvent(35, () =>
            {
                if (m_ShowHideAnimationPlayer.playbackSpeed >= 0)
                {
                    m_Layer.interactable = true;
                    m_TitleAnimationPlayer.Play();
                }
                else
                {
                    m_Layer.interactable = false;
                    m_TitleAnimationPlayer.Pause();
                }
            });

            return animation;
        }

        KeyframeAnimation CreateTitleAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (m_TitleAnimationPlayer.playbackSpeed >= 0)
                {
                    m_PostProcessingLayer.visible = true;
                    m_PostProcessingLayer.maskElement = m_DialogBox.title;
                    m_PostProcessingLayer.overscan = 8f;
                }
            });
            var t1 = animation.AddTrack((float opacity) => m_DialogBox.title.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack = animation.AddTrack((float blurSize) => m_PostProcessingLayer.blurSize = blurSize);
            blurTrack.AddKeyframe(10, BaseLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(30, 0f, Easing.StepOut);

            var t3 = animation.AddTrack((float animationProgress) => m_DialogBox.title.animationProgress = animationProgress);
            t3.AddKeyframe(30, 0f);
            t3.AddKeyframe(90, 1f);

            animation.AddEvent(90, () =>
            {
                if (m_TitleAnimationPlayer.playbackSpeed >= 0)
                {
                    m_PostProcessingLayer.maskElement = m_DialogBox.title.label;
                    m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
                }
            });
            var t4 = animation.AddTrack((float opacity) => m_DialogBox.title.label.style.opacity = opacity);
            t4.AddKeyframe(90, 0f);
            t4.AddKeyframe(110, 1f);

            blurTrack.AddKeyframe(90, BaseLayer.DefaultBlurSize);
            blurTrack.AddKeyframe(120, 0f);
            animation.AddEvent(120, () =>
            {
                if (m_TitleAnimationPlayer.playbackSpeed >= 0)
                {
                    m_PostProcessingLayer.visible = false;
                }
            });

            return animation;
        }

        public void HideImmediate()
        {
            m_BackgroundPostProcessingLayer.visible = false;
            m_Layer.visible = false;
            m_Layer.interactable = false;
            m_Layer.blocksRaycasts = false;
            m_PostProcessingLayer.visible = false;
        }

        public void Show()
        {
            m_ShowHideAnimationPlayer.playbackSpeed = 1f;
            m_ShowHideAnimationPlayer.Play();
        }

        public void Hide()
        {
            m_ShowHideAnimationPlayer.playbackSpeed = -1;
            m_ShowHideAnimationPlayer.Play();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            m_ShowHideAnimationPlayer.Stop();
            m_TitleAnimationPlayer.Stop();

            LayerManager.RemoveLayer(m_Layer);
            LayerManager.RemoveLayer(m_BackgroundPostProcessingLayer);
            LayerManager.RemoveLayer(m_PostProcessingLayer);

            m_Disposed = true;
        }

        public static DialogBox CreateQuitDialogBox()
        {
            var dialogBox = new DialogBox();

            dialogBox.titleLabel = "Quit?";
            dialogBox.rightButtonLabel = "Cancel";
            dialogBox.leftButtonLabel = "Yes";

            var label = new Label();
            label.text = "Do you want to close application?";
            label.AddToClassList(k_DialogBoxLabelQuitVariantUssClassName);
            dialogBox.contentContainer.Add(label);

            return dialogBox;
        }

        public static DialogBox CreateInfoDialogBox()
        {
            var dialogBox = new DialogBox();

            dialogBox.titleLabel = "Info";
            dialogBox.buttonDisplay = ButtonDisplay.LeftCenter;
            dialogBox.leftButtonLabel = "OK";

            var handle = Addressables.LoadAssetAsync<VisualTreeAsset>(k_InfoDialogBoxContentVtaAddress);
            var contentVisualTreeAsset = handle.WaitForCompletion();
            dialogBox.contentContainer.Add(contentVisualTreeAsset.Instantiate());

            return dialogBox;
        }
    }
}
