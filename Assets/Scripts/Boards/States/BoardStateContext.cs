using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boards.States
{
    public class BoardStateContext
    {
        BoardState m_State;

        public BoardState state
        {
            get => m_State;
            set => m_State = value;
        }

        public void Cancel()
        {
            m_State.Cancel();
        }

        public void Any()
        {
            m_State.Any();
        }

        public void Confirm()
        {
            m_State.Confirm();
        }

        public void Left()
        {
            m_State.Left();
        }

        public void Right()
        {
            m_State.Right();
        }

        public void Info()
        {
            m_State.Info();
        }
    }
}
