using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

namespace Boards.States
{
    public class InfoDialogBoxState : BoardState
    {
        const int k_DisplaySortOrder = 2000;
        const int k_InputSortOrder = 2000;

        DialogBox m_DialogBox;
        bool m_Interactable;

        public InfoDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_DialogBox = DialogBox.CreateInfoDialogBox();
            m_DialogBox.displaySortOrder = k_DisplaySortOrder;
            m_DialogBox.inputSortOrder = k_InputSortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, OnConfirmOrCancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, OnConfirmOrCancel);
            m_DialogBox.Show(() => m_Interactable = true);
        }

        void OnHide()
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
        }

        void OnConfirmOrCancel()
        {
            if (m_Interactable)
            {
                m_Interactable = false;
                m_DialogBox.Hide(OnHide);
            }
        }

        public override void Info()
        {
            OnConfirmOrCancel();
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
