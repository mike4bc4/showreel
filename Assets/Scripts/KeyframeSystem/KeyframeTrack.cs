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
        partial class KeyframeTrack : IKeyframeTrack
        {
            List<Keyframe> m_Keyframes;
            Dictionary<string, Keyframe> m_NamedKeyframes;
            TaskScheduler m_TaskScheduler;

            public int keyframeIndex { get; set; }
            public int keyframeCount { get => m_Keyframes.Count; }
            public IKeyframe this[int i] => GetKeyframe(i);
            public IKeyframe this[string name] => GetKeyframe(name);

            public KeyframeTrack()
            {
                m_Keyframes = new List<Keyframe>();
                m_NamedKeyframes = new Dictionary<string, Keyframe>();
                m_TaskScheduler = new TaskScheduler();
            }

            public UniTask Play(CancellationToken cancellationToken = default)
            {
                return m_TaskScheduler.Schedule(async (ct) =>
                {
                    var keyframes = m_Keyframes.Where(k => k.index >= keyframeIndex);
                    foreach (var keyframe in keyframes)
                    {
                        try
                        {
                            keyframe.isPlaying = true;
                            await keyframe.forward.Invoke(keyframe, ct);
                            keyframeIndex++;
                        }
                        catch (OperationCanceledException)
                        {
                            await keyframe.forwardRollback.Invoke(keyframe, default);
                            throw;
                        }
                        finally
                        {
                            keyframe.isPlaying = false;
                        }
                    }

                    keyframeIndex = m_Keyframes.Count - 1;
                }, cancellationToken);
            }

            public UniTask PlayBackwards(CancellationToken cancellationToken = default)
            {
                return m_TaskScheduler.Schedule(async (ct) =>
                {
                    var keyframes = m_Keyframes.Where(k => k.index <= keyframeIndex);
                    keyframes = keyframes.Reverse();

                    foreach (var keyframe in keyframes)
                    {
                        try
                        {
                            keyframe.isPlaying = true;
                            await keyframe.backward.Invoke(keyframe, ct);
                            keyframeIndex--;
                        }
                        catch (OperationCanceledException)
                        {
                            await keyframe.backwardRollback.Invoke(keyframe, default);
                            throw;
                        }
                        finally
                        {
                            keyframe.isPlaying = false;
                        }
                    }

                    keyframeIndex = 0;
                }, cancellationToken);
            }

            public IKeyframe AddKeyframe(KeyframeFactory keyframeFactory)
            {
                var keyframe = new Keyframe(keyframeFactory);
                AddKeyframe(keyframe);
                return keyframe;
            }

            void AddKeyframe(Keyframe keyframe)
            {
                m_Keyframes.Add(keyframe);
                keyframe.index = m_Keyframes.Count - 1;
                if (keyframe.name != null)
                {
                    m_NamedKeyframes.TryAdd(keyframe.name, keyframe);
                }
            }

            public IKeyframe AddWaitUntilKeyframe(WaitUntilKeyframeFactory keyframeFactory)
            {
                var keyframe = new Keyframe() { name = keyframeFactory.name };
                keyframe.forward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
                {
                    await UniTask.WaitUntil(() => keyframeFactory.forwardPredicate != null ? keyframeFactory.forwardPredicate() : true, cancellationToken: cancellationToken);
                });

                keyframe.backward = new KeyframeAction(async (IKeyframe keyframe, CancellationToken cancellationToken) =>
                {
                    await UniTask.WaitUntil(() => keyframeFactory.backwardPredicate != null ? keyframeFactory.backwardPredicate() : true, cancellationToken: cancellationToken);
                });

                keyframe.forwardRollback = KeyframeAction.Empty;
                keyframe.backwardRollback = KeyframeAction.Empty;
                AddKeyframe(keyframe);
                return keyframe;
            }

            public IKeyframe GetKeyframe(int index)
            {
                return m_Keyframes.ElementAtOrDefault(index);
            }

            public IKeyframe GetKeyframe(string name)
            {
                if (m_NamedKeyframes.TryGetValue(name, out var keyframe))
                {
                    return keyframe;
                }

                return null;
            }

            T LerpUnclamped<T>(T a, T b, float t)
            {
                switch (a, b)
                {
                    case (float lhs, float rhs):
                        return (T)(object)Mathf.LerpUnclamped(lhs, rhs, t);
                    case (Vector2 lhs, Vector2 rhs):
                        return (T)(object)Vector2.LerpUnclamped(lhs, rhs, t);
                    case (Vector3 lhs, Vector3 rhs):
                        return (T)(object)Vector3.LerpUnclamped(lhs, rhs, t);
                    case (Color lhs, Color rhs):
                        return (T)(object)Color.LerpUnclamped(lhs, rhs, t);
                    case (Quaternion lhs, Quaternion rhs):
                        return (T)(object)Quaternion.LerpUnclamped(lhs, rhs, t);
                    default:
                        throw new NotSupportedException($"Cannot interpolate value of type {typeof(T)}");
                }
            }

            public IAnimationKeyframe AddAnimationKeyframe<T>(Action<T> setter, T from, T to, float duration = 1f, TimingFunction timingFunction = TimingFunction.EaseInOutSine, string name = null)
            {
                duration = Mathf.Max(0.001f, duration);
                var animationKeyframe = new AnimationKeyframe()
                {
                    duration = duration,
                    name = name
                };

                animationKeyframe.forward = new KeyframeAction(async (keyframe, cancellationToken) =>
                {
                    while (animationKeyframe.progress < 1f)
                    {
                        var lerpValue = LerpUnclamped(from, to, Curve.Evaluate(timingFunction, animationKeyframe.progress));
                        setter?.Invoke(lerpValue);

                        var t = Time.time;
                        await UniTask.NextFrame(PlayerLoopTiming.Initialization, cancellationToken);
                        animationKeyframe.playbackTime += (Time.time - t);
                    }
                });

                animationKeyframe.backward = new KeyframeAction(async (kf, token) =>
                {
                    while (animationKeyframe.progress > 0f)
                    {
                        var lerpValue = LerpUnclamped(from, to, Curve.Evaluate(timingFunction, animationKeyframe.progress));
                        setter?.Invoke(lerpValue);

                        var t = Time.time;
                        await UniTask.NextFrame(PlayerLoopTiming.Initialization, token);
                        animationKeyframe.playbackTime -= (Time.time - t);
                    }
                });

                animationKeyframe.backwardRollback = KeyframeAction.Empty;
                animationKeyframe.forwardRollback = KeyframeAction.Empty;
                AddKeyframe(animationKeyframe);
                return animationKeyframe;
            }
        }
    }
}
