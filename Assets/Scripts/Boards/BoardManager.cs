using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boards.States;
using Controls;
using FSM;
using InputHelper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Boards
{
    public class InputManager
    {
        const string k_ActionMapName = "UI";
        const string k_AnyActionName = "Any";
        const string k_ConfirmActionName = "Confirm";
        const string k_CancelActionName = "Cancel";
        const string k_LeftActionName = "Left";
        const string k_RightActionName = "Right";
        const string k_InfoActionName = "Info";

        InputActionAsset m_InputActionAsset;
        InputActionMap m_ActionMap;
        InputAction m_AnyAction;
        InputAction m_ConfirmAction;
        InputAction m_CancelAction;
        InputAction m_LeftAction;
        InputAction m_RightAction;
        InputAction m_InfoAction;

        public InputActionMap actionMap => m_ActionMap;
        public InputAction anyAction => m_AnyAction;
        public InputAction confirmAction => m_ConfirmAction;
        public InputAction cancelAction => m_CancelAction;
        public InputAction leftAction => m_LeftAction;
        public InputAction rightAction => m_RightAction;
        public InputAction infoAction => m_InfoAction;

        public InputManager(InputActionAsset inputActionAsset)
        {
            m_InputActionAsset = inputActionAsset;
            InputActionAssetHelper.InputActionAsset = inputActionAsset;
            m_ActionMap = inputActionAsset.FindActionMap(k_ActionMapName);
            m_ActionMap.Enable();
            m_AnyAction = m_ActionMap[k_AnyActionName];
            m_ConfirmAction = m_ActionMap[k_ConfirmActionName];
            m_CancelAction = m_ActionMap[k_CancelActionName];
            m_LeftAction = m_ActionMap[k_LeftActionName];
            m_RightAction = m_ActionMap[k_RightActionName];
            m_InfoAction = m_ActionMap[k_InfoActionName];
        }
    }

    public class BoardManager : MonoBehaviour
    {
        static BoardManager s_Instance;

        [SerializeField] InputActionAsset m_InputActionAsset;

        List<Board> m_Boards;
        List<BoardState> m_BoardStates;
        InputManager m_InputManager;
        BoardStateContext m_StateContext;

        public static InputManager InputManager => s_Instance.m_InputManager;

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
            m_Boards = GetComponentsInChildren<Board>(true).ToList();
            m_InputManager = new InputManager(m_InputActionAsset);
            m_StateContext = new BoardStateContext();

            m_InputManager.anyAction.GetHelper().performed += (ctx) => m_StateContext.Any();
            m_InputManager.cancelAction.GetHelper().performed += (ctx) => m_StateContext.Cancel();
            m_InputManager.confirmAction.GetHelper().performed += (ctx) => m_StateContext.Confirm();
            m_InputManager.leftAction.GetHelper().performed += (ctx) => m_StateContext.Left();
            m_InputManager.rightAction.GetHelper().performed += (ctx) => m_StateContext.Right();
            m_InputManager.infoAction.GetHelper().performed += (ctx) => m_StateContext.Info();
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

            GetBoard<BackgroundBoard>().ShowImmediate();
            m_StateContext.state = new InitialBoardState(m_StateContext);
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
    }
}
