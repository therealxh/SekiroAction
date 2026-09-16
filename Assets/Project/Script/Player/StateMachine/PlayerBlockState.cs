using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockState : PlayerState
{
    public PlayerBlockState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _ctx.SetBlock(true);//播放格挡动画
        _ctx.Rb.velocity = new Vector3(0,_ctx.Rb.velocity.y,0);//站定格挡
    }
    public override void Update()
    {
        //状态转移：攻击/闪避打断格挡
        if (TryTransitByAttackDodge()) return;
        //状态转移：松开格挡->回地面状态
        if (!_ctx.IsBlockHeld)
        {
            TryTransitToGround();
        }        
    }
    public override void Exit()
    {
        _ctx.SetBlock(false);
    }
}
