using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

            m_Boards = GetComponentsInChildren<Board>(true).ToList();

            m_InputActions = new InputActions();
            m_InputActions.UI.Enable();
            m_InputActions.UI.Left.performed += OnLeft;
            m_InputActions.UI.Right.performed += OnRight;
            m_InputActions.UI.Confirm.performed += OnConfirm;
            m_InputActions.UI.Cancel.performed += OnCancel;
            m_InputActions.UI.Any.performed += OnAny;
        }

        void Start()
        {
            foreach (IBoard board in m_Boards)
            {
                board.Init();
            }

            // m_StateMachine.SetState(InitialBoard.StateID);
            // m_StateMachine.SetState(TestBoard.StateID);
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

        void OnLeft(InputAction.CallbackContext callbackContext)
        {
            // Debug.Log("OnLeft");
        }

        void OnRight(InputAction.CallbackContext callbackContext)
        {
            // Debug.Log("OnRight");
        }

        void OnConfirm(InputAction.CallbackContext callbackContext)
        {
            // Debug.Log("OnConfirm");
        }

        void OnCancel(InputAction.CallbackContext callbackContext)
        {
            // Debug.Log("OnCancel");
        }

        void OnAny(InputAction.CallbackContext callbackContext)
        {
            // Debug.Log("OnAny");
        }
    }
}
