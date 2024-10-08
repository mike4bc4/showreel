using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

namespace Boards.States
{
    public class FinalDialogBoxState : BoardState
    {
        const int k_DisplaySortOrder = 2000;
        const int k_InputSortOrder = 2000;

        DialogBox m_DialogBox;

        public FinalDialogBoxState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            enabled = false;

            m_DialogBox = DialogBox.CreateFinalDialogBox();
            m_DialogBox.displaySortOrder = k_DisplaySortOrder;
            m_DialogBox.inputSortOrder = k_InputSortOrder;
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Left, OnConfirm);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Right, OnCancel);
            m_DialogBox.RegisterClickCallback(DialogBox.ButtonIndex.Background, OnCancel);
            m_DialogBox.Show(() => enabled = true);
        }

        void OnHide()
        {
            m_DialogBox.Dispose();

            // Final dialog box shows up only when user reaches final list board, so there is no
            // reason to implement switch to perform change based on previous state.
            context.state = new OtherListBoardState(context);
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
