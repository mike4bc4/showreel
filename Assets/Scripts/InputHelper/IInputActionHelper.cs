using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputActionHelper
{
    public event Action<InputAction.CallbackContext> performed;
    public bool isSuppressedThisFrame { get; set; }
    public bool wasPerformedThisFrame { get; }
    public bool wasScheduledThisFrame { get; }
}
