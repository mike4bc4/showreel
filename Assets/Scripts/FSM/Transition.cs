using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    public class Transition
    {
        Action m_Action;
        State m_Target;

        public State target
        {
            get => m_Target;
            internal set => m_Target = value;
        }

        public Action action
        {
            get => m_Action;
            set => m_Action = value;
        }
    }
}
