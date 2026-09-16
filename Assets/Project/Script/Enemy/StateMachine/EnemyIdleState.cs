using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyController ctx, EnemyStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _ctx.Rb.velocity =new Vector3(0,_ctx.Rb.velocity.y,0);
        _ctx.Animator.SetFloat("Speed", 0);
    }
    public override void Update()
    {
        //Íæ¼Ò½øÈëË÷µÐ·¶Î§->×·»÷
        if (_ctx.PlayerInDetectRange)
        {
            _sm.ChangeState(new EnemyChaseState(_ctx, _sm));
        }
    }
    public override void Exit()
    {
        
    }
}
