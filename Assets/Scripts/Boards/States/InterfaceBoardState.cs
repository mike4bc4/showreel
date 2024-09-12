using System.Collections;
using System.Collections.Generic;
using Controls;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boards.States
{
    public class InterfaceBoardState : BoardState
    {
        InterfaceBoard m_InterfaceBoard;

        public InterfaceBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_InterfaceBoard = BoardManager.GetBoard<InterfaceBoard>();
            m_InterfaceBoard.interactable = false;
            m_InterfaceBoard.blocksRaycasts = true;
            switch (context.previousState)
            {
                case InitialBoardState:
                    m_InterfaceBoard.Show(() => context.state = new DiamondBarBoardState(context));
                    break;
                case DiamondBarBoardState:
                    m_InterfaceBoard.Hide(() => context.state = new InitialBoardState(context));
                    break;
            }
        }

        public override void Any()
        {
            switch (context.previousState)
            {
                case InitialBoardState:
                    m_InterfaceBoard.ShowImmediate();
                    context.state = new DiamondBarBoardState(context);
                    break;
                case DiamondBarBoardState:
                    m_InterfaceBoard.HideImmediate();
                    context.state = new InitialBoardState(context);
                    break;
            }
        }
    }
}
