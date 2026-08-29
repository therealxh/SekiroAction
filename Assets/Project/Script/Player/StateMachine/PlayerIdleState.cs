using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController ctx , PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Exit()
    {
        
    }
    public override void Update()
    {
        //滑行减速
        _ctx.Rb.velocity = Vector3.MoveTowards(_ctx.Rb.velocity,new Vector3(0,_ctx.Rb.velocity.y,0), _ctx.Acceleration*Time.deltaTime);
        //动画过渡回待机
        Vector3 horizontalVelocity = new Vector3(_ctx.Rb.velocity.x, 0, _ctx.Rb.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude / _ctx.MoveSpeed;
        _ctx.Animator.SetFloat("Speed",currentSpeed,0.1f,Time.deltaTime);
        //转移条件：有输出->转到移动
        if(_ctx.MoveInput != Vector2.zero)
        {
            _sm.ChangeState(new PlayerMoveState(_ctx, _sm));
        }
    }
    public override void Enter()
    {
        
    }
}
