using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeyframeSystem
{
    public class KeyframeAnimation
    {
        List<TrackEvent> m_Events;
        List<KeyframeTrack> m_Tracks;
        AnimationPlayer m_Player;

        public AnimationPlayer player
        {
            get => m_Player;
            internal set => m_Player = value;
        }

        public int lastFrameIndex
        {
            get
            {
                var frameIndex = 0;
                foreach (var track in m_Tracks)
                {
                    var keyframe = track.keyframes.LastOrDefault();
                    if (keyframe != null && keyframe.frameIndex > frameIndex)
                    {
                        frameIndex = keyframe.frameIndex;
                    }
                }

                foreach (var evt in m_Events)
                {
                    if (evt.frameIndex > frameIndex)
                    {
                        frameIndex = evt.frameIndex;
                    }
                }

                return frameIndex;
            }
        }

        public KeyframeAnimation()
        {
            m_Events = new List<TrackEvent>();
            m_Tracks = new List<KeyframeTrack>();
        }

        internal List<TrackEvent> GetEventsInRange(int startFrameIndex, int endFrameIndex)
        {
            if (endFrameIndex < startFrameIndex)
            {
                (startFrameIndex, endFrameIndex) = (endFrameIndex, startFrameIndex);
            }

            var events = new List<TrackEvent>();
            foreach (var evt in m_Events)
            {
                if (startFrameIndex <= evt.frameIndex && evt.frameIndex <= endFrameIndex)
                {
                    events.Add(evt);
                }
            }

            return events;
        }

        public KeyframeTrack AddTrack(Action<float> setter)
        {
            var track = new KeyframeTrack();
            track.setter = setter;
            m_Tracks.Add(track);
            return track;
        }

        public TrackEvent AddEvent(int frameIndex, Action action, int delay = 0)
        {
            var evt = new TrackEvent();
            evt.frameIndex = frameIndex;
            evt.action = action;
            evt.delay = delay;
            m_Events.Add(evt);
            return evt;
        }

        internal void SampleTracks(int frameIndex)
        {
            foreach (var track in m_Tracks)
            {
                track.Sample(frameIndex);
            }
        }
    }
}
