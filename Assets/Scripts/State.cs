using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    string m_Id;
    Action m_Action;
    List<Connection> m_Connections;

    public string id
    {
        get => m_Id;
    }

    public Action action
    {
        get => m_Action;
        set => m_Action = value;
    }

    public List<Connection> connections
    {
        get => m_Connections;
    }

    public State(string id)
    {
        m_Id = id;
        m_Connections = new List<Connection>();
    }

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

    public void AddConnection(string id, Action transitionAction)
    {
        var connection = GetConnection(id);
        if (connection == null)
        {
            connection = new Connection(id);
            m_Connections.Add(connection);
        }

        connection.transitionAction = transitionAction;
    }
}