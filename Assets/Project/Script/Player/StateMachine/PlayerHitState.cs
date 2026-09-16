using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerState
{
    private float _enterTime;//进入时刻(缓冲帧用)
    public PlayerHitState(PlayerController ctx,PlayerStateMachine sm) : base(ctx, sm)
    {

    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _ctx.Rb.velocity = new Vector3(0,_ctx.Rb.velocity.y,0);
        _ctx.SetHitTrigger();
    }
    public override void Update()
    {
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        //受击播完（回Locomotion）->回地面；硬直期不响应任何输入
        if (Time.time - _enterTime > 0.1f && stateInfo.IsName("Locomotion"))
        {
            TryTransitToGround();
        }
    }
    public override void Exit()
    {
        
    }
}
