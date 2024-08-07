using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Vector2Utils
    {
        public static bool IsNan(this Vector2 vector2)
        {
            return vector2.x.IsNan() || vector2.y.IsNan();
        }
    }
}
