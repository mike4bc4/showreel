using System.Collections;
using System.Collections.Generic;
using Controls;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boards.States
{
    public class WelcomeDialogBoxState : BoardState
    {
        public const int DisplaySortOrder = 2000;
        public const int InputSortOrder = 2000;

        InputActionMap m_ActionMap;
        DialogBox m_DialogBox;

        public WelcomeDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            enabled = false;

            m_DialogBox = DialogBox.CreateWelcomeDialogBox();
            m_DialogBox.displaySortOrder = DisplaySortOrder;
            m_DialogBox.inputSortOrder = InputSortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, OnConfirmOrCancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, OnConfirmOrCancel);
            m_DialogBox.Show(() => enabled = true);
        }

        void OnConfirmOrCancel()
        {
            enabled = false;
            m_DialogBox.Hide(OnHide);
        }

        void OnHide()
        {
            m_DialogBox.Dispose();
            BoardManager.InterfaceBoard.interactable = true;
            context.state = new PoliticoListBoardState(context);
        }

        protected override void OnCancel()
        {
            OnConfirmOrCancel();
        }

        protected override void OnConfirm()
        {
            OnConfirmOrCancel();
        }
    }
}
