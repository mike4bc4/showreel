using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public partial class KeyframeTrackPlayer
    {
        partial class KeyframeTrack : IKeyframeTrack
        {
            class AnimationKeyframe : Keyframe, IAnimationKeyframe
            {
                public float playbackTime { get; set; }
                public float duration { get; set; }
                public override float progress
                {
                    get => duration != 0 ? Mathf.Clamp01(playbackTime / duration) : float.NaN;
                    set => playbackTime = Mathf.Clamp01(value) * duration;
                }
            }
        }
    }
}
