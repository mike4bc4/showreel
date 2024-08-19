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
    }
}
