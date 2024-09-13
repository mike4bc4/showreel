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

        InputActionMap m_ActionMap;
        DialogBox m_DialogBox;
        bool m_Interactable;

        public WelcomeDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_DialogBox = DialogBox.CreateWelcomeDialogBox();
            m_DialogBox.displaySortOrder = DisplaySortOrder;
            m_DialogBox.onStatusChanged += OnStatusChanged;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, OnConfirmOrCancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, OnConfirmOrCancel);
            m_DialogBox.Show();
        }

        void OnStatusChanged(DialogBox.Status previousStatus)
        {
            if (m_DialogBox.isShown)
            {
                m_Interactable = true;
            }
            else if (m_DialogBox.isHidden)
            {
                m_DialogBox.Dispose();
                BoardManager.GetBoard<InterfaceBoard>().interactable = true;
                context.state = new PoliticoListBoardState(context);
            }
        }

        void OnConfirmOrCancel()
        {
            if (m_Interactable)
            {
                m_Interactable = false;
                m_DialogBox.Hide();
            }
        }

        public override void Cancel()
        {
            OnConfirmOrCancel();
        }

        public override void Confirm()
        {
            OnConfirmOrCancel();
        }
    }
}
