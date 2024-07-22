using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class UniTaskUtils
    {
        public static async UniTask Chain(this UniTask task, List<Func<UniTask>> continuationFunctions)
        {
            await task;
            foreach (var continuationFunction in continuationFunctions)
            {
                await continuationFunction();
            }
        }

        public static async UniTask Chain(this UniTask task, TaskPool taskPool)
        {
            await task;
            for (int i = 0; i < taskPool.length; i++)
            {
                switch (taskPool[i])
                {
                    case Func<UniTask> func:
                        await func();
                        break;
                    case Action func:
                        func();
                        break;
                }
            }
        }
    }
}