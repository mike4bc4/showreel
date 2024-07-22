using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public static class ResolvedStyleUtils
    {
        public static Vector2 GetSize(this IResolvedStyle resolvedStyle)
        {
            return new Vector2(resolvedStyle.width, resolvedStyle.height);
        }
    }
}
