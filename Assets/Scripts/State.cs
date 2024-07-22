using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    string m_ID;
    List<Connection> m_Connections;
    // List<Transition> m_Transitions;

    public string id
    {
        get => m_ID;
    }
    public List<Connection> connections
    {
        get => m_Connections;
    }

    public State(string id)
    {
        m_ID = id;
        m_Connections = new List<Connection>();
        // m_Transitions = new List<Transition>();
    }

    // public Transition GetTransition(string toID)
    // {
    //     foreach (var transition in m_Transitions)
    //     {
    //         if (transition.toID == toID)
    //         {
    //             return transition;
    //         }
    //     }

    //     return null;
    // }

    // public Transition AddTransition(State toState)
    // {
    //     return AddTransition(toState.id);
    // }

    // public Transition AddTransition(string toID)
    // {
    //     var transition = GetTransition(toID);
    //     if (transition == null)
    //     {
    //         transition = new Transition(id, toID);
    //         m_Transitions.Add(transition);
    //     }

    //     return transition;
    // }

    public Connection GetConnection(string id)
    {
        foreach (var connection in m_Connections)
        {
            if (connection.id == id)
            {
                return connection;
            }
        }

        return null;
    }

    public Connection AddConnection(State state, StateMachineAction action = null)
    {
        return AddConnection(state.id, action);
    }

    public Connection AddConnection(string id, StateMachineAction action = null)
    {
        var connection = GetConnection(id);
        if (connection == null)
        {
            connection = new Connection(id, action);
            m_Connections.Add(connection);
        }

        return connection;
    }
}