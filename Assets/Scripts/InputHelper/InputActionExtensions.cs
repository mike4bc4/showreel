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

        public static IInputActionHelper GetHelper(this InputAction inputAction)
        {
            return InputActionAssetHelper.FindAction(inputAction.id);
        }
    }
}
