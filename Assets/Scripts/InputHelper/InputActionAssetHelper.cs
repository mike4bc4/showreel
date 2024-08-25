using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace InputHelper
{
    public class InputActionAssetHelper
    {
        static InputActionAssetHelper s_Instance;

        InputActionAsset m_InputActionAsset;
        Dictionary<Guid, InputActionHelper> m_InputActions;
        Dictionary<string, InputActionHelper> m_InputActionsLookup;

        public static InputActionAssetHelper Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new InputActionAssetHelper();
                }

                return s_Instance;
            }
        }

        public static InputActionAsset InputActionAsset
        {
            get => Instance.m_InputActionAsset;
            set
            {
                Instance.m_InputActionAsset = value;
                Rebuild();
            }
        }

        static Dictionary<Guid, InputActionHelper> inputActions => Instance.m_InputActions;
        static Dictionary<string, InputActionHelper> inputActionsLookup => Instance.m_InputActionsLookup;

        private InputActionAssetHelper()
        {
            m_InputActions = new Dictionary<Guid, InputActionHelper>();
            m_InputActionsLookup = new Dictionary<string, InputActionHelper>();
        }

        public static IInputActionHelper FindAction(Guid actionId)
        {
            if (inputActions.TryGetValue(actionId, out var inputAction))
            {
                return inputAction;
            }

            return null;
        }

        public static IInputActionHelper FindAction(string actionName)
        {
            if (inputActionsLookup.TryGetValue(actionName.ToLower(), out var inputAction))
            {
                return inputAction;
            }

            return null;
        }

        public static void Rebuild()
        {
            inputActionsLookup.Clear();
            if (InputActionAsset == null)
            {
                inputActions.Clear();
                return;
            }

            var unusedWrapperPaths = new List<Guid>();
            foreach (var kv in inputActions)
            {
                if (!kv.Value.Reattach())
                {
                    unusedWrapperPaths.Add(kv.Key);
                }
            }

            foreach (var path in unusedWrapperPaths)
            {
                inputActions.Remove(path);
            }

            foreach (var inputAction in InputActionAsset)
            {
                if (!inputActions.ContainsKey(inputAction.id))
                {
                    var wrapper = new InputActionHelper(inputAction);
                    inputActions.Add(inputAction.id, wrapper);
                }
            }

            foreach (var kv in inputActions)
            {
                inputActionsLookup.Add(kv.Value.inputAction.GetPath().ToLower(), kv.Value);
            }
        }
    }
}
