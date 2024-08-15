using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace KeyframeSystem
{
    public interface ITrackEvent
    {
        public float time { get; set; }
        public int delay { get; set; }
        public Action action { get; set; }
        public KeyframeTrackPlayer player { get; }
    }

    public partial class KeyframeTrackPlayer
    {
        class TrackEvent : ITrackEvent
        {
            public int m_Delay;
            public float m_Time;

            public KeyframeTrackPlayer player { get; set; }
            public int currentFrameDelay { get; set; }
            public Action action { get; set; }
            public string batchId { get; set; }

            public int delay
            {
                get => m_Delay;
                set => m_Delay = Mathf.Max(0, value);
            }

            public float time
            {
                get => m_Time;
                set => m_Time = Mathf.Max(0f, value);
            }

            public int frameIndex
            {
                get => Mathf.CeilToInt(time * player.sampling);
                set
                {
                    time = value / (float)player.sampling;
                }
            }

            public TrackEvent() { }
        }
    }
}


