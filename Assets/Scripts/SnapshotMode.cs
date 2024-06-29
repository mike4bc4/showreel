using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum SnapshotMode
{
    Exact = 1,
    SetAbsoluteWorldPosition = 2,
    ClearTransitions = 4,
    Default = SetAbsoluteWorldPosition | ClearTransitions,
}
