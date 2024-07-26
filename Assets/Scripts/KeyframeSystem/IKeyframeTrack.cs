using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IKeyframeTrack
    {
        public int keyframeIndex { get; }
        public int keyframeCount { get; }
        public IAnimationKeyframe AddAnimationKeyframe<T>(AnimationKeyframeDescriptor<T> descriptor);
        public IKeyframe AddKeyframe(KeyframeDescriptor descriptor);
        public IKeyframe AddWaitUntilKeyframe(WaitUntilKeyframeDescriptor descriptor);
        public IKeyframe GetKeyframe(int index);
        public IKeyframe GetKeyframe(string name);
        public IKeyframe this[int i] { get; }
        public IKeyframe this[string name] { get; }
    }
}
