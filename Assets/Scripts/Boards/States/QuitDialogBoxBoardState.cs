using System.Collections;
using System.Collections.Generic;
using Controls;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boards.States
{
    public class ListBoard2QuitDialogBoxState : QuitDialogBoxState
    {
        public ListBoard2QuitDialogBoxState(BoardStateContext context) : base(context) { }

        protected override void OnHidden()
        {
            context.state = new ListBoard2State(context);
        }
    }

    public class ListBoardQuitDialogBoxState : QuitDialogBoxState
    {
        public ListBoardQuitDialogBoxState(BoardStateContext context) : base(context) { }

        protected override void OnHidden()
        {
            context.state = new ListBoard1State(context);
        }
    }

    public class QuitDialogBoxFromInitialBoardState : QuitDialogBoxState
    {
        public QuitDialogBoxFromInitialBoardState(BoardStateContext context) : base(context) { }

        protected override void OnHidden()
        {
            context.state = new InitialBoardState(context);
        }
    }

    public abstract class QuitDialogBoxState : BoardState
    {
        const int k_DisplaySortOrder = 2000;
        DialogBox m_DialogBox;

        public QuitDialogBoxState(BoardStateContext context) : base(context)
        {
            m_DialogBox = DialogBox.CreateQuitDialogBox();
            m_DialogBox.displaySortOrder = k_DisplaySortOrder;
            m_DialogBox.onStatusChanged += OnStatusChanged;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, Cancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, Confirm);
            m_DialogBox.Show();
        }

        void OnStatusChanged(DialogBox.Status previousStatus)
        {
            if (m_DialogBox.isHidden)
            {
                m_DialogBox.Dispose();
                OnHidden();
            }
        }

        protected abstract void OnHidden();

        public override void Cancel()
        {
            if (!m_DialogBox.isShown)
            {
                return;
            }

            m_DialogBox.Hide();
        }

        public override void Confirm()
        {
            if (!m_DialogBox.isShown)
            {
                return;
            }

            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}