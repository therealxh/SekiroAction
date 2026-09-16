using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float _enterTime;//进入时刻(缓冲帧用)
    private bool _hitPerformed;//本次挥刀是否已执行判定
    public EnemyAttackState(EnemyController ctx, EnemyStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _ctx.RecordAttackTime();//冷却起点
        _ctx.Rb.velocity = new Vector3(0, _ctx.Rb.velocity.y, 0);//站定攻击
        _ctx.SetAttackTrigger();
    }
    public override void Update()
    {
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        //缓冲帧后：动画进行中且播到50%时，执行一次扫掠判定
        if (!_hitPerformed && Time.time - _enterTime > 0.1f
            && !stateInfo.IsName("Locomotion") && stateInfo.normalizedTime >= 0.5f)
        {
            _hitPerformed = true;
            AttackResult result = CombatSweep.Perform(_ctx.transform, _ctx.AttackDamage);
            if (result == AttackResult.Blocked)
            {
                _ctx.TriggerHit();//被格挡：被弹开
                return;
            }
        }
        //缓冲帧后，攻击播完(回Locomotion)->按距离决定去向
        if (Time.time - _enterTime > 0.1f && stateInfo.IsName("Locomotion"))
        {
            if (_ctx.PlayerInDetectRange)
            {
                _sm.ChangeState(new EnemyChaseState(_ctx, _sm));
            }
            else
            {
                _sm.ChangeState(new EnemyIdleState(_ctx, _sm));
            }
        }
    }
    public override void Exit()
    {
    }
}
