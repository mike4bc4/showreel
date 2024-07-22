using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class FloatUtils
    {
        public static bool IsNan(this float f)
        {
            return float.IsNaN(f);
        }
    }
}
