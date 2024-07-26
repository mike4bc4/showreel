using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public struct KeyframeDescriptor
    {
        public KeyframeAction forward;
        public KeyframeAction forwardRollback;
        public KeyframeAction backward;
        public KeyframeAction backwardRollback;
        public string name;
    }
}
