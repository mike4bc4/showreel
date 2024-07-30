using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public struct AnimationKeyframeDescriptor<T>
    {
        public Action<T> setter;
        public T from;
        public T to;
        public float duration;
        public TimingFunction timingFunction;
        public string label;

        public AnimationKeyframeDescriptor(string label) : this()
        {
            this.label = label;
        }
    }
}
