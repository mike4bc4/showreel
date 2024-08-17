using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
// using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace KeyframeSystem
{
    public class TrackEvent
    {
        string m_ID;
        int m_CurrentDelay;
        int m_Delay;
        int m_FrameIndex;
        Action m_Action;

        internal string id
        {
            get => m_ID;
            set => m_ID = value;
        }

        internal int currentDelay
        {
            get => m_CurrentDelay;
            set => m_CurrentDelay = value;
        }

        public int delay
        {
            get => m_Delay;
            set => m_Delay = Mathf.Max(0, value);
        }

        public int frameIndex
        {
            get => m_FrameIndex;
            set => m_FrameIndex = value;
        }

        public Action action
        {
            get => m_Action;
            set => m_Action = value;
        }
    }
}


