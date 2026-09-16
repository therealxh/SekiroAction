using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(PlayerController ctx,PlayerStateMachine sm) : base(ctx, sm)
    {

    }
    public override void Enter()
    {
        _ctx.Rb.velocity = new Vector3(0,_ctx.Rb.velocity.y,0);
        _ctx.SetDeathTrigger();
    }
    public override void Update()
    {
        //终态：不响应任何输入
    }
    public override void Exit() { }
}
