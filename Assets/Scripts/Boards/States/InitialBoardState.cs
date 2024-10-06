using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boards;
using Controls;
using Cysharp.Threading.Tasks;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boards.States
{
    public class InitialBoardState : BoardState
    {
        InitialBoard m_InitialBoard;
        bool m_ShowCompleted;

        public InitialBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_InitialBoard = BoardManager.GetBoard<InitialBoard>();
            m_InitialBoard.blocksRaycasts = false;
            m_InitialBoard.interactable = true;

            switch (context.previousState)
            {
                case QuitDialogBoxState:
                    m_ShowCompleted = true;
                    break;
                default:
                    m_InitialBoard.Show(() => m_ShowCompleted = true);
                    break;
            }
        }

        protected override void OnBeforeActionExecution()
        {
            // Any is always scheduled, no need to check.
            if (!m_ShowCompleted)
            {
                UnscheduleAction(OnCancel); // Suppress cancel if any skips animation.
            }
            else if (IsActionScheduled(OnCancel))
            {
                UnscheduleAction(OnAny);    // Suppress any if animation cannot be skipped and cancel was pressed.
            }
        }

        protected override void OnAny()
        {
            if (!m_ShowCompleted)
            {
                m_InitialBoard.ShowImmediate();
                m_InitialBoard.interactable = false;
                m_ShowCompleted = true;
            }
            else
            {
                enabled = false;
                m_InitialBoard.Hide(() => context.state = new InterfaceBoardState(context));
            }
        }

        protected override void OnCancel()
        {
            m_InitialBoard.interactable = false;
            enabled = false;
            context.state = new QuitDialogBoxState(context);
        }
    }
}
