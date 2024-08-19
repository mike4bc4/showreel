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
        // public const int DisplaySortOrder = InterfaceBoard.DisplaySortOrder + 1000; // In front of InterfaceBoard as Layer
        // public const int InputSortOrder = InterfaceBoard.InputSortOrder + 1000; // In front of InterfaceBoard as UIDocument

        // const string k_SingleButtonVariantButtonContainerUssClassName = "dialog__box__button-container--single-button";
        // const float k_PopupScale = 0.95f;
        // static readonly Color s_EffectLayerColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        // static readonly string s_EffectLayerName = $"EffectLayer({Guid.NewGuid().ToString("N")})";
        // const string k_MainAnimationName = "MainAnimation";
        // const string k_TitleAnimationName = "TitleAnimation";

        // public event Action leftButtonClicked;
        // public event Action rightButtonClicked;
        // public event Action backgroundClicked;

        // [SerializeField] VisualTreeAsset m_DialogBoxVta;

        // Layer m_DialogBoxLayer;
        // PostProcessingLayer m_BackgroundPostProcessingLayer;
        // PostProcessingLayer m_PostProcessingLayer;
        // ScrollBox m_ScrollBox;
        // DiamondTitle m_Title;
        // Button m_LeftButton;
        // Label m_LeftButtonLabel;
        // Button m_RightButton;
        // Label m_RightButtonLabel;
        // VisualElement m_DialogContainer;
        // VisualElement m_BoxShadow;
        // VisualElement m_ButtonContainer;
        // ButtonsDisplay m_ButtonsDisplay;
        // AnimationPlayer m_Player;
        // AnimationPlayer m_HidePlayer;
        // AnimationPlayer m_TitlePlayer;

        // string m_TitleText;
        // string m_LeftButtonLabelText;
        // string m_RightButtonLabelText;
        // VisualElement m_ContentContainer;

        // public ButtonsDisplay buttonsDisplay
        // {
        //     get => m_ButtonsDisplay;
        //     set
        //     {
        //         if (value != m_ButtonsDisplay)
        //         {
        //             m_ButtonsDisplay = value;
        //             ApplyButtonsDisplaySettings();
        //         }
        //     }
        // }

        // public string title
        // {
        //     get => m_TitleText;
        //     set
        //     {
        //         m_TitleText = value;
        //         if (m_DialogBoxLayer != null)
        //         {
        //             m_Title.text = m_TitleText;
        //         }
        //     }
        // }

        // public string leftButtonText
        // {
        //     get => m_LeftButtonLabelText;
        //     set
        //     {
        //         m_LeftButtonLabelText = value;
        //         if (m_DialogBoxLayer != null)
        //         {
        //             m_LeftButtonLabel.text = m_LeftButtonLabelText;
        //         }
        //     }
        // }

        // public string rightButtonText
        // {
        //     get => m_RightButtonLabelText;
        //     set
        //     {
        //         m_RightButtonLabelText = value;
        //         if (m_DialogBoxLayer != null)
        //         {
        //             m_RightButtonLabel.text = m_RightButtonLabelText;
        //         }
        //     }
        // }

        // public VisualElement contentContainer
        // {
        //     get => m_ContentContainer;
        // }

        // public void Init()
        // {
        //     m_TitleText = "Title";
        //     m_LeftButtonLabelText = "Confirm";
        //     m_RightButtonLabelText = "Cancel";
        //     m_ContentContainer = new VisualElement() { name = "dialog-content-container" };

        //     m_Player = new AnimationPlayer();
        //     m_Player.AddAnimation(CreateMainAnimation(), k_MainAnimationName);
        //     m_Player.animation = m_Player[k_MainAnimationName];

        //     m_TitlePlayer = new AnimationPlayer();
        //     m_TitlePlayer.AddAnimation(CreateTitleAnimation(), k_TitleAnimationName);
        //     m_TitlePlayer.animation = m_TitlePlayer[k_TitleAnimationName];
        // }

        // KeyframeAnimation CreateMainAnimation()
        // {
        //     var animation = new KeyframeAnimation();
        //     animation.AddEvent(0, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_DialogBoxLayer = LayerManager.CreateLayer(m_DialogBoxVta, displaySortOrder: DisplaySortOrder);
        //             m_DialogBoxLayer.alpha = 0f;
        //             m_DialogBoxLayer.inputSortOrder = InputSortOrder;
        //             m_DialogBoxLayer.interactable = false;
        //             m_DialogBoxLayer.blocksRaycasts = true;

        //             m_BackgroundPostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: DisplaySortOrder - 1);

        //             m_ScrollBox = m_DialogBoxLayer.rootVisualElement.Q<ScrollBox>("scroll-box");
        //             m_ScrollBox.Clear();
        //             m_ScrollBox.Add(contentContainer);

        //             m_Title = m_DialogBoxLayer.rootVisualElement.Q<DiamondTitle>("title");
        //             m_Title.style.opacity = 0f;
        //             m_Title.label.style.opacity = 0f;
        //             m_Title.animationProgress = 0f;
        //             m_Title.text = m_TitleText;

        //             m_LeftButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("left-button");
        //             m_LeftButton.RegisterCallback<ClickEvent>(evt => leftButtonClicked?.Invoke());

        //             m_LeftButtonLabel = m_LeftButton.Q<Label>("text");
        //             m_LeftButtonLabel.text = m_LeftButtonLabelText;

        //             m_RightButton = m_DialogBoxLayer.rootVisualElement.Q<Button>("right-button");
        //             m_RightButton.RegisterCallback<ClickEvent>(evt => rightButtonClicked?.Invoke());

        //             m_RightButtonLabel = m_RightButton.Q<Label>("text");
        //             m_RightButtonLabel.text = m_RightButtonLabelText;

        //             m_DialogContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-container");
        //             m_DialogContainer.RegisterCallback<ClickEvent>(evt => backgroundClicked?.Invoke());

        //             m_BoxShadow = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("box-shadow");
        //             m_BoxShadow.style.scale = Vector2.one;
        //             m_ButtonContainer = m_DialogBoxLayer.rootVisualElement.Q<VisualElement>("button-container");

        //             ApplyButtonsDisplaySettings();
        //         }
        //         else
        //         {
        //             LayerManager.RemoveLayer(m_DialogBoxLayer);
        //             LayerManager.RemoveLayer(m_BackgroundPostProcessingLayer);
        //             m_TitlePlayer.animationTime = 0f;
        //             if (m_PostProcessingLayer != null)
        //             {
        //                 LayerManager.RemoveLayer(m_PostProcessingLayer);
        //             }
        //         }
        //     });

        //     var t1 = animation.AddTrack((float t) =>
        //     {
        //         if (m_BackgroundPostProcessingLayer != null)
        //         {
        //             m_BackgroundPostProcessingLayer.tint = Color.Lerp(Color.white, s_EffectLayerColor, t);
        //         }
        //     });
        //     t1.AddKeyframe(0, 0f);
        //     t1.AddKeyframe(30, 1f);

        //     var t2 = animation.AddTrack((float blurSize) =>
        //     {
        //         if (m_BackgroundPostProcessingLayer != null)
        //         {
        //             m_BackgroundPostProcessingLayer.blurSize = blurSize;
        //         }
        //     });
        //     t2.AddKeyframe(0, 0f);
        //     t2.AddKeyframe(30, Layer.DefaultBlurSize);

        //     var t3 = animation.AddTrack((float scaleMultiplier) =>
        //     {
        //         if (m_BoxShadow != null)
        //         {
        //             m_BoxShadow.style.scale = Vector2.one * scaleMultiplier;
        //         }
        //     });
        //     t3.AddKeyframe(20, k_PopupScale);
        //     t3.AddKeyframe(35, 1f);

        //     var t4 = animation.AddTrack((float alpha) =>
        //     {
        //         if (m_DialogBoxLayer != null)
        //         {
        //             m_DialogBoxLayer.alpha = alpha;
        //         }
        //     });
        //     t4.AddKeyframe(20, 0f);
        //     t4.AddKeyframe(35, 1f);

        //     animation.AddEvent(35, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_DialogBoxLayer.interactable = true;
        //             m_TitlePlayer.Play();
        //         }
        //         else
        //         {
        //             m_DialogBoxLayer.interactable = false;
        //             m_TitlePlayer.Pause();
        //         }
        //     });

        //     return animation;
        // }

        // KeyframeAnimation CreateTitleAnimation()
        // {
        //     var animation = new KeyframeAnimation();
        //     animation.AddEvent(0, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: DisplaySortOrder + 1);
        //             m_PostProcessingLayer.overscan = 8f;
        //             m_PostProcessingLayer.maskElement = m_Title;
        //             m_PostProcessingLayer.blurSize = BaseLayer.DefaultBlurSize;

        //         }
        //         else
        //         {
        //             LayerManager.RemoveLayer(m_PostProcessingLayer);
        //         }
        //     });

        //     var t1 = animation.AddTrack((float opacity) =>
        //     {
        //         if (m_Title != null)
        //         {
        //             m_Title.style.opacity = opacity;
        //         }
        //     });
        //     t1.AddKeyframe(0, 0f);
        //     t1.AddKeyframe(20, 1f);

        //     var blurTrack = animation.AddTrack((float blurSize) =>
        //     {
        //         if (m_PostProcessingLayer != null)
        //         {
        //             m_PostProcessingLayer.blurSize = blurSize;
        //         }
        //     });
        //     blurTrack.AddKeyframe(10, BaseLayer.DefaultBlurSize);
        //     blurTrack.AddKeyframe(30, 0f, Easing.StepOut);

        //     var t3 = animation.AddTrack((float animationProgress) => m_Title?.SetAnimationProgress(animationProgress));
        //     t3.AddKeyframe(30, 0f);
        //     t3.AddKeyframe(90, 1f);

        //     animation.AddEvent(90, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             m_PostProcessingLayer.maskElement = m_Title.label;
        //             m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
        //             m_PostProcessingLayer.blurSize = BaseLayer.DefaultBlurSize;
        //         }
        //         else
        //         {
        //             m_PostProcessingLayer.maskElement = m_Title;
        //             m_PostProcessingLayer.overscan = 8f;
        //         }
        //     });

        //     var t4 = animation.AddTrack((float opacity) =>
        //     {
        //         if (m_Title?.label != null)
        //         {
        //             m_Title.label.style.opacity = opacity;
        //         }
        //     });
        //     t4.AddKeyframe(90, 0f);
        //     t4.AddKeyframe(110, 1f);

        //     blurTrack.AddKeyframe(90, BaseLayer.DefaultBlurSize);
        //     blurTrack.AddKeyframe(120, 0f);

        //     animation.AddEvent(120, () =>
        //     {
        //         if (m_Player.playbackSpeed > 0)
        //         {
        //             LayerManager.RemoveLayer(m_PostProcessingLayer);
        //         }
        //         else
        //         {
        //             m_PostProcessingLayer = LayerManager.CreatePostProcessingLayer(displaySortOrder: DisplaySortOrder + 1);
        //             m_PostProcessingLayer.overscan = new Overscan(8, 8, 0, 8);
        //             m_PostProcessingLayer.maskElement = m_Title.label;
        //         }
        //     });

        //     return animation;
        // }

        // void ApplyButtonsDisplaySettings()
        // {
        //     switch (m_ButtonsDisplay)
        //     {
        //         case ButtonsDisplay.Both:
        //             m_ButtonContainer.RemoveFromClassList(k_SingleButtonVariantButtonContainerUssClassName);
        //             m_RightButton.SetEnabled(true);
        //             break;
        //         case ButtonsDisplay.Left:
        //             m_ButtonContainer.AddToClassList(k_SingleButtonVariantButtonContainerUssClassName);
        //             m_RightButton.SetEnabled(false);
        //             break;
        //     }
        // }

        // public void ShowImmediate()
        // {

        // }

        // public void HideImmediate()
        // {

        // }

        // public UniTask Show(CancellationToken cancellationToken = default)
        // {
        //     return UniTask.CompletedTask;
        // }

        // public UniTask Hide(CancellationToken cancellationToken = default)
        // {
        //     return UniTask.CompletedTask;
        // }


        // List<DialogBox> m_DialogBoxes = new List<DialogBox>();

        // struct DialogBoxEntry
        // {
        //     public DialogBox dialogBox;
        //     public Layer dialogBoxLayer;
        //     public PostProcessingLayer dialogBoxBackgroundLayer;
        // }

        // // public DialogBox CreateDialogBox()
        // // {
        // //     var dialogBox = new DialogBox();

        // //     var dialogBoxLayer = LayerManager.CreateLayer();
        // //     dialogBoxLayer.rootVisualElement.Add(dialogBox);

        // //     m_DialogBoxes.Add(dialogBox);
        // //     return dialogBox;
        // // }

        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Z))
        //     {
        //         m_Player.playbackSpeed = 1f;
        //         m_Player.Play();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.C))
        //     {
        //         m_Player.playbackSpeed = -1f;
        //         m_Player.Play();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.Q))
        //     {
        //         ShowImmediate();
        //     }
        //     else if (Input.GetKeyDown(KeyCode.E))
        //     {
        //         HideImmediate();
        //     }
        // }

        public void Init()
        {

        }
    }
}
