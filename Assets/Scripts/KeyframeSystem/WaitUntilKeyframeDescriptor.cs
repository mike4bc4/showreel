using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public struct WaitUntilKeyframeDescriptor
    {
        public Func<bool> forwardPredicate;
        public Func<bool> backwardPredicate;
        public string name;
    }
}
