using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathState : EnemyState
{
    public EnemyDeathState(EnemyController ctx, EnemyStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _ctx.Rb.velocity = new Vector3(0, _ctx.Rb.velocity.y, 0);//停下
        _ctx.Animator.SetFloat("Speed", 0);
        _ctx.SetDeathTrigger();//播放死亡动画
    }
    public override void Update()
    {
        //终态：什么都不做，永不退出
    }
    public override void Exit() { }
}
