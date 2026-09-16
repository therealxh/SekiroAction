using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyState
{
    protected EnemyController _ctx;//敌人上下文
    protected EnemyStateMachine _sm;
    public EnemyState(EnemyController ctx, EnemyStateMachine sm)
    {
        _ctx = ctx;
        _sm = sm;
    }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
