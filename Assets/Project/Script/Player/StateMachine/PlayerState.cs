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
    //攻击/闪避转移(可打断大多数状态，包括格挡)
    protected bool TryTransitByAttackDodge()
    {
        if (_ctx.ConsumeAttackPressed())
        {
            _sm.ChangeState(new PlayerAttackState(_ctx,_sm));
            return true;
        }
        if (_ctx.ConsumeDodgePressed())
        {
            _sm.ChangeState(new PlayerDodgeState(_ctx,_sm));
            return true;
        }
        return false;
    }
    //格挡转移(仅地面状态可进入)
    protected bool TryTransitToBlock()
    {
        if (_ctx.IsBlockHeld)
        {
            _sm.ChangeState(new PlayerBlockState(_ctx,_sm));
            return true;
        }
        return false;
    }
    //全量战斗输入（Idle/Move/Attack出口/Dodge出口用）
    protected bool TryTransitByInput()
    {
        if(TryTransitByAttackDodge()) return true;
        if(TryTransitToBlock()) return true;
        return false;
    }
    // 地面归属（有移动→Move，无→Idle）
    protected void TryTransitToGround()
    {
        if (_ctx.MoveInput != Vector2.zero)
        {
            _sm.ChangeState(new PlayerMoveState(_ctx, _sm));
        }
        else
        {
            _sm.ChangeState(new PlayerIdleState(_ctx, _sm));
        }
    }
}
