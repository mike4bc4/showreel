using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils2
{
    public static class TaskStatusUtils
    {
        public static bool IsCompleted(this TaskStatus taskStatus)
        {
            return taskStatus == TaskStatus.Completed;
        }

        public static void SetCompleted(this ref TaskStatus taskStatus)
        {
            taskStatus = TaskStatus.Completed;
        }

        public static void SetPending(this ref TaskStatus taskStatus)
        {
            taskStatus = TaskStatus.Pending;
        }
    }
}
