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
        bool m_Interactable;

        public InitialBoardState(BoardStateContext context) : base(context)
        {
            m_Interactable = true;
            m_InitialBoard = BoardManager.GetBoard<InitialBoard>();
            if (!m_InitialBoard.isVisible)
            {
                m_InitialBoard.Show(null);
            }
        }

        public override void Any()
        {
            if (!m_Interactable)
            {
                return;
            }

            if (!m_InitialBoard.isVisible)
            {
                m_InitialBoard.ShowImmediate();
                BoardManager.InputManager.cancelAction.GetHelper().isSuppressedThisFrame = true;
            }
            else if (!BoardManager.InputManager.cancelAction.WasPerformedThisFrame())
            {
                m_Interactable = false;
                m_InitialBoard.Hide(() => context.state = new InterfaceBoardState(context));
            }
        }

        public override void Cancel()
        {
            if (m_InitialBoard.isVisible)
            {
                context.state = new QuitDialogBoxFromInitialBoardState(context);
            }
        }
    }
}
