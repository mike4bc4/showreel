using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Boards
{
    public class BoardManager : MonoBehaviour
    {
        static readonly string s_InitialStateID = Guid.NewGuid().ToString();
        static BoardManager s_Instance;

        List<Board> m_Boards;
        StateMachine m_StateMachine;
        State m_InitialState;
        InterfaceBoard m_InterfaceBoard;
        DialogBoard m_DialogBoard;

        public static InterfaceBoard InterfaceBoard
        {
            get
            {
                if (s_Instance.m_InterfaceBoard == null)
                {
                    s_Instance.m_InterfaceBoard = GetBoard<InterfaceBoard>();
                }

                return s_Instance.m_InterfaceBoard;
            }
        }

        public static DialogBoard DialogBoard
        {
            get
            {
                if (s_Instance.m_DialogBoard == null)
                {
                    s_Instance.m_DialogBoard = GetBoard<DialogBoard>();
                }

                return s_Instance.m_DialogBoard;
            }
        }

        public static StateMachine StateMachine
        {
            get => s_Instance.m_StateMachine;
        }

        public static State InitialState
        {
            get => s_Instance.m_InitialState;
        }

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;

            m_StateMachine = new StateMachine();
            m_InitialState = m_StateMachine.AddState(s_InitialStateID);
            // m_StateMachine.SetState(s_InitialStateID);

            m_Boards = GetComponentsInChildren<Board>().ToList();
        }

        void Start()
        {
            foreach (IBoard board in m_Boards)
            {
                board.Init();
            }

            // m_StateMachine.SetState(InitialBoard.StateID);
            // m_StateMachine.SetState(TestBoard.StateID);
        }

        public static T GetBoard<T>()
        {
            foreach (IBoard board in s_Instance.m_Boards)
            {
                if (board is T b)
                {
                    return b;
                }
            }

            return default(T);
        }
    }
}
