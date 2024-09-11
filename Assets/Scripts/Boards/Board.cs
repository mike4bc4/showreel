using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boards
{
    public delegate void OnStatusChangedCallback(BoardStatus previousStatus);

    public abstract class Board : MonoBehaviour
    {
        protected Action m_ShowCompletedCallback;
        protected Action m_HideCompletedCallback;
        protected bool m_IsVisible;

        public abstract bool interactable { get; set; }
        public abstract bool blocksRaycasts { get; set; }

        public bool isVisible
        {
            get => m_IsVisible;
        }

        public virtual void EarlyInit() { }
        public virtual void Init() { }
        public virtual void ShowImmediate() { }
        public virtual void HideImmediate() { }

        public virtual void Show(Action onCompleted = null)
        {
            m_ShowCompletedCallback = onCompleted;
        }

        public virtual void Hide(Action onCompleted = null)
        {
            m_HideCompletedCallback = onCompleted;
        }
    }
}