using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Boards
{
    public interface IBoard
    {
        public Coroutine Show();
        public Coroutine Hide();
        public void Init();
    }
}
