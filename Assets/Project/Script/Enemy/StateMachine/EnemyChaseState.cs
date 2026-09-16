using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController ctx, EnemyStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _ctx.Animator.SetFloat("Speed", 1);
    }
    public override void Update()
    {
        //½øÈë¹¥»÷·¶Î§ÇÒÀäÈ´ºÃ->¹¥»÷
        if (_ctx.PlayerInAttackRange && _ctx.CanAttack)
        {
            _sm.ChangeState(new EnemyAttackState(_ctx, _sm));
            return;
        }
        //ÍÑÕ½(³¬³öË÷µÐ·¶Î§£©->´ý»ú
        if (!_ctx.PlayerInDetectRange)
        {
            _sm.ChangeState(new EnemyIdleState(_ctx, _sm));
            return;
        }
        //³¯Íæ¼ÒÒÆ¶¯
        Vector3 dir = _ctx.Player.position - _ctx.transform.position;
        dir.y = 0;
        dir.Normalize();
        _ctx.Rb.velocity = new Vector3(dir.x*_ctx.MoveSpeed,_ctx.Rb.velocity.y,dir.z*_ctx.MoveSpeed);
        //×ªÏòÍæ¼Ò
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, targetRotation, _ctx.RotateSpeed * Time.deltaTime);
    }
    public override void Exit() { }
}
