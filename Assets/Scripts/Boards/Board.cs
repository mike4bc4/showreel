using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boards
{
    public abstract class Board : MonoBehaviour
    {
        public virtual void EarlyInit() { }
        public virtual void Init() { }
    }
}