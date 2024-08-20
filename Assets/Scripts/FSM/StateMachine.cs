using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace FSM
{
    public class StateMachine
    {
        Dictionary<string, State> m_States;
        State m_State;
        State m_InitialState;

        public State initialState
        {
            get => m_InitialState;
        }

        public State state
        {
            get => m_State;
            set
            {
                var transition = m_State[value.name];
                if (transition == null)
                {
                    throw new Exception($"Transition from '{m_State.name}' to '{value.name}' does not exist.");
                }

                transition.action?.Invoke();
                m_State = value;
            }
        }

        public StateMachine()
        {
            m_States = new Dictionary<string, State>();
            m_InitialState = AddState($"InitialState({Guid.NewGuid().ToString("N")})");
            m_State = m_InitialState;
        }

        public State this[string name]
        {
            get
            {
                if (m_States.TryGetValue(name, out var state))
                {
                    return state;
                }

                return null;
            }
        }

        /// <summary>
        /// Adds new state to state machine with give name. If name parameter is default, state will
        /// be initialized with GUID as a name.
        /// </summary>
        public State AddState(string name = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = Guid.NewGuid().ToString("N");
            }
            else if (m_States.ContainsKey(name))
            {
                throw new Exception($"State named '{name}' already exist.");
            }

            var state = new State();
            state.name = name;
            state.stateMachine = this;
            m_States.Add(name, state);

            return state;
        }

        public void RemoveState(State state)
        {
            RemoveState(state.name);
        }

        public void RemoveState(string name)
        {
            if (name == m_InitialState.name)
            {
                throw new Exception("Cannot remove initial state");
            }

            RemoveStateInternal(name);
        }

        void RemoveStateInternal(string name)
        {
            if (!m_States.Remove(name, out var removedState))
            {
                return;
            }

            removedState.stateMachine = null;

            // Remove all transitions leading to removed state.
            foreach (var kv in m_States)
            {
                var state = kv.Value;
                var transition = state[name];
                if (transition != null)
                {
                    state.RemoveTransition(transition);
                }
            }
        }
    }
}