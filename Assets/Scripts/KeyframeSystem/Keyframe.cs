using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KeyframeSystem
{
    public interface IKeyframe
    {
        public float time { get; set; }
        public int frameIndex { get; set; }
        public IKeyframeTrack track { get; }
        public float value { get; set; }
        public Easing easing { get; set; }
    }

    public partial class KeyframeTrackPlayer
    {
        class Keyframe : IKeyframe
        {
            public float time { get; set; }
            public KeyframeTrack track { get; set; }
            public float value { get; set; }
            public Easing easing { get; set; }
            IKeyframeTrack IKeyframe.track => this.track;

            public int frameIndex
            {
                get => Mathf.RoundToInt(time * track.player.sampling);
                set => time = value / (float)track.player.sampling;
            }
        }
    }
}