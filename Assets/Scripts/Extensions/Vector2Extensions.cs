using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static bool IsNaN(this Vector2 vector2)
    {
        return float.IsNaN(vector2.x) || float.IsNaN(vector2.y);
    }
}
