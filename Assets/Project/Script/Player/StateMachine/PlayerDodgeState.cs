using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgeState : PlayerState
{
    private float _enterTime;//进入时刻(缓冲帧用)
    public PlayerDodgeState(PlayerController ctx,PlayerStateMachine sm) : base(ctx, sm)
    {

    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _ctx.Rb.velocity = new Vector3(0,_ctx.Rb.velocity.y,0);//站定闪避
        _ctx.SetInvincible(true);//开无敌帧
        _ctx.SetDodgeTrigger();//播后跳动画

    }
    public override void Update()
    {
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        //缓冲帧之后，Animator已回Locomotion(动画播完)，状态转移
        if(Time.time -  _enterTime > 0.1f && stateInfo.IsName("Locomotion"))
        {
            if (TryTransitByInput()) return;
            TryTransitToGround();
        }
    }
    public override void Exit()
    {
        _ctx.SetInvincible(false);
    }
}
