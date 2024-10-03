using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimerUtility
{
    class TimerRunner : MonoBehaviour
    {
        public event Action update;

        void Update()
        {
            update?.Invoke();
        }
    }
}
