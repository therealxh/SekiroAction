using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
    }
    public override void Update()
    {
        //状态转移：战斗输入
        if (TryTransitByInput()) return;

        //减速停止
        _ctx.Rb.velocity = Vector3.MoveTowards(_ctx.Rb.velocity, new Vector3(0, _ctx.Rb.velocity.y, 0), _ctx.Acceleration * Time.deltaTime);
        //播放待机动画
        Vector3 horizontalVelocity = new Vector3(_ctx.Rb.velocity.x, 0, _ctx.Rb.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude / _ctx.MoveSpeed;
        _ctx.Animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
        //转向：锁定中面向敌人
        if (_ctx.IsLocking)
        {
            _ctx.RotateTowardsLockTarget(_ctx.RotateSpeed * Time.deltaTime);
        }
        //状态转移：有移动输入->切移动
        if (_ctx.MoveInput != Vector2.zero)
        {
            _sm.ChangeState(new PlayerMoveState(_ctx, _sm));
        }
    }
    public override void Exit()
    {
    }
}
