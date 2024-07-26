using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KeyframeSystem
{
    public class KeyframeAction
    {
        Func<IKeyframe, CancellationToken, UniTask> m_Action;

        public static KeyframeAction Empty
        {
            get => new KeyframeAction((keyframe) => { });
        }

        public KeyframeAction(Func<IKeyframe, CancellationToken, UniTask> action)
        {
            m_Action = action;
        }

        public KeyframeAction(Action<IKeyframe> action)
        {
            m_Action = new Func<IKeyframe, CancellationToken, UniTask>((keyframe, token) =>
            {
                action?.Invoke(keyframe);
                return UniTask.CompletedTask;
            });
        }

        public UniTask Invoke(IKeyframe keyframe, CancellationToken cancellationToken)
        {
            if (m_Action != null)
            {
                return m_Action(keyframe, cancellationToken);
            }

            return UniTask.CompletedTask;
        }
    }
}
