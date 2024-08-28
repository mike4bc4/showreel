using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace InputHelper
{
    class InputActionHelper : IInputActionHelper
    {
        public event Action<InputAction.CallbackContext> performed;

        InputAction m_InputAction;
        Guid m_PreviousInputActionId;
        InputAction.CallbackContext m_LatestCallbackContext;
        bool m_IsSuppressedThisFrame;
        bool m_WasPerformedThisFrame;
        bool m_WasScheduledThisFrame;

        public bool isSuppressedThisFrame
        {
            get => m_IsSuppressedThisFrame;
            set => m_IsSuppressedThisFrame = value;
        }

        public bool wasPerformedThisFrame
        {
            get => m_WasPerformedThisFrame;
            set => m_WasPerformedThisFrame = value;
        }

        public bool wasScheduledThisFrame
        {
            get => m_WasScheduledThisFrame;
            set => m_WasScheduledThisFrame = value;
        }

        public InputAction inputAction
        {
            get => m_InputAction;
        }

        public InputActionHelper(InputAction inputAction)
        {
            m_InputAction = inputAction;
            m_PreviousInputActionId = inputAction.id;
            m_InputAction.performed += OnPerformed;
        }

        public bool Reattach()
        {
            var inputAction = InputActionAssetHelper.InputActionAsset.FindAction(m_PreviousInputActionId);
            if (inputAction == null)
            {
                return false;
            }

            m_InputAction = inputAction;
            m_InputAction.performed -= OnPerformed;
            m_InputAction.performed += OnPerformed;
            return true;
        }

        public void InvokePerformed()
        {
            performed?.Invoke(m_LatestCallbackContext);
        }

        void OnPerformed(InputAction.CallbackContext callbackContext)
        {
            this.m_LatestCallbackContext = callbackContext;
            InputActionAssetHelper.RegisterPerformedAction(this);
        }
    }
}
