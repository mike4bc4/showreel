using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IKeyframe
    {
        public int index { get; }
        public string label { get; }
        public float progress { get; }
        public IKeyframe DelayForward(Func<bool> predicate);
        public IKeyframe DelayBackward(Func<bool> predicate);
    }
}

