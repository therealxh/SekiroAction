using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitState : EnemyState
{
    private float _enterTime;//进入时刻（缓冲帧）

    public EnemyHitState(EnemyController ctx, EnemyStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _ctx.Rb.velocity = new Vector3(0, _ctx.Rb.velocity.y, 0);//硬直：站定
        _ctx.Animator.SetFloat("Speed", 0);
        _ctx.SetHitTrigger();//播放受击动画
    }
    public override void Update()
    {
        //缓冲帧
        if (Time.time - _enterTime < 0.1f) return;
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        //受击播完（切回 Locomotion）→ 按距离回追击/待机
        if (stateInfo.IsName("Locomotion"))
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
    public override void Exit() { }
}
