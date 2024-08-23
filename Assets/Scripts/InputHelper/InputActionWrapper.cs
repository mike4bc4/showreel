using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace InputHelper
{
    class InputActionWrapper : IInputAction
    {
        public event Action<InputAction.CallbackContext> performed;

        InputAction m_InputAction;
        int m_LastPerformedFrameIndex;
        int m_PerformedCountThisFrame;
        Guid m_PreviousInputActionId;

        /// <summary>
        /// When this property is read inside performed callback, it will return index BEFORE current
        /// callback was executed, thus value after first performed invocation should be different 
        /// than Time.frameCount, and any following read should return value equal to Time.frameCount. 
        /// </summary>
        public int lastPerformedFrameIndex
        {
            get => m_LastPerformedFrameIndex;
        }

        /// <summary>
        /// When this property is read inside performed callback method, it will return count BEFORE
        /// current call was made, thus value after first performed invocation should be 0, and will
        /// increase with every invocation of 'performed' event.
        /// </summary>
        public int performedCountThisFrame
        {
            get
            {
                if (Time.frameCount != m_LastPerformedFrameIndex)
                {
                    m_PerformedCountThisFrame = 0;
                }

                return m_PerformedCountThisFrame;
            }
        }

        public InputAction inputAction
        {
            get => m_InputAction;
        }

        public InputActionWrapper(InputAction inputAction)
        {
            m_LastPerformedFrameIndex = -1;
            m_LastPerformedFrameIndex = -1;

            m_InputAction = inputAction;
            m_PreviousInputActionId = inputAction.id;
            m_InputAction.performed += OnPerformed;
        }

        internal bool Reattach()
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

        void OnPerformed(InputAction.CallbackContext callbackContext)
        {
            if (Time.frameCount == m_LastPerformedFrameIndex)
            {
                m_PerformedCountThisFrame++;
            }
            else
            {
                m_PerformedCountThisFrame = 1;
            }

            performed?.Invoke(callbackContext);
            m_LastPerformedFrameIndex = Time.frameCount;
        }
    }
}
