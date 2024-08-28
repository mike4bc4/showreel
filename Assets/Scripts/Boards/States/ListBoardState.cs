using System.Collections;
using System.Collections.Generic;
using InputHelper;
using UnityEngine;

namespace Boards.States
{
    public class ListBoardState : BoardState
    {
        ListBoard m_ListBoard;

        public ListBoardState(BoardStateContext context) : base(context)
        {
            m_ListBoard = BoardManager.GetBoard<ListBoard>();
            if (!m_ListBoard.isVisible)
            {
                m_ListBoard.Show();
            }
        }

        public override void Any()
        {
            if (!m_ListBoard.isVisible)
            {
                m_ListBoard.ShowImmediate();
                BoardManager.InputManager.cancelAction.GetHelper().isSuppressedThisFrame = true;
            }
        }

        public override void Left()
        {
        }

        public override void Right()
        {
        }

        public override void Cancel()
        {
            if (m_ListBoard.isVisible)
            {
                context.state = new ListBoardQuitDialogBoxState(context);
            }
        }
    }
}
