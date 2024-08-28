using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boards.States
{
    public abstract class BoardState
    {
        BoardStateContext m_Context;

        protected BoardStateContext context
        {
            get => m_Context;
        }

        public BoardState(BoardStateContext context)
        {
            m_Context = context;
        }

        public virtual void Any() { }
        public virtual void Cancel() { }
        public virtual void Confirm() { }
        public virtual void Left() { }
        public virtual void Right() { }
        public virtual void Info() { }
    }
}