using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace InputHelper
{
    public static class InputActionExtensions
    {
        public static string GetPath(this InputAction inputAction)
        {
            return inputAction.actionMap.name + "/" + inputAction.name;
        }

        public static int GetPerformedCountThisFrame(this InputAction inputAction)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            return action != null ? action.performedCountThisFrame : -1;
        }

        public static int GetLastPerformedFrameIndex(this InputAction inputAction)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            return action != null ? action.lastPerformedFrameIndex : -1;
        }

        public static void SetPerformedInvokesOncePerFrame(this InputAction inputAction, bool value)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            if (action != null)
            {
                action.performedInvokesOncePerFrame = value;
            }
        }

        public static void SetIsPerformedDelayed(this InputAction inputAction, bool value)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            if (action != null)
            {
                action.isPerformedDelayed = value;
            }
        }

        public static void RegisterHelperPerformedCallback(this InputAction inputAction, Action<InputAction.CallbackContext> callback)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            if (action != null)
            {
                action.performed += callback;
            }
        }

        public static void UnregisterHelperPerformedCallback(this InputAction inputAction, Action<InputAction.CallbackContext> callback)
        {
            var action = InputActionAssetHelper.FindAction(inputAction.id);
            if (action != null)
            {
                action.performed -= callback;
            }
        }
    }
}
