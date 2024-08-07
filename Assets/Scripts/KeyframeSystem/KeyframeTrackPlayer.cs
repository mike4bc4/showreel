using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KeyframeSystem
{
    public partial class KeyframeTrackPlayer
    {
        public const int DefaultSampling = 60;
        public event Action onUpdate;

        List<KeyframeTrack> m_Tracks;
        List<TrackEvent> m_Events;
        List<TrackEvent> m_ScheduledEvents;
        List<TrackEvent> m_QueuedEvents;
        HashSet<TrackEvent> m_InvokedEvents;
        int m_Sampling;
        float m_Time;
        float m_PlaybackSpeed;
        bool m_IsPlaying;

        public float time
        {
            get => m_Time;
            set
            {
                var previousFrameIndex = frameIndex;
                m_Time = value;

                // Event queue becomes invalid when player time is changed externally, thus
                // rebuild is necessary. 
                if (frameIndex != previousFrameIndex)
                {
                    RebuildInvokedEvents();
                    RebuildQueuedEvents();
                }
            }
        }

        public int frameIndex
        {
            get => Mathf.RoundToInt(m_Time * sampling);
            set => time = value / (float)sampling;
        }

        public int lastFrameIndex
        {
            get => Mathf.RoundToInt(duration * sampling);
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
                    RebuildInvokedEvents();
                    RebuildQueuedEvents();

                    // All previously scheduled events which have not been batched together
                    // have to receive new batch id. This allows to avoid mixing them with future
                    // events with similar frame index but executed in other playback direction.
                    var guid = Guid.NewGuid().ToString("N");
                    foreach (var evt in m_ScheduledEvents)
                    {
                        if (string.IsNullOrEmpty(evt.batchId))
                        {
                            evt.batchId = guid;
                        }
                    }
                }
            }
        }

        public bool isPlaying
        {
            get => m_IsPlaying;
        }

        public int sampling
        {
            get => m_Sampling;
            set => m_Sampling = Mathf.Max(1, value);
        }

        public float duration
        {
            get
            {
                var duration = -1f;
                foreach (var track in m_Tracks)
                {
                    var lastKeyframe = track[track.keyframeCount - 1];
                    if (lastKeyframe != null && lastKeyframe.time > duration)
                    {
                        duration = lastKeyframe.time;
                    }
                }

                if (m_Events.Count > 0)
                {
                    var d = m_Events.Last().time;
                    if (d > duration)
                    {
                        duration = d;
                    }
                }

                return duration;
            }
        }

        public IKeyframeTrack this[int index]
        {
            get
            {
                if (index >= 0 && index < m_Tracks.Count)
                {
                    return m_Tracks[index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        public int trackCount => m_Tracks.Count;

        public KeyframeTrackPlayer()
        {
            m_Tracks = new List<KeyframeTrack>();
            m_Events = new List<TrackEvent>();
            m_ScheduledEvents = new List<TrackEvent>();
            m_QueuedEvents = new List<TrackEvent>();
            m_InvokedEvents = new HashSet<TrackEvent>();
            sampling = DefaultSampling;
            m_PlaybackSpeed = 1f;
        }

        public IKeyframeTrack AddKeyframeTrack(Action<float> setter = null)
        {
            var keyframeTrack = new KeyframeTrack();
            keyframeTrack.setter = setter;
            keyframeTrack.player = this;
            m_Tracks.Add(keyframeTrack);
            return keyframeTrack;
        }

        public ITrackEvent AddEvent(int frameIndex, Action action, EventInvokeFlags invokeFlags = EventInvokeFlags.Both, int delay = 0)
        {
            return AddEvent(frameIndex / (float)sampling, action, invokeFlags, delay);
        }

        public ITrackEvent AddEvent(float time, Action action, EventInvokeFlags invokeFlags = EventInvokeFlags.Both, int delay = 0)
        {
            var evt = new TrackEvent();
            evt.player = this;
            evt.time = time;
            evt.action = action;
            evt.invokeFlags = invokeFlags;
            evt.delay = delay;
            m_Events.Add(evt);
            m_Events.Sort((e1, e2) => e1.frameIndex.CompareTo(e2.frameIndex));

            // When event is added while player is running and event's call time is
            // in the player's future, we have to rebuild queued events in order to include
            // it later in scheduled events.
            if (m_IsPlaying)
            {
                // Because RebuildInvokedEvents method would put current frame events into
                // invoked set, we are handling state of new event manually.
                if (playbackSpeed < 0 ? evt.frameIndex >= frameIndex : evt.frameIndex <= frameIndex)
                {
                    m_InvokedEvents.Add(evt);
                }

                RebuildQueuedEvents();
            }

            return evt;
        }

        public List<ITrackEvent> Update(EventInvokeFlags invokeFlags = EventInvokeFlags.Both, bool force = false)
        {
            var events = new List<ITrackEvent>();
            foreach (var evt in m_Events)
            {
                if (evt.frameIndex == frameIndex && (evt.invokeFlags & invokeFlags) == invokeFlags)
                {
                    events.Add(evt);
                }
            }

            if (events.Count > 0)
            {
                events.Sort((e1, e2) => e1.delay.CompareTo(e2.delay));
            }

            UpdateTracks(force);
            return events;
        }

        void UpdateTracks(bool force = false)
        {
            foreach (var track in m_Tracks)
            {
                track.Update(frameIndex, force);
            }
        }

        void RebuildInvokedEvents()
        {
            // Calling this method will reset invoked evens set and then put only those that 
            // are in the past according to current playback time. This allows to later create
            // proper queued event list.
            m_InvokedEvents.Clear();
            if (playbackSpeed < 0)
            {
                for (int i = m_Events.Count - 1; i >= 0; i--)
                {
                    var evt = m_Events[i];
                    if (evt.frameIndex <= frameIndex)
                    {
                        break;
                    }

                    m_InvokedEvents.Add(evt);
                }
            }
            else
            {
                foreach (var evt in m_Events)
                {
                    if (evt.frameIndex >= frameIndex)
                    {
                        break;
                    }

                    m_InvokedEvents.Add(evt);
                }
            }
        }

        void RebuildQueuedEvents()
        {
            // Here we are creating sorted queue of events that should be executed in the future. This means that
            // only events with correct invoke flag are included. Events which already have been invoked or are
            // awaiting for invocation in scheduled events list are skipped, so calling this method twice does
            // not break current player state unless playback direction has been changed in the meantime.
            // In such case invoked events set should be rebuild to avoid adding events from the past that actually
            // have never been reached.
            m_QueuedEvents.Clear();
            var invokeFlags = playbackSpeed < 0 ? EventInvokeFlags.Backward : EventInvokeFlags.Forward;
            foreach (var evt in m_Events)
            {
                if ((evt.invokeFlags & invokeFlags) == invokeFlags && !m_InvokedEvents.Contains(evt) && !m_ScheduledEvents.Contains(evt))
                {
                    m_QueuedEvents.Add(evt);
                }
            }

            m_QueuedEvents.Sort((e1, e2) => e1.frameIndex.CompareTo(e2.frameIndex) * playbackSpeed < 0f ? -1 : 1);
        }

        void ScheduleEvents()
        {
            // Here we are putting events form the past or current frame into the scheduled events list.
            // Old events are included so spike in delta time will not result in skipping any event. Because
            // events are moved from sorted queued events list, scheduled events are in correct order as long
            // as playback speed does not change its sign. In such case queued events should be rebuild to
            // ensure valid order of scheduled events.
            var playbackSign = (int)Mathf.Sign(playbackSpeed);
            for (int i = 0; i < m_QueuedEvents.Count; i++)
            {
                var evt = m_QueuedEvents[i];
                // Invert comparison when playing backwards.
                bool pastOrCurrentEvent = evt.frameIndex.CompareTo(frameIndex) * playbackSign <= 0;
                if (!pastOrCurrentEvent)
                {
                    break;
                }

                evt.currentFrameDelay = evt.delay;
                m_ScheduledEvents.Add(evt);
                m_QueuedEvents.RemoveAt(i);
                i--;
            }
        }

        void OnUpdate()
        {
            var previousFrameIndex = frameIndex;
            m_Time += Time.deltaTime * m_PlaybackSpeed;
            if (previousFrameIndex != frameIndex)
            {
                ScheduleEvents();
                UpdateTracks();
            }

            if (m_ScheduledEvents.Count > 0)
            {
                var firstFrameIndex = m_ScheduledEvents.First().frameIndex;
                var firstBatchId = m_ScheduledEvents.First().batchId;
                for (int i = 0; i < m_ScheduledEvents.Count; i++)
                {
                    var evt = m_ScheduledEvents[i];
                    if (evt.frameIndex != firstFrameIndex || evt.batchId != firstBatchId)
                    {
                        break;
                    }

                    if (evt.currentFrameDelay == 0)
                    {
                        evt.action?.Invoke();
                        m_InvokedEvents.Add(evt);
                        m_ScheduledEvents.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        evt.currentFrameDelay--;
                    }
                }
            }

            var d = duration;
            m_Time = Mathf.Clamp(m_Time, 0f, d);
            if ((m_Time <= 0 || d <= m_Time) && m_ScheduledEvents.Count == 0)
            {
                Pause();
            }

            onUpdate?.Invoke();
            if (!m_IsPlaying)
            {
                return;
            }
        }

        public void Play()
        {
            if (m_IsPlaying)
            {
                return;
            }

            m_Time = Mathf.Clamp(m_Time, 0, duration);
            RebuildQueuedEvents();
            ScheduleEvents();
            UpdateTracks();

            KeyframeTrackPlayerRunner.onUpdate -= OnUpdate;
            KeyframeTrackPlayerRunner.onUpdate += OnUpdate;
            m_IsPlaying = true;
        }

        public void Pause()
        {
            KeyframeTrackPlayerRunner.onUpdate -= OnUpdate;
            m_IsPlaying = false;
        }

        public void Stop()
        {
            Pause();
            m_InvokedEvents.Clear();
            m_ScheduledEvents.Clear();
            m_Time = 0;
        }
    }
}
