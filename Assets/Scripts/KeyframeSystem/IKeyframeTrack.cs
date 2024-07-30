using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IKeyframeTrack
    {
        public IKeyframe firstKeyframe { get; }
        public IKeyframe lastKeyframe { get; }
        public int keyframeIndex { get; }
        public int keyframeCount { get; }
        public IAnimationKeyframe AddAnimationKeyframe<T>(AnimationKeyframeDescriptor<T> descriptor);
        public IKeyframe AddKeyframe(KeyframeDescriptor descriptor);
        public IKeyframe GetKeyframe(int index);
        public IKeyframe GetKeyframe(string label);
        public IKeyframe this[int i] { get; }
        public IKeyframe this[string label] { get; }
    }
}
