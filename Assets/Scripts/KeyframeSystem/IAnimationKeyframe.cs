using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IAnimationKeyframe : IKeyframe
    {
        public float playbackTime { get; }
        public float duration { get; }
        public float progress { get; }
    }
}
