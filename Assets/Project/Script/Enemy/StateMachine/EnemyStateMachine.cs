using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    private EnemyState _current;
    public void ChangeState(EnemyState newState)
    {
        _current?.Exit();
        _current = newState;
        _current?.Enter();
    }
    public void Update()
    {
        _current?.Update();
    }
}
