using UnityEngine;

public class PlayerMoveState : PlayerState
{
    private Vector2 _lastInput;//上一次输入
    private Vector3 _intentDir;//移动意图方向
    public PlayerMoveState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
    }
    public override void Update()
    {
        //状态转移:战斗输入
        if (TryTransitByInput()) return;

        Vector2 input = _ctx.MoveInput;
        //状态转移:无移动输入->切回地面默认状态(Idle)
        if (input == Vector2.zero)
        {
            _sm.ChangeState(new PlayerIdleState(_ctx, _sm));
            return;
        }
        //意图方向（输入变化时刷新）
        if (input != _lastInput)
        {
            _lastInput = input;
            if (_ctx.IsLocking)
            {
                //锁定中：相对锁定目标（y=靠近/后退，x=横移）
                Vector3 toTarget = _ctx.LockTarget.transform.position - _ctx.transform.position;
                toTarget.y = 0;
                Vector3 forward = toTarget.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                _intentDir = forward * input.y + right * input.x;
            }
            else
            {
                //未锁定：相对相机方向
                _intentDir = Camera.main.transform.TransformDirection(input.x, 0, input.y);
            }
            _intentDir.y = 0;
            _intentDir.Normalize();
        }
        //移动(向速度逼近)
        Vector3 targetVelocity = new Vector3(_ctx.MoveSpeed * _intentDir.x, _ctx.Rb.velocity.y, _ctx.MoveSpeed * _intentDir.z);
        _ctx.Rb.velocity = Vector3.MoveTowards(_ctx.Rb.velocity, targetVelocity, _ctx.Acceleration * Time.deltaTime);
        //转向
        if (_ctx.IsLocking)
        {
            _ctx.RotateTowardsLockTarget(_ctx.RotateSpeed * Time.deltaTime);//锁定中：始终面向敌人
        }
        else if (_intentDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_intentDir);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, targetRotation, _ctx.RotateSpeed * Time.deltaTime);
        }
        //播放动画
        Vector3 horizontalVelocity = new Vector3(_ctx.Rb.velocity.x, 0, _ctx.Rb.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude / _ctx.MoveSpeed;
        _ctx.Animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }
    public override void Exit() { }
}
