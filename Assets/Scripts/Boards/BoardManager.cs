using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controls;
using FSM;
using InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Boards
{
    public class BoardManager : MonoBehaviour
    {
        static BoardManager s_Instance;

        List<Board> m_Boards;
        StateMachine m_StateMachine;
        InterfaceBoard m_InterfaceBoard;
        DialogBoard m_DialogBoard;
        ActionMapsWrapper m_ActionMapsWrapper;

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
        public static ActionMapsWrapper ActionMapsWrapper => s_Instance.m_ActionMapsWrapper;

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            m_ActionMapsWrapper = new ActionMapsWrapper();
            m_Boards = GetComponentsInChildren<Board>(true).ToList();
        }

        void Start()
        {
            foreach (Board board in m_Boards)
            {
                board.EarlyInit();
            }

            foreach (Board board in m_Boards)
            {
                board.Init();
            }

            var initialBoard = GetBoard<InitialBoard>();
            initialBoard.Show();
            initialBoard.inputActions.Enable();
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
