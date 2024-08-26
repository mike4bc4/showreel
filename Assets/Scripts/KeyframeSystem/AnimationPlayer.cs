using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace KeyframeSystem
{
    public enum WrapMode
    {
        Once,
        Loop,
    }

    public class AnimationPlayer
    {
        public enum Status
        {
            Stopped,
            Paused,
            Playing,
        }

        Dictionary<string, KeyframeAnimation> m_Animations;
        int m_Sampling;
        KeyframeAnimation m_Animation;
        float m_PlaybackSpeed;
        float m_AnimationTime;
        int m_PreviousFrameIndex;
        List<TrackEvent> m_ScheduledEvents;
        WrapMode m_WrapMode;
        Status m_Status;
        bool m_InvokingEvent;
        bool m_UpdateInvalid;

        public bool isPlayingForward
        {
            get => m_PlaybackSpeed >= 0f;
        }

        public Status status
        {
            get => m_Status;
            private set
            {
                m_Status = value;
                if (m_InvokingEvent)
                {
                    m_UpdateInvalid = true;
                }
            }
        }

        public WrapMode wrapMode
        {
            get => m_WrapMode;
            set => m_WrapMode = value;
        }

        public float duration
        {
            get
            {
                if (m_Animation == null)
                {
                    return -1f;
                }

                return m_Animation.lastFrameIndex / (float)sampling;
            }
        }

        public float animationTime
        {
            get => m_AnimationTime;
            set
            {
                m_AnimationTime = value;

                // Fix previous frame index when jumping in animation time to avoid scheduling events
                // placed on skipped frames.
                m_PreviousFrameIndex = m_PlaybackSpeed >= 0 ? frameIndex - 1 : frameIndex + 1;
            }
        }

        public int frameIndex
        {
            get => Mathf.RoundToInt(m_AnimationTime * sampling);
        }

        public int sampling
        {
            get => m_Sampling;
            set => m_Sampling = value;
        }

        public float playbackSpeed
        {
            get => m_PlaybackSpeed;
            set
            {
                var previousPlaybackSpeed = m_PlaybackSpeed;
                m_PlaybackSpeed = value;

                if (Mathf.Sign(previousPlaybackSpeed) != Mathf.Sign(m_PlaybackSpeed))
                {
                    var evtId = Guid.NewGuid().ToString("N");
                    foreach (var evt in m_ScheduledEvents)
                    {
                        if (string.IsNullOrEmpty(evt.id))
                        {
                            evt.id = evtId;
                        }
                    }

                    m_PreviousFrameIndex = m_PlaybackSpeed >= 0 ? frameIndex - 1 : frameIndex + 1;
                }
            }
        }

        public KeyframeAnimation animation
        {
            get => m_Animation;
            set
            {
                if (!m_Animations.ContainsValue(value))
                {
                    throw new Exception("Cannot select animation which has not been added to the player.");
                }

                if (m_Animation == value)
                {
                    return;
                }

                if (status != Status.Stopped)
                {
                    Stop();
                }

                m_Animation = value;
            }
        }

        public KeyframeAnimation this[string name]
        {
            get
            {
                if (m_Animations.TryGetValue(name, out var animation))
                {
                    return animation;
                }

                return null;
            }
        }

        public AnimationPlayer()
        {
            m_Animations = new Dictionary<string, KeyframeAnimation>();
            m_ScheduledEvents = new List<TrackEvent>();
            m_PlaybackSpeed = 1f;
            m_PreviousFrameIndex = -1;
            m_Sampling = 60;
        }

        void Update()
        {
            m_AnimationTime += Time.deltaTime * m_PlaybackSpeed;
            if (frameIndex != m_PreviousFrameIndex)
            {
                m_Animation.SampleTracks(frameIndex);
                ScheduleEvents();
                m_PreviousFrameIndex = frameIndex;
            }

            if (m_ScheduledEvents.Count > 0)
            {
                var firstEventFrameIndex = m_ScheduledEvents[0].frameIndex;
                var batchId = m_ScheduledEvents[0].id;
                for (int i = 0; i < m_ScheduledEvents.Count; i++)
                {
                    var evt = m_ScheduledEvents[i];
                    if (evt.frameIndex != firstEventFrameIndex || evt.id != batchId)
                    {
                        break;
                    }

                    if (evt.currentDelay == 0)
                    {
                        m_InvokingEvent = true;
                        evt.action?.Invoke();
                        m_InvokingEvent = false;

                        // If player state was changed while event was being invoked (probably by event
                        // itself), then we cannot relay on current update loop context anymore. That's
                        // why it will be marked as invalid, and have to exit (and safely restart next 
                        // frame if requested).
                        if (m_UpdateInvalid)
                        {
                            m_UpdateInvalid = false;
                            m_ScheduledEvents.Remove(evt);
                            return;
                        }

                        m_ScheduledEvents.RemoveAt(i);
                    }
                    else
                    {
                        evt.currentDelay--;
                    }
                }
            }

            if (((m_PlaybackSpeed >= 0 && m_AnimationTime >= duration) || (m_PlaybackSpeed < 0 && m_AnimationTime <= 0f)) && m_ScheduledEvents.Count == 0)
            {
                // Because of 'random' values of delta time it's possible that animation time exceeds
                // zero if playing backwards or duration if playing forwards, so let's just clamp it
                // for additional safety.
                m_AnimationTime = Mathf.Clamp(m_AnimationTime, 0f, duration);
                switch (m_WrapMode)
                {
                    case WrapMode.Once:
                        Pause();
                        break;
                    case WrapMode.Loop:
                        m_AnimationTime = m_PlaybackSpeed >= 0f ? 0f : duration;
                        break;
                }
            }
        }

        void ScheduleEvents()
        {
            var startFrameIndex = m_PlaybackSpeed >= 0 ? m_PreviousFrameIndex + 1 : m_PreviousFrameIndex - 1;
            var events = m_Animation.GetEventsInRange(startFrameIndex, frameIndex);
            events.Sort((e1, e2) => e1.frameIndex.CompareTo(e2.frameIndex) * m_PlaybackSpeed >= 0 ? 1 : -1);
            foreach (var evt in events)
            {
                evt.currentDelay = evt.delay;
            }

            m_ScheduledEvents.AddRange(events);
        }

        public void AddAnimation(KeyframeAnimation animation, string name)
        {
            if (animation == null)
            {
                return;
            }

            if (m_Animations.ContainsKey(name))
            {
                m_Animations[name] = animation;
            }
            else
            {
                m_Animations.Add(name, animation);
            }

            animation.player = this;
        }

        public void RemoveAnimation(KeyframeAnimation animation)
        {
            if (animation == null)
            {
                return;
            }

            foreach (var kv in m_Animations.Where(kvp => kvp.Value == animation).ToList())
            {
                m_Animations.Remove(kv.Key);
                animation.player = null;
            }
        }

        public void RemoveAnimation(string name)
        {
            m_Animations.Remove(name, out var animation);
            animation.player = null;
        }

        public void Sample()
        {
            m_Animation.SampleTracks(frameIndex);
        }

        public void Pause()
        {
            status = Status.Paused;
            AnimationPlayerRunner.onUpdate -= Update;
        }

        public void Play()
        {
            if (status == Status.Playing)
            {
                return;
            }

            if (m_Animation == null)
            {
                throw new Exception("First select an animation.");
            }

            if (status == Status.Stopped)
            {
                m_PreviousFrameIndex = m_PlaybackSpeed >= 0 ? frameIndex - 1 : frameIndex + 1;
            }

            status = Status.Playing;
            AnimationPlayerRunner.onUpdate -= Update;
            AnimationPlayerRunner.onUpdate += Update;
        }

        public void Stop()
        {
            status = Status.Stopped;
            AnimationPlayerRunner.onUpdate -= Update;
            m_ScheduledEvents.Clear();
            m_AnimationTime = 0f;
        }
    }
}
