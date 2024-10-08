using System;
using System.Collections;
using System.Collections.Generic;
using KeyframeSystem;
using Controls.Raw;
using UnityEngine;
using UnityEngine.UIElements;
using Layers;
using Templates;
using Localization;
using UI;

namespace Controls
{
    public class DialogBox : DisposableObject
    {
        const int k_DefaultDisplaySortOrder = 100;
        const int k_DefaultInputSortOrder = 100;
        const string k_ShowHideAnimationName = "ShowHideAnimation";
        const string k_TitleAnimationName = "TitleAnimation";
        const float k_PopupScale = 0.95f;
        const string k_DialogBoxLabelQuitVariantUssClassName = "dialog-box__label--quit";
        static readonly Color s_BackgroundLayerColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        const string k_InfoDialogBoxContentVtaAddress = "InfoDialogBoxContentVisualTreeAsset";
        const string k_WelcomeDialogBoxContentVtaAddress = "WelcomeDialogBoxContentVisualTreeAsset";

        public enum ButtonIndex
        {
            Left,
            Right,
            Background,
        }

        public enum Status
        {
            Initial,
            Hiding,
            Hidden,
            Showing,
            Shown,
        }

        public event Action<Status> onStatusChanged;

        GroupLayer m_GroupLayer;
        UILayer m_UILayer;
        PostProcessingLayer m_BackgroundPostProcessingLayer;
        PostProcessingLayer m_PostProcessingLayer;
        Controls.Raw.DialogBox m_DialogBox;
        AnimationPlayer m_ShowHideAnimationPlayer;
        AnimationPlayer m_TitleAnimationPlayer;
        List<Action> m_ClickDelegates = new List<Action>();
        Status m_Status;
        Action m_HideCompletedCallback;
        Action m_ShowCompletedCallback;

        public bool isHidden => status == Status.Hidden;
        public bool isHiding => status == Status.Hiding;
        public bool isShown => status == Status.Shown;
        public bool isShowing => status == Status.Showing;

        public Control rootVisualElement
        {
            get => m_DialogBox;
        }

        public Status status
        {
            get => m_Status;
            protected set
            {
                var previousStatus = m_Status;
                if (value != previousStatus)
                {
                    m_Status = value;
                    onStatusChanged?.Invoke(previousStatus);
                }
            }
        }

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
            get => m_GroupLayer.displaySortOrder;
            set
            {
                m_GroupLayer.displaySortOrder = value;
                m_BackgroundPostProcessingLayer.displaySortOrder = value - 1;
            }
        }

        public int inputSortOrder
        {
            get => m_UILayer.inputSortOrder;
            set => m_UILayer.inputSortOrder = value;
        }

        public LocalizationAddress titleLocalizationAddress
        {
            get => m_DialogBox.titleLocalizationAddress;
            set => m_DialogBox.titleLocalizationAddress = value;
        }

        public LocalizationAddress leftButtonLocalizationAddress
        {
            get => m_DialogBox.leftButtonLocalizationAddress;
            set => m_DialogBox.leftButtonLocalizationAddress = value;
        }

        public LocalizationAddress rightButtonLocalizationAddress
        {
            get => m_DialogBox.rightButtonLocalizationAddress;
            set => m_DialogBox.rightButtonLocalizationAddress = value;
        }

