using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    State m_CurrentState;
    Dictionary<string, State> m_States;

    public State currentState
    {
        get => m_CurrentState;
    }

    public StateMachine()
    {
        m_States = new Dictionary<string, State>();
    }

    public State GetState(string id)
    {
        if (m_States.TryGetValue(id, out var state))
        {
            return state;
        }

        return null;
    }

    public State AddState(string id)
    {
        var state = new State(id);
        if (!m_States.TryAdd(id, state))
        {
            Debug.LogErrorFormat("State with id '{0}' already exists in state machine.", id);
            return null;
        }

        return state;
    }

    public void SetState(string id)
    {
        if (!m_States.TryGetValue(id, out var state))
        {
            Debug.LogErrorFormat("State with id '{0}' does not exist in state machine.", id);
            return;
        }

        if (m_CurrentState == null)
        {
            m_CurrentState = state;
            state.action?.Invoke();
        }
        else
        {
            var connection = m_CurrentState.GetConnection(id);
            if (connection == null)
            {
                Debug.LogErrorFormat("State '{0}' does not have defined connection with state '{1}'.", m_CurrentState.id, id);
                return;
            }

            connection.transitionAction?.Invoke();
            m_CurrentState = state;
            state.action?.Invoke();
        }
    }
}
