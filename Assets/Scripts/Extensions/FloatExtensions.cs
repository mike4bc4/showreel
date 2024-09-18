using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class FloatExtensions
    {
        public static bool IsNaN(this float f)
        {
            return float.IsNaN(f);
        }

        public static bool EqualsApproximately(this float a, float b, float epsilon = 0.01f)
        {
            return Mathf.Abs(a - b) <= epsilon;
        }
    }
}
