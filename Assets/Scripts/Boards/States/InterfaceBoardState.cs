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
        DiamondBarBoard m_DiamondBarBoard;

        public InterfaceBoardState(BoardStateContext context) : base(context)
        {
            m_InterfaceBoard = BoardManager.GetBoard<InterfaceBoard>();

            m_InterfaceBoard.blocksRaycasts = true;
            if (!m_InterfaceBoard.isVisible)
            {
                m_InterfaceBoard.Show(() =>
                {
                    m_InterfaceBoard.interactable = true;
                    m_DiamondBarBoard.Show(OnDiamondBarBoardShowCompleted);
                });
            }
            else
            {
                m_InterfaceBoard.interactable = true;
            }

            m_DiamondBarBoard = BoardManager.GetBoard<DiamondBarBoard>();
        }

        void OnDiamondBarBoardShowCompleted()
        {
            context.state = new WelcomeDialogBoxBoardState(context);
        }

        public override void Any()
        {
            if (!m_InterfaceBoard.isVisible)
            {
                m_InterfaceBoard.ShowImmediate();
                m_DiamondBarBoard.Show(OnDiamondBarBoardShowCompleted);
            }
            else if (!m_DiamondBarBoard.isVisible)
            {
                m_DiamondBarBoard.ShowImmediate();
                OnDiamondBarBoardShowCompleted();
            }
        }
    }
}
