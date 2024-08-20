using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils2
{
    public static class Vector3Utils
    {
        public static Translate ToTranslate(this Vector3 vector3)
        {
            return new Translate(vector3.x, vector3.y, vector3.z);
        }

        public static TransformOrigin ToTransformOrigin(this Vector3 vector3)
        {
            return new TransformOrigin(vector3.x, vector3.y, vector3.z);
        }
    }
}