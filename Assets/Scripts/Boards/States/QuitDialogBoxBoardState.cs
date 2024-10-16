using System.Collections;
using System.Collections.Generic;
using Controls;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boards.States
{
    public class QuitDialogBoxState : BoardState
    {
        const int k_DisplaySortOrder = 2000;
        const int k_DefaultInputSortOrder = 2000;
        DialogBox m_DialogBox;

        public QuitDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            // Dialog boxes do not receive input until shown, so we are not going to serve any actions either.
            enabled = false;

            m_DialogBox = DialogBox.CreateQuitDialogBox();
            m_DialogBox.displaySortOrder = k_DisplaySortOrder;
            m_DialogBox.inputSortOrder = k_DefaultInputSortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, Confirm);
            m_DialogBox.Show(() => enabled = true);
        }

        void OnHide()
        {
            m_DialogBox.Dispose();
            switch (context.previousState)
            {
                case InitialBoardState:
                    context.state = new InitialBoardState(context);
                    break;
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

        protected override void OnCancel()
        {
            enabled = false;
            m_DialogBox.Hide(OnHide);
        }

        protected override void OnConfirm()
        {
            enabled = false;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}