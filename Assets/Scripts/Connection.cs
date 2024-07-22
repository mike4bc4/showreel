using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    string m_ID;
    StateMachineAction m_AsyncAction;
    Action m_Action;

    public string id
    {
        get => m_ID;
    }

    public StateMachineAction asyncAction
    {
        get => m_AsyncAction;
    }

    public Action action
    {
        get => m_Action;
    }

    public bool isAsync
    {
        get => m_AsyncAction != null;
    }

    public Connection(string id, StateMachineAction action)
    {
        m_ID = id;
        m_AsyncAction = action;
    }

    public Connection(string id, Action action)
    {
        m_ID = id;
        m_Action = action;
    }

    // public void Then()
}

// public class Transition
// {
//     string m_FromID;
//     string m_ToID;
//     StateMachineAction m_ToAction;
//     StateMachineAction m_FromAction;

//     public string fromID
//     {
//         get => m_FromID;
//     }

//     public string toID
//     {
//         get => m_ToID;
//     }

//     public StateMachineAction fromAction
//     {
//         get => m_FromAction;
//         set
//         {

//         }
//     }

//     public StateMachineAction toAction
//     {
//         get => m_ToAction;
//         set
//         {

//         }
//     }

//     public Transition(string fromID, string toID)
//     {
//         m_FromID = fromID;
//         m_ToID = toID;
//     }

//     public Transition(State fromState, State toState) : this(fromState.id, toState.id) { }
// }
