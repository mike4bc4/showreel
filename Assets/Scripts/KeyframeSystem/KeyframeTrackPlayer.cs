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
        List<KeyframeTrack> m_KeyframeTracks;
        TaskScheduler m_TaskScheduler;
        
        public IKeyframeTrack this[int index] => m_KeyframeTracks.ElementAtOrDefault(index);

        public KeyframeTrackPlayer()
        {
            m_KeyframeTracks = new List<KeyframeTrack>();
            m_TaskScheduler = new TaskScheduler();
        }

        public IKeyframeTrack AddTrack()
        {
            var keyframeTrack = new KeyframeTrack();
            m_KeyframeTracks.Add(keyframeTrack);
            return keyframeTrack;
        }

        public UniTask Play(CancellationToken cancellationToken = default)
        {
            return m_TaskScheduler.Schedule(async (token) =>
            {
                var tasks = new List<UniTask>();
                foreach (var track in m_KeyframeTracks)
                {
                    tasks.Add(track.Play(token));
                }

                await UniTask.WhenAll(tasks);
            }, cancellationToken);
        }

        public UniTask PlayBackwards(CancellationToken cancellationToken = default)
        {
            return m_TaskScheduler.Schedule(async (token) =>
            {
                var tasks = new List<UniTask>();
                foreach (var track in m_KeyframeTracks)
                {
                    tasks.Add(track.PlayBackwards(token));
                }

                await UniTask.WhenAll(tasks);
            }, cancellationToken);
        }

        public void Pause()
        {
            // Schedule nothing, this will send cancel signal to scheduled or currently executed Play and PlayBackwards.
            m_TaskScheduler.Schedule(() => { });
        }

        public void JumpToEndpoint(Endpoint endpoint)
        {
            m_TaskScheduler.Schedule(() =>
            {
                foreach (var track in m_KeyframeTracks)
                {
                    switch (endpoint)
                    {
                        case Endpoint.Start:
                            track.keyframeIndex = 0;
                            break;
                        case Endpoint.Finish:
                            track.keyframeIndex = track.keyframeCount - 1;
                            break;
                    }
                }
            });
        }
    }
}

