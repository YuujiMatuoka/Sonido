using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM<T>
{
    IState<T> _current;
    public void SetInit(IState<T> initialState)
    {
        _current = initialState;
        _current.SetFSM = this;
        _current.Init();
    }

    public void OnUpdate()
    {
        if(_current != null)
        {
            _current.Execute();
        }
        
    }

    //public void OnLateUpdate()
    //{
    //    if (_current != null)
    //    {
    //        _current.LateExecute();
    //    }
    //}

    public void Transition(T input)
    {
        IState<T> newState = _current.GetTransition(input);
        if(newState != null)
        {
            _current.Sleep();
            _current = newState;
            _current.SetFSM = this;
            _current.Init();
        }
    }

    public IState<T> CurrentState => _current;
}
