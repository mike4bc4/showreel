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
            m_StateMachine = CreateStateMachine();
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

            m_StateMachine.state = m_StateMachine[BMState.InitialBoard.ToString()];
            GetBoard<BackgroundBoard>().ShowImmediate();
        }

        public static T GetBoard<T>()
        {
            foreach (Board board in s_Instance.m_Boards)
            {
                if (board is T b)
                {
                    return b;
                }
            }

            return default(T);
        }

        StateMachine CreateStateMachine()
        {
            var stateMachine = new StateMachine();
            stateMachine.AddState(BMState.InitialBoard);
            stateMachine.AddState(BMState.InterfaceBoard);

            stateMachine.initialState.AddTransition(BMState.InitialBoard, () =>
            {
                var initialBoard = GetBoard<InitialBoard>();
                initialBoard.Show();
            });

            stateMachine[BMState.InitialBoard].AddTransition(BMState.InterfaceBoard, () =>
            {
                var initialBoard = GetBoard<InitialBoard>();
                var interfaceBoard = GetBoard<InterfaceBoard>();
                var diamondBarBoard = GetBoard<DiamondBarBoard>();

                initialBoard.Hide();
                Action onHideFinished = null;
                initialBoard.onHideFinished += onHideFinished = () =>
                {
                    interfaceBoard.Show();
                    diamondBarBoard.Show();
                    initialBoard.onHideFinished -= onHideFinished;
                };
            });

            return stateMachine;
        }
    }
}
