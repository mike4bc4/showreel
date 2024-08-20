using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FSM
{
    public class State
    {
        string m_Name;
        StateMachine m_StateMachine;
        Dictionary<string, Transition> m_Transitions;

        public StateMachine stateMachine
        {
            get => m_StateMachine;
            internal set => m_StateMachine = value;
        }

        public string name
        {
            get => m_Name;
            internal set => m_Name = value;
        }

        public Transition this[string toName]
        {
            get
            {
                if (m_Transitions.TryGetValue(toName, out var transition))
                {
                    return transition;
                }

                return null;
            }
        }

        public State()
        {
            m_Transitions = new Dictionary<string, Transition>();
        }

        public Transition AddTransition(State state, Action action = null)
        {
            return AddTransition(state.name, action);
        }

        public Transition AddTransition(string toName, Action action = null)
        {
            var target = m_StateMachine[toName];
            if (target == null)
            {
                throw new Exception($"State named '{toName}' is does not exist in state machine.");
            }

            var transition = new Transition();
            transition.target = target;
            transition.action = action;
            m_Transitions.Add(target.name, transition);
            return transition;
        }

        public void RemoveTransition(Transition transition)
        {
            foreach (var kv in m_Transitions.Where(kvp => kvp.Value == transition))
            {
                m_Transitions.Remove(kv.Key);
            }
        }
    }
}