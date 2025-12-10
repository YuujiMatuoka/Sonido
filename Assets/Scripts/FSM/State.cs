using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T> : IState<T>
{
    Dictionary<T, IState<T>> _transitions;
    protected FSM<T> _fsm;
    public State()
    {
        _transitions = new Dictionary<T, IState<T>>();
    }

    public State(Dictionary<T, IState<T>> transitions)
    {
        _transitions = transitions;
    }

    public virtual void Init()
    {

    }

    public virtual void Execute()
    {

    }

    public virtual void LateExecute()
    {

    }

    public virtual void Sleep()
    {

    }

    public void AddTransition(T input, IState<T> state)
    {
        _transitions[input] = state;
    }

    public void RemoveTransition(T input)
    {
        
        if (_transitions.ContainsKey(input))
        {
            _transitions.Remove(input);
        }
    }

    //Remover trancision solo conociendo el estado y no el input en si
    public void RemoveTransition(IState<T> state)
    {
        foreach (var item in _transitions)
        {
            T key = item.Key;
            IState<T> value = item.Value;
            if(value == state)
            {
                _transitions.Remove(key);
                break;
            }
        }
    }

    public IState<T> GetTransition(T input)
    {
        if (_transitions.ContainsKey(input))
        {
            return _transitions[input];
        }

        return null;
    }

    public FSM<T> SetFSM { set { _fsm = value; } }
}
