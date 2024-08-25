using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputActionHelper
{
    public event Action<InputAction.CallbackContext> performed;
    public int lastPerformedFrameIndex { get; }
    public int performedCountThisFrame { get; }
    public bool performedInvokesOncePerFrame { get; set; }
    public bool isPerformedDelayed { get; set; }
}
