using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IKeyframe
    {
        public int index { get; }
        public string name { get; }
        public bool isPlaying { get; }
    }
}

