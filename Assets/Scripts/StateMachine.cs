using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


public delegate UniTask StateMachineAction(CancellationToken ct);

public class StateMachine
{
    State m_State;
    State m_TargetState;
    State m_InitialState;
    Dictionary<string, State> m_States;
    CancellationTokenSource m_Cst;
    // List<Transition> m_Transitions;

    public State targetState
    {
        get => m_TargetState;
    }

    public State initialState
    {
        get => m_InitialState;
    }

    public State state
    {
        get => m_State;
    }

    public bool executingTransition
    {
        get => m_State != m_TargetState;
    }

    public StateMachine()
    {
        m_States = new Dictionary<string, State>();
        // m_Transitions = new List<Transition>();

        m_InitialState = AddState(Guid.NewGuid().ToString("N"));
        m_State = m_InitialState;
    }

    ~StateMachine()
    {
        m_Cst?.Dispose();
    }

    // public Transition GetTransition(State from, State to)
    // {
    //     foreach (var transition in m_Transitions)
    //     {
    //         if (transition.fromID == from.id && transition.toID == to.id)
    //         {
    //             return transition;
    //         }
    //     }

    //     return null;
    // }

    // public Transition AddTransition(State from, State to)
    // {
    //     var transition = GetTransition(from, to);
    //     if (transition == null)
    //     {
    //         transition = new Transition(from, to);
    //         m_Transitions.Add(transition);
    //     }

    //     return transition;
    // }

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

    public void SetState(State state, bool force = false)
    {
        SetState(state.id, force);
    }

    public void SetState(string id, bool force = false)
    {
        if (!m_States.TryGetValue(id, out var targetState))
        {
            Debug.LogErrorFormat("State with id '{0}' does not exist in state machine.", id);
            return;
        }

        // m_TargetState = targetState;
        // var transition = GetTransition(m_State, m_TargetState);
        // if (!force && transition == null)
        // {
        //     Debug.LogErrorFormat("No transition defined from state '{1}' to state '{1}'.", m_State.id, id);
        //     return;
        // }

        // Stop();
        // if(!force)

        var connection = m_State.GetConnection(id);
        if (!force && connection == null)
        {
            Debug.LogErrorFormat("State '{0}' does not have defined connection with state '{1}'.", m_State.id, id);
            return;
        }

        Stop();
        m_TargetState = targetState;

        if (!force && connection.asyncAction != null)
        {
            m_Cst = new CancellationTokenSource();

            async UniTask Task()
            {
                await connection.asyncAction.Invoke(m_Cst.Token);
                m_State = m_TargetState;
            }

            Task().Forget();
        }
        else
        {
            m_State = m_TargetState;
        }
    }

    public void Stop()
    {
        if (m_Cst != null)
        {
            m_Cst.Cancel();
            m_Cst.Dispose();
            m_Cst = null;
        }

        m_TargetState = m_State;
    }

    // public void Reset()
    // {
    //     Stop();
    //     m_State = m_InitialState;
    //     m_TargetState = m_InitialState;
    // }





    // public UniTask WaitUntilStateReached(string id, CancellationToken ct)
    // {
    //     var state = GetState(id);
    //     if (state == null)
    //     {
    //         throw new Exception($"No state with id '{id}' registered.");
    //     }

    //     return UniTask.WaitUntil(() => this.state == state && !m_ExecutingTransition, cancellationToken: ct);
    // }

    // public UniTask WaitUntilStateReached(State state, CancellationToken ct)
    // {
    //     return UniTask.WaitUntil(() => this.state == state && !m_ExecutingTransition, cancellationToken: ct);
    // }
}