        public DialogBox()
        {
            m_ClickDelegates = new List<Action>();
            for (int i = 0; i < Enum.GetNames(typeof(ButtonIndex)).Length; i++)
            {
                m_ClickDelegates.Add(delegate { });
            }

            m_GroupLayer = LayerManager.CreateGroupLayer("DialogBox");
            m_BackgroundPostProcessingLayer = LayerManager.CreatePostProcessingLayer("DialogBoxBackground");
            displaySortOrder = k_DefaultDisplaySortOrder;

            m_UILayer = LayerManager.CreateUILayer("DialogBox");
            m_UILayer.displaySortOrder = k_DefaultDisplaySortOrder;
            m_GroupLayer.Add(m_UILayer);
            inputSortOrder = k_DefaultInputSortOrder;

            m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer("DialogBox");
            m_PostProcessingLayer.displaySortOrder = k_DefaultDisplaySortOrder + 1;
            m_GroupLayer.Add(m_PostProcessingLayer);

            m_DialogBox = new Controls.Raw.DialogBox();
            m_DialogBox.rightButton.clicked += () => PerformClick(ButtonIndex.Right);
            m_DialogBox.leftButton.clicked += () => PerformClick(ButtonIndex.Left);
            m_DialogBox.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == m_DialogBox)
                {
                    PerformClick(ButtonIndex.Background);
                }
            });

            m_UILayer.rootVisualElement.Add(m_DialogBox);

            m_ShowHideAnimationPlayer = new AnimationPlayer();
            m_ShowHideAnimationPlayer.AddAnimation(CreateShowHideAnimation(), k_ShowHideAnimationName);
            m_ShowHideAnimationPlayer.animation = m_ShowHideAnimationPlayer[k_ShowHideAnimationName];

            m_TitleAnimationPlayer = new AnimationPlayer();
            m_TitleAnimationPlayer.AddAnimation(CreateTitleAnimation(), k_TitleAnimationName);
            m_TitleAnimationPlayer.animation = m_TitleAnimationPlayer[k_TitleAnimationName];

            HideImmediateInternal();
        }

        void HideImmediateInternal()
        {
            m_BackgroundPostProcessingLayer.visible = false;
            m_UILayer.visible = false;
            m_UILayer.interactable = false;
            m_UILayer.blocksRaycasts = false;
            m_PostProcessingLayer.visible = false;
        }

        public void HideImmediate()
        {
            if (isHidden)
            {
                return;
            }

            HideImmediateInternal();
            status = Status.Hidden;
        }

        public void Show(Action onCompleted = null)
        {
            m_ShowCompletedCallback += onCompleted;
            if (isShown)
            {
                return;
            }

            m_ShowHideAnimationPlayer.playbackSpeed = 1f;
            m_ShowHideAnimationPlayer.Play();
            status = Status.Showing;
        }

        public void Hide(Action onCompleted = null)
        {
            m_HideCompletedCallback += onCompleted;
            if (isHidden)
            {
                return;
            }

            m_ShowHideAnimationPlayer.playbackSpeed = -1;
            m_ShowHideAnimationPlayer.Play();
            status = Status.Hiding;
        }

        public void RegisterClickCallback(ButtonIndex buttonIndex, Action callback)
        {
            m_ClickDelegates[(int)buttonIndex] += callback;
        }

        public void UnregisterClickCallback(ButtonIndex buttonIndex, Action callback)
        {
            m_ClickDelegates[(int)buttonIndex] -= callback;
        }

        public void PerformClick(ButtonIndex buttonIndex)
        {
            m_ClickDelegates[(int)buttonIndex].Invoke();
        }

        KeyframeAnimation CreateShowHideAnimation()
        {
            var animation = new KeyframeAnimation();

            animation.AddEvent(0, () =>
            {
                if (m_ShowHideAnimationPlayer.playbackSpeed >= 0)
                {
                    m_BackgroundPostProcessingLayer.visible = true;
                    m_UILayer.visible = true;
                    m_UILayer.blocksRaycasts = true;
                    m_UILayer.interactable = false;

                    m_DialogBox.title.animationProgress = 0f;
                    m_DialogBox.title.style.opacity = 0f;
                }
                else
                {
                    m_BackgroundPostProcessingLayer.visible = false;
                    m_UILayer.visible = false;
                    m_UILayer.blocksRaycasts = false;
                    m_UILayer.interactable = false;

                    m_DialogBox.title.animationProgress = 1f;
                    m_DialogBox.title.style.opacity = 1f;

                    m_PostProcessingLayer.visible = false;
                    m_TitleAnimationPlayer.animationTime = 0f;

                    status = Status.Hidden;
                    m_HideCompletedCallback?.Invoke();
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

            var t4 = animation.AddTrack((float alpha) => m_UILayer.alpha = alpha);
            t4.AddKeyframe(20, 0f);
            t4.AddKeyframe(35, 1f);
            animation.AddEvent(35, () =>
            {
                if (m_ShowHideAnimationPlayer.playbackSpeed >= 0)
                {
                    m_UILayer.interactable = true;
                    m_TitleAnimationPlayer.Play();
                    status = Status.Shown;
                    m_ShowCompletedCallback?.Invoke();
                }
                else
                {
                    m_UILayer.interactable = false;
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
                    m_PostProcessingLayer.overscan = Overscan.FromReferenceResolution(8f);
                }
            });
            var t1 = animation.AddTrack((float opacity) => m_DialogBox.title.style.opacity = opacity);
            t1.AddKeyframe(0, 0f);
            t1.AddKeyframe(20, 1f);

            var blurTrack = animation.AddTrack((float blurSize) => m_PostProcessingLayer.blurSize = blurSize);
            blurTrack.AddKeyframe(10, Layer.DefaultBlurSize);
            blurTrack.AddKeyframe(30, 0f, Easing.StepOut);

            var t3 = animation.AddTrack((float animationProgress) => m_DialogBox.title.animationProgress = animationProgress);
            t3.AddKeyframe(30, 0f);
            t3.AddKeyframe(90, 1f);

            animation.AddEvent(90, () =>
            {
                if (m_TitleAnimationPlayer.playbackSpeed >= 0)
                {
                    m_PostProcessingLayer.maskElement = m_DialogBox.title.label;
                    m_PostProcessingLayer.overscan = Overscan.FromReferenceResolution(8, 8, 0, 8);
                }
            });
            var t4 = animation.AddTrack((float opacity) => m_DialogBox.title.label.style.opacity = opacity);
            t4.AddKeyframe(90, 0f);
            t4.AddKeyframe(110, 1f);

            blurTrack.AddKeyframe(90, Layer.DefaultBlurSize);
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

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            m_ShowHideAnimationPlayer.Stop();
            m_TitleAnimationPlayer.Stop();

            LayerManager.RemoveLayer(m_UILayer);
            LayerManager.RemoveLayer(m_BackgroundPostProcessingLayer);
            LayerManager.RemoveLayer(m_PostProcessingLayer);
            LayerManager.RemoveLayer(m_GroupLayer);

            m_Disposed = true;
        }

        public static DialogBox CreateQuitDialogBox()
        {
            var dialogBox = new DialogBox();

            dialogBox.titleLocalizationAddress = "Table:QuitQuestion";
            dialogBox.rightButtonLocalizationAddress = "Table:Cancel";
            dialogBox.leftButtonLocalizationAddress = "Table:Yes";

            var label = new LocalizedLabel();
            label.localizationAddress = "Table:DoYouWantToCloseQuestion";
            label.AddToClassList(k_DialogBoxLabelQuitVariantUssClassName);
            dialogBox.contentContainer.Add(label);
            LocalizationManager.LocalizeVisualTree(dialogBox.rootVisualElement);
            return dialogBox;
        }

        public static DialogBox CreateInfoDialogBox()
        {
            var dialogBox = new DialogBox();

            dialogBox.titleLocalizationAddress = new LocalizationAddress("Table", "Info");
            dialogBox.buttonDisplay = ButtonDisplay.LeftCenter;
            dialogBox.leftButtonLabel = "OK";
            dialogBox.contentContainer.Add(DialogBoxResources.GetContentVisualTreeAsset("InfoDialogBoxContent").Instantiate());
            LocalizationManager.LocalizeVisualTree(dialogBox.rootVisualElement);
            return dialogBox;
        }

        public static DialogBox CreateWelcomeDialogBox()
        {
            var dialogBox = new DialogBox();

            dialogBox.titleLocalizationAddress = new LocalizationAddress("Table", "Welcome");
            dialogBox.buttonDisplay = ButtonDisplay.LeftCenter;
            dialogBox.leftButtonLocalizationAddress = new LocalizationAddress("Table", "Continue");
            dialogBox.contentContainer.Add(DialogBoxResources.GetContentVisualTreeAsset("WelcomeDialogBoxContent").Instantiate());
            LocalizationManager.LocalizeVisualTree(dialogBox.rootVisualElement);
            return dialogBox;
        }

        public static DialogBox CreateSettingsDialogBox()
        {
            var dialogBox = new DialogBox();
            dialogBox.titleLocalizationAddress = "Table:Settings";
            dialogBox.leftButtonLocalizationAddress = "Table:Save";
            dialogBox.rightButtonLocalizationAddress = "Table:Cancel";
            dialogBox.contentContainer.Add(DialogBoxResources.GetContentVisualTreeAsset("SettingsDialogBoxContent").Instantiate());
            LocalizationManager.LocalizeVisualTree(dialogBox.rootVisualElement);
            return dialogBox;
        }

        public static DialogBox CreateFinalDialogBox()
        {
            var dialogBox = new DialogBox();
            dialogBox.titleLocalizationAddress = "Table:TheEnd";
            dialogBox.leftButtonLocalizationAddress = "Table:Quit";
            dialogBox.rightButtonLocalizationAddress = "Table:Stay";
            dialogBox.contentContainer.Add(DialogBoxResources.GetContentVisualTreeAsset("FinalDialogBoxContent").Instantiate());
            LocalizationManager.LocalizeVisualTree(dialogBox.rootVisualElement);
            return dialogBox;
        }
    }
}
