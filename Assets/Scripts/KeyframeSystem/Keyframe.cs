using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KeyframeSystem
{
    public class Keyframe
    {
        public static readonly KeyframeComparer Comparer = new KeyframeComparer();

        public class KeyframeComparer : IComparer<Keyframe>
        {
            public int Compare(Keyframe x, Keyframe y)
            {
                return x.frameIndex.CompareTo(y.frameIndex);
            }
        }

        int m_FrameIndex;
        float m_Value;
        Easing m_Easing;

        public int frameIndex
        {
            get => m_FrameIndex;
            set => m_FrameIndex = value;
        }

        public float value
        {
            get => m_Value;
            set => m_Value = value;
        }

        public Easing easing
        {
            get => m_Easing;
            set => m_Easing = value;
        }
    }
}