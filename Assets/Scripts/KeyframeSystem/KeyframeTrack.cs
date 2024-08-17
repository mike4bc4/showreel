using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public class KeyframeTrack
    {
        List<Keyframe> m_Keyframes;
        Action<float> m_Setter;
        // float m_PreviousValue;

        internal List<Keyframe> keyframes
        {
            get => m_Keyframes;
        }

        public Action<float> setter
        {
            get => m_Setter;
            set => m_Setter = value;
        }

        public KeyframeTrack()
        {
            m_Keyframes = new List<Keyframe>();
            // m_PreviousValue = float.NaN;
        }

        public Keyframe AddKeyframe(int frameIndex, float value, Easing easing = Easing.Ease)
        {
            var keyframe = new Keyframe();
            keyframe.frameIndex = frameIndex;

            int idx = m_Keyframes.BinarySearch(keyframe, Keyframe.Comparer);
            if (idx >= 0)
            {
                keyframe = m_Keyframes[idx];
            }

            keyframe.value = value;
            keyframe.easing = easing;
            m_Keyframes.Add(keyframe);
            m_Keyframes.Sort(Keyframe.Comparer);
            return keyframe;
        }

        Keyframe GetPreviousKeyframe(int index)
        {
            Keyframe previousKeyframe = null;
            foreach (var keyframe in m_Keyframes)
            {
                if (keyframe.frameIndex >= index)
                {
                    break;
                }

                previousKeyframe = keyframe;
            }

            return previousKeyframe;
        }

        Keyframe GetNextKeyframe(int index)
        {
            Keyframe nextKeyframe = null;
            for (int i = m_Keyframes.Count - 1; i >= 0; i--)
            {
                if (m_Keyframes[i].frameIndex < index)
                {
                    break;
                }

                nextKeyframe = m_Keyframes[i];
            }

            return nextKeyframe;
        }

        internal void Sample(int frameIndex, bool force = false)
        {
            var previousKeyframe = GetPreviousKeyframe(frameIndex);
            var nextKeyframe = GetNextKeyframe(frameIndex);

            if (previousKeyframe == null && nextKeyframe == null)
            {
                return;
            }

            float val;
            if (previousKeyframe == null)
            {
                val = nextKeyframe.value;
            }
            else if (nextKeyframe == null)
            {
                val = previousKeyframe.value;
            }
            else
            {
                float x = (frameIndex - previousKeyframe.frameIndex) / (float)(nextKeyframe.frameIndex - previousKeyframe.frameIndex);
                float t = previousKeyframe.easing.Evaluate(x);
                val = Mathf.Lerp(previousKeyframe.value, nextKeyframe.value, t);
            }

            setter?.Invoke(val);

            // if (force || val != m_PreviousValue)
            // {
            //     setter?.Invoke(val);
            //     m_PreviousValue = val;
            // }
        }
    }
}