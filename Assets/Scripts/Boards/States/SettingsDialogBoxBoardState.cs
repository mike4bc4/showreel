using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controls;
using Controls.Raw;
using Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boards.States
{
    public class SettingsDialogBoxState : BoardState
    {
        const int k_DefaultDisplaySortOrder = 2000;
        const int k_DefaultInputSortOrder = 2000;
        DialogBox m_DialogBox;
        Select m_WindowModeSelect;
        Select m_ResolutionSelect;
        Select m_RefreshRateSelect;
        Select m_VerticalSyncSelect;
        Select m_BlurQualitySelect;
        Select m_ShowWelcomeWindowSelect;

        public SettingsDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_DialogBox = DialogBox.CreateSettingsDialogBox();

            m_WindowModeSelect = m_DialogBox.rootVisualElement.Q<Select>("window-mode-select");
            m_ResolutionSelect = m_DialogBox.rootVisualElement.Q<Select>("resolution-select");
            m_RefreshRateSelect = m_DialogBox.rootVisualElement.Q<Select>("refresh-rate-select");
            m_VerticalSyncSelect = m_DialogBox.rootVisualElement.Q<Select>("vertical-sync-select");
            m_BlurQualitySelect = m_DialogBox.rootVisualElement.Q<Select>("blur-quality-select");
            m_ShowWelcomeWindowSelect = m_DialogBox.rootVisualElement.Q<Select>("show-welcome-window-select");

            m_ResolutionSelect.onChoiceChanged += OnResolutionChanged;

            m_ResolutionSelect.SetChoices(SettingsManager.Resolution.choices);
            m_RefreshRateSelect.SetChoices(SettingsManager.RefreshRate.choices);
            m_VerticalSyncSelect.SetChoices(SettingsManager.VerticalSync.choices);
            m_BlurQualitySelect.SetChoices(SettingsManager.BlurQuality.choices);
            m_ShowWelcomeWindowSelect.SetChoices(SettingsManager.ShowWelcomeWindow.choices);

            m_ResolutionSelect.choice = SettingsManager.Resolution.name;
            m_RefreshRateSelect.choice = SettingsManager.RefreshRate.name;
            m_VerticalSyncSelect.choice = SettingsManager.VerticalSync.name;
            m_BlurQualitySelect.choice = SettingsManager.BlurQuality.name;
            m_ShowWelcomeWindowSelect.choice = SettingsManager.ShowWelcomeWindow.name;

            SettingsManager.WindowMode.onChanged += UpdateWindowModeSelect;
            UpdateWindowModeSelect();

            m_DialogBox.displaySortOrder = k_DefaultDisplaySortOrder;
            m_DialogBox.inputSortOrder = k_DefaultInputSortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, Confirm);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, Cancel);
            m_DialogBox.Show();
        }

        void UpdateWindowModeSelect()
        {
            m_WindowModeSelect.SetChoices(SettingsManager.WindowMode.choices);
            m_WindowModeSelect.choice = SettingsManager.WindowMode.name;
        }

        void OnResolutionChanged()
        {
            // Setting resolution causes refresh rate setting to update, as list of refresh rate 
            // setting options changes. This means that we only have to update 'view' part here
            // that is; it's necessary to pass new set of refresh rate options and new choice.
            SettingsManager.Resolution.SetValue(m_ResolutionSelect.choice);

            m_RefreshRateSelect.SetChoices(SettingsManager.RefreshRate.choices);
            m_RefreshRateSelect.choice = SettingsManager.RefreshRate.name;
        }

        void Close()
        {
            SettingsManager.WindowMode.onChanged -= UpdateWindowModeSelect;
            m_DialogBox.Hide(() =>
            {
                m_DialogBox.Dispose();
                switch (context.previousState)
                {
                    case PoliticoListBoardState:
                        context.state = new PoliticoListBoardState(context);
                        break;
                    case LayoutSystemListBoardState:
                        context.state = new LayoutSystemListBoardState(context);
                        break;
                    case LocalizationListBoardState:
                        context.state = new LocalizationListBoardState(context);
                        break;
                    case OtherListBoardState:
                        context.state = new OtherListBoardState(context);
                        break;
                }
            });
        }

        public override void Confirm()
        {
            SettingsManager.WindowMode.SetValue(m_WindowModeSelect.choice);
            SettingsManager.Resolution.SetValue(m_ResolutionSelect.choice);
            SettingsManager.RefreshRate.SetValue(m_RefreshRateSelect.choice);
            SettingsManager.VerticalSync.SetValue(m_VerticalSyncSelect.choice);
            SettingsManager.BlurQuality.SetValue(m_BlurQualitySelect.choice);
            SettingsManager.ShowWelcomeWindow.SetValue(m_ShowWelcomeWindowSelect.choice);
            SettingsManager.Apply();
            SettingsManager.Write();
            Close();
        }

        public override void Cancel()
        {
            SettingsManager.Restore();
            Close();
        }

        public override void Settings()
        {
            SettingsManager.Restore();
            Close();
        }
    }
}
