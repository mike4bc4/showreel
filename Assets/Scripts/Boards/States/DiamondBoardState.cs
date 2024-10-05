using System.Collections;
using System.Collections.Generic;
using Boards;
using Boards.States;
using Settings;
using UnityEngine;

namespace Boards.States
{
    public class DiamondBarBoardState : BoardState
    {
        DiamondBarBoard m_DiamondBarBoard;

        public DiamondBarBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_DiamondBarBoard = BoardManager.GetBoard<DiamondBarBoard>();
            m_DiamondBarBoard.interactable = false;
            m_DiamondBarBoard.blocksRaycasts = false;
            m_DiamondBarBoard.activeIndex = -1;
            switch (context.previousState)
            {
                case InterfaceBoardState:
                    m_DiamondBarBoard.Show(SetWelcomeDialogBoxOrPoliticoListBoardState);
                    break;
                case PoliticoListBoardState:
                    m_DiamondBarBoard.Hide(() => context.state = new InterfaceBoardState(context));
                    break;
            }
        }

        void SetWelcomeDialogBoxOrPoliticoListBoardState()
        {
            if (SettingsManager.ShowWelcomeWindow)
            {
                context.state = new WelcomeDialogBoxState(context);
            }
            else
            {
                // Normally interface board would become interactable after welcome dialog
                // box is closed, but if we are skipping right into list board we have to
                // activate it here.
                BoardManager.GetBoard<InterfaceBoard>().interactable = true;
                context.state = new PoliticoListBoardState(context);
            }
        }

        public override void Any()
        {
            switch (context.previousState)
            {
                case InterfaceBoardState:
                    m_DiamondBarBoard.ShowImmediate();
                    SetWelcomeDialogBoxOrPoliticoListBoardState();
                    break;
                case PoliticoListBoardState:
                    m_DiamondBarBoard.HideImmediate();
                    context.state = new InterfaceBoardState(context);
                    break;
            }
        }
    }
}
