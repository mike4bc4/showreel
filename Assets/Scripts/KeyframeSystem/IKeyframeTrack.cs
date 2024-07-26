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
        public IAnimationKeyframe AddAnimationKeyframe<T>(Action<T> setter, T from, T to, float duration = 1f, TimingFunction timingFunction = TimingFunction.EaseInOutSine, string name = null);
        public IKeyframe AddKeyframe(KeyframeFactory keyframeFactory);
        public IKeyframe AddWaitUntilKeyframe(WaitUntilKeyframeFactory keyframeFactory);
        public IKeyframe GetKeyframe(int index);
        public IKeyframe GetKeyframe(string name);
        public IKeyframe this[int i] { get; }
        public IKeyframe this[string name] { get; }
    }
}
