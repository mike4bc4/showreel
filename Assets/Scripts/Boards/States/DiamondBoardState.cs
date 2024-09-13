using System.Collections;
using System.Collections.Generic;
using Boards;
using Boards.States;
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
                    m_DiamondBarBoard.Show(() => context.state = new WelcomeDialogBoxState(context));
                    break;
                case PoliticoListBoardState:
                    m_DiamondBarBoard.Hide(() => context.state = new InterfaceBoardState(context));
                    break;
            }
        }

        public override void Any()
        {
            switch (context.previousState)
            {
                case InterfaceBoardState:
                    m_DiamondBarBoard.ShowImmediate();
                    context.state = new WelcomeDialogBoxState(context);
                    break;
                case PoliticoListBoardState:
                    m_DiamondBarBoard.HideImmediate();
                    context.state = new InterfaceBoardState(context);
                    break;
            }
        }
    }
}
