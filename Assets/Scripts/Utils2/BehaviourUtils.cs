using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BehaviourUtils
{
    public static void SetEnabled(this Behaviour behaviour, bool enabled)
    {
        behaviour.enabled = enabled;
    }
}
