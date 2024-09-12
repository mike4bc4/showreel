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

        public InitialBoardState(BoardStateContext context) : base(context) { }

        public override void Init()
        {
            m_InitialBoard = BoardManager.GetBoard<InitialBoard>();
            m_InitialBoard.blocksRaycasts = false;
            m_InitialBoard.interactable = true;

            if (context.previousState is not QuitDialogBoxState)
            {
                m_InitialBoard.Show();
            }
        }

        public override void Any()
        {
            if (!m_InitialBoard.interactable)
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
                m_InitialBoard.Hide(() =>
                {
                    m_InitialBoard.interactable = false;
                    context.state = new InterfaceBoardState(context);
                });
            }
        }

        public override void Cancel()
        {
            if (m_InitialBoard.isVisible)
            {
                context.state = new QuitDialogBoxState(context);
            }
        }
    }
}
