using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyframeSystem
{
    public interface IKeyframeTrack
    {
        public Action<float> setter { get; set; }
        public int keyframeCount { get; }
        public IKeyframe this[int index] { get; }
        public IKeyframe AddKeyframe(float time, float value, Easing easing = Easing.Ease);
        public IKeyframe AddKeyframe(int frameIndex, float value, Easing easing = Easing.Ease);
    }

    public partial class KeyframeTrackPlayer
    {
        class KeyframeTrack : IKeyframeTrack
        {
            List<Keyframe> m_Keyframes;
            float m_PreviousValue;

            public KeyframeTrackPlayer player { get; set; }
            public Action<float> setter { get; set; }
            public int keyframeCount => m_Keyframes.Count;
            IKeyframe IKeyframeTrack.this[int index] => (IKeyframe)this[index];

            public IKeyframe this[int index]
            {
                get
                {
                    if (index < 0 || index > m_Keyframes.Count - 1)
                    {
                        return null;
                    }

                    return m_Keyframes[index];
                }
            }

            public KeyframeTrack()
            {
                m_Keyframes = new List<Keyframe>();
                m_PreviousValue = float.NaN;
            }

            public IKeyframe AddKeyframe(int frameIndex, float value, Easing easing = Easing.Ease)
            {
                var keyframe = new Keyframe()
                {
                    time = frameIndex / (float)player.sampling,
                    value = value,
                    easing = easing,
                };

                AddKeyframe(keyframe);
                return keyframe;
            }

            public IKeyframe AddKeyframe(float time, float value, Easing easing = Easing.Ease)
            {
                var keyframe = new Keyframe()
                {
                    time = time,
                    value = value,
                    easing = easing,
                };

                AddKeyframe(keyframe);
                return keyframe;
            }

            public void AddKeyframe(Keyframe keyframe)
            {
                m_Keyframes.Add(keyframe);
                keyframe.track = this;
                m_Keyframes.Sort((k1, k2) => k1.frameIndex.CompareTo(k2.frameIndex));
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

            public void Update(int frameIndex, bool force = false)
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

                if (force || val != m_PreviousValue)
                {
                    setter?.Invoke(val);
                    m_PreviousValue = val;
                }
            }
        }
    }
}