using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    string m_Id;
    Action m_TransitionAction;

    public string id
    {
        get => m_Id;
    }

    public Action transitionAction
    {
        get => m_TransitionAction;
        set => m_TransitionAction = value;
    }

    public Connection(string id)
    {
        m_Id = id;
    }
}
