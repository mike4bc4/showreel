using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boards
{
    public abstract class Board : MonoBehaviour
    {
        public virtual void EarlyInit() { }
        public virtual void Init() { }
        public virtual void Show() { }
        public virtual void ShowImmediate() { }
        public virtual void Hide() { }
        public virtual void HideImmediate() { }
    }
}