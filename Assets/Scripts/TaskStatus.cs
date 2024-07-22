using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This enum serves as external status for UniTask asynchronous calls.
/// Although has to be set manually, it's way more useful because we can
/// await other task status change what would result in throwing an exception
/// if used with default UniTaskStatus.
/// </summary>
public enum TaskStatus
{
    Completed,
    Pending,
}
