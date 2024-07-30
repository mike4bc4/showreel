using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public struct TrackMarker
    {
        int m_Index;
        float m_Progress;

        public IKeyframeTrack track { get; set; }

        public int index
        {
            get => m_Index;
            set => m_Index = Mathf.Max(0, value);
        }

        public float progress
        {
            get => m_Progress;
            set => m_Progress = Mathf.Clamp01(value);
        }

        public TrackMarker(IKeyframeTrack track, int index, float progress = 0f) : this()
        {
            this.track = track;
            this.index = index;
            this.progress = progress;
        }
    }

    public struct DelayKeyframeDescriptor
    {
        public string label;
        public TrackMarker fromMarker;
        public TrackMarker toMarker;

        public DelayKeyframeDescriptor(string label) : this()
        {
            this.label = label;
        }
    }
}
