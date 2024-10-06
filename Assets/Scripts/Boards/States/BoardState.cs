using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Boards.States
{
    public abstract class BoardState
    {
        BoardStateContext m_Context;
        List<Action> m_ScheduledActions;
        bool m_ExecutionRequested;
        bool m_Enabled;

        protected BoardStateContext context
        {
            get => m_Context;
        }

        protected bool enabled
        {
            get => m_Enabled;
            set
            {
                if (value != m_Enabled)
                {
                    m_Enabled = value;
                    if (!m_Enabled)
                    {
                        UnscheduleAllActions();
                    }
                }
            }
        }

        public BoardState(BoardStateContext context)
        {
            m_Context = context;
            m_ScheduledActions = new List<Action>();
            m_Enabled = true;
        }

        public virtual void Init() { }

        public void Any() => ScheduleAction(OnAny);
        public void Cancel() => ScheduleAction(OnCancel);
        public void Confirm() => ScheduleAction(OnConfirm);
        public void Left() => ScheduleAction(OnLeft);
        public void Right() => ScheduleAction(OnRight);
        public void Info() => ScheduleAction(OnInfo);
        public void Settings() => ScheduleAction(OnSettings);

        void ScheduleAction(Action action)
        {
            if (!enabled)
            {
                return;
            }

            if (!m_ScheduledActions.Contains(action))
            {
                m_ScheduledActions.Add(action);
                RequestActionsExecution();
            }
        }

        void RequestActionsExecution()
        {
            if (m_ExecutionRequested)
            {
                return;
            }

            m_ExecutionRequested = true;
            Scheduler.delayCall += ExecuteActions;
        }

        void ExecuteActions()
        {
            OnBeforeActionExecution();
            for (int i = 0; i < m_ScheduledActions.Count; i++)
            {
                m_ScheduledActions[i]?.Invoke();
            }

            m_ScheduledActions.Clear();
            m_ExecutionRequested = false;
        }

        protected void UnscheduleAction(Action action)
        {
            m_ScheduledActions.Remove(action);
        }

        protected void UnscheduleActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                m_ScheduledActions.Remove(action);
            }
        }

        protected void UnscheduleAllActions()
        {
            m_ScheduledActions.Clear();
        }

        protected bool IsActionScheduled(Action action)
        {
            return m_ScheduledActions.Contains(action);
        }

        protected bool IsAnyOfActionsScheduled(params Action[] actions)
        {
            foreach (var action in actions)
            {
                if (m_ScheduledActions.Contains(action))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnBeforeActionExecution() { }
        protected virtual void OnAny() { }
        protected virtual void OnCancel() { }
        protected virtual void OnConfirm() { }
        protected virtual void OnLeft() { }
        protected virtual void OnRight() { }
        protected virtual void OnInfo() { }
        protected virtual void OnSettings() { }
    }
}