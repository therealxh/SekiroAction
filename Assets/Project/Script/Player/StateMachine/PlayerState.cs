using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState 
{
    protected PlayerController _ctx;//上下文：拿组件引用和数据
    protected PlayerStateMachine _sm;
    public PlayerState(PlayerController ctx,PlayerStateMachine sm)
    {
        _ctx = ctx; _sm = sm;
    }
    public abstract void Enter();//进入状态执行一次
    public abstract void Update();//状态中每帧执行一次
    public abstract void Exit();//结束状态执行一次
    
}
