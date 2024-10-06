using System.Collections;
using System.Collections.Generic;
using InputHelper;
using UnityEngine;

namespace Boards.States
{
    public abstract class BaseListBoardState : BoardState
    {
        ListBoard m_ListBoard;
        DiamondBarBoard m_DiamondBarBoard;
        bool m_ShowCompleted;

        protected ListBoard listBoard
        {
            get => m_ListBoard;
        }

        protected DiamondBarBoard diamondBarBoard
        {
            get => m_DiamondBarBoard;
        }

        protected bool showCompleted
        {
            get => m_ShowCompleted;
            set => m_ShowCompleted = value;
        }

        public BaseListBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_ListBoard = BoardManager.GetBoard<ListBoard>();
            m_DiamondBarBoard = BoardManager.GetBoard<DiamondBarBoard>();
        }

        protected void ShowBoard()
        {
            listBoard.blocksRaycasts = true;
            listBoard.Show(() =>
            {
                listBoard.interactable = true;
                m_ShowCompleted = true;
            });
        }


        protected override void OnBeforeActionExecution()
        {
            if (IsAnyOfActionsScheduled(OnInfo, OnSettings, OnCancel))
            {
                UnscheduleActions(OnLeft, OnRight, OnAny);
            }
            else if (showCompleted)
            {
                UnscheduleAction(OnAny);
            }
            else
            {
                UnscheduleActions(OnLeft, OnRight, OnCancel, OnSettings, OnInfo);
            }
        }

        protected override void OnAny()
        {
            // State is not being disabled here as further actions should be served.
            listBoard.ShowImmediate();
            listBoard.interactable = true;
            listBoard.blocksRaycasts = true;
            m_ShowCompleted = true;
        }
    }
}
