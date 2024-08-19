using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Boards
{
    public class BoardManager : MonoBehaviour
    {
        static BoardManager s_Instance;

        List<Board> m_Boards;
        StateMachine m_StateMachine;
        InterfaceBoard m_InterfaceBoard;
        DialogBoard m_DialogBoard;
        InputActions m_InputActions;

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

        public static StateMachine StateMachine => s_Instance.m_StateMachine;
        public static InputActions InputActions => s_Instance.m_InputActions;

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            m_InputActions = new InputActions();
            m_StateMachine = new StateMachine();
            m_Boards = GetComponentsInChildren<Board>(true).ToList();
        }

        void Start()
        {
            foreach (IBoard board in m_Boards)
            {
                board.Init();
            }

            // m_StateMachine.initialState.AddTransition(InitialBoard.StateName, () =>
            // {
            //     var board = GetBoard<InitialBoard>();
            //     board.Show();
            //     InputActions.InitialBoard.Enable();
            // });

            // m_StateMachine.state = m_StateMachine[InitialBoard.StateName];
            GetBoard<BackgroundBoard>().ShowImmediate();
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
