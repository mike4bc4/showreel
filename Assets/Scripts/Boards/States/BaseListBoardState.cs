using System.Collections;
using System.Collections.Generic;
using InputHelper;
using UnityEngine;

namespace Boards.States
{
    public abstract class BaseListBoardState : BoardState
    {
        ListBoard m_ListBoard;
        bool m_AllowShowSkip;

        protected bool allowShowSkip
        {
            get => m_AllowShowSkip;
            set => m_AllowShowSkip = value;
        }

        protected ListBoard listBoard
        {
            get
            {
                if (m_ListBoard == null)
                {
                    m_ListBoard = BoardManager.GetBoard<ListBoard>();
                }

                return m_ListBoard;
            }
        }

        public BaseListBoardState(BoardStateContext context) : base(context) { }

        public override void Any()
        {
            if (allowShowSkip)
            {
                allowShowSkip = false;
                listBoard.interactable = true;
                listBoard.blocksRaycasts = true;
                listBoard.ShowImmediate();
                BoardManager.InputManager.cancelAction.GetHelper().isSuppressedThisFrame = true;
            }
        }
    }
}
