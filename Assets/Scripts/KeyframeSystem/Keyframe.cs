using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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

                public Func<bool> forwardDelayPredicate;
                public Func<bool> backwardDelayPredicate;

                public int index { get; set; }
                public string label { get; set; }
                public virtual float progress { get; set; }
                public KeyframeTrack track { get; set; }

                public Keyframe() { }

                public Keyframe(KeyframeDescriptor factory) : this()
                {
                    forward = factory.forward ?? KeyframeAction.Empty;
                    backward = factory.backward ?? KeyframeAction.Empty;
                    forwardRollback = factory.forwardRollback ?? factory.backward;
                    backwardRollback = factory.backwardRollback ?? factory.forward;
                    label = factory.label;
                }

                public IKeyframe DelayForward(Func<bool> predicate)
                {
                    // throw new NotImplementedException();
                    forwardDelayPredicate = predicate;
                    return this;
                }

                public IKeyframe DelayBackward(Func<bool> predicate)
                {
                    // throw new NotImplementedException();
                    backwardDelayPredicate = predicate;
                    return this;
                }
            }
        }
    }
}
