using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    private Vector2 _lastInput;//上一次输入
    private Vector3 _intentDir;//移动意图方向快照
    public PlayerMoveState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {

    }
    public override void Update()
    {
        //状态转移:攻击
        if (_ctx.ConsumeAttackPressed())
        {
            _sm.ChangeState(new PlayerAttackState(_ctx, _sm));
            return;
        }
        Vector2 input = _ctx.MoveInput;
        //状态转移条件：无输入->切回待机（减速由Idle负责）
        if(input == Vector2.zero)
        {
            _sm.ChangeState(new PlayerIdleState(_ctx,_sm));
            return;
        }
        //意图快照（输入变化时刷新）
        if (input != _lastInput) { 
            _lastInput = input;
            _intentDir = Camera.main.transform.TransformDirection(input.x, 0, input.y);
            _intentDir.y = 0;
            _intentDir.Normalize();
        }
        //移动(加速度逼近)
        Vector3 targetVelocity = new Vector3(_ctx.MoveSpeed * _intentDir.x, _ctx.Rb.velocity.y, _ctx.MoveSpeed * _intentDir.z);
        _ctx.Rb.velocity = Vector3.MoveTowards(_ctx.Rb.velocity,targetVelocity,_ctx.Acceleration*Time.deltaTime);
        //转身
        if (_intentDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_intentDir);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, targetRotation, _ctx.RotateSpeed * Time.deltaTime);
        }
        //动画驱动
        Vector3 horizontalVelocity = new Vector3(_ctx.Rb.velocity.x, 0, _ctx.Rb.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude / _ctx.MoveSpeed;
        _ctx.Animator.SetFloat("Speed",currentSpeed,0.1f,Time.deltaTime);
    }
    public override void Exit() { }

}
