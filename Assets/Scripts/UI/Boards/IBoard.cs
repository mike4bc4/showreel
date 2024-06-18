using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Boards
{
    public interface IBoard
    {
        public void Show();
        public void Hide();
        public void Init();
    }
}
