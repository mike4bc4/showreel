using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public partial class KeyframeTrackPlayer
    {
        partial class KeyframeTrack : IKeyframeTrack
        {
            class Keyframe : IKeyframe
            {
                public KeyframeAction forward;
                public KeyframeAction forwardRollback;
                public KeyframeAction backward;
                public KeyframeAction backwardRollback;

                public int index { get; set; }
                public string name { get; set; }
                public bool isPlaying { get; set; }

                public Keyframe() { }

                public Keyframe(KeyframeDescriptor factory) : this()
                {
                    forward = factory.forward ?? KeyframeAction.Empty;
                    backward = factory.backward ?? KeyframeAction.Empty;
                    forwardRollback = factory.forwardRollback ?? factory.backward;
                    backwardRollback = factory.backwardRollback ?? factory.forward;
                    name = factory.name;
                }
            }
        }
    }
}
