using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private int _comboCount;//当前连段
    private float _enterTime;//进入时刻(缓冲帧用)
    private bool _hitPerformed;//本次挥刀是否已执行判定
    public PlayerAttackState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _comboCount = _ctx.GetNextCombo();
        if (_ctx.IsLocking)
        {
            _ctx.RotateTowardsLockTarget(1f);//锁定中：攻击前对准敌人(瞬间转向)
        }
        _ctx.Rb.velocity = new Vector3(0, _ctx.Rb.velocity.y, 0);//站定攻击
        _ctx.SetAttackTrigger(_comboCount);//播放第N刀
    }
    public override void Update()
    {
        //获取当前播放的攻击动画信息
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);

        //缓冲帧：等Animator完成状态切换(SetTrigger下一帧才生效)
        if (Time.time - _enterTime > 0.1f)
        {
            //攻击判定：动画进行中且播到50%时，执行一次扫掠判定
            if (!_hitPerformed && !stateInfo.IsName("Locomotion") && stateInfo.normalizedTime >= 0.5f)
            {
                _hitPerformed = true;
                AttackResult result = CombatSweep.Perform(_ctx.transform, _ctx.AttackDamage);
                if (result == AttackResult.Blocked)
                {
                    _ctx.TriggerHit();//被格挡：攻击方受弹
                    return;
                }
            }
            //连段判定：当前动画播到60%之后，攻击输入可接下一段
            if (stateInfo.normalizedTime > 0.6f && _comboCount < 3)
            {
                if (_ctx.ConsumeAttackPressed())
                {
                    _comboCount = _ctx.GetNextCombo();
                    _ctx.SetAttackTrigger(_comboCount);
                    _hitPerformed = false;//新一段攻击重新允许判定
                }
            }
            else
            {
                _ctx.ConsumeAttackPressed();//窗口未开或连段已满：丢弃按键防滞留
            }
        }
        //出口:Animator已切回Locomotion->按输入决定去向
        if (stateInfo.IsName("Locomotion"))
        {
            _ctx.RecordAttackLength(stateInfo.length);
            if (TryTransitByInput()) return;
            TryTransitToGround();
        }
    }
    public override void Exit()
    {
    }
}
