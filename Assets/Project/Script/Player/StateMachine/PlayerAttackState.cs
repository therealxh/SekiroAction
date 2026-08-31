using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState  : PlayerState
{
    private int _comboCount;//连击计数
    private float _enterTime;//进入时刻(缓冲帧用);
    public PlayerAttackState(PlayerController ctx, PlayerStateMachine sm) : base(ctx, sm)
    {
    }
    public override void Enter()
    {
        _enterTime = Time.time;
        _comboCount = _ctx.GetNextCombo();
        _ctx.Rb.velocity = new Vector3(0,_ctx.Rb.velocity.y,0);//站定出刀
        _ctx.SetAttackTrigger(_comboCount);//播第一刀
    }
    public override void Update()
    {
        AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);

        //缓冲帧：等Animator完成状态切换(SetTrigger下一帧才生效)
        if(Time.time -  _enterTime > 0.1f)
        {
            //连击：当前刀播放到60%之后，攻击键可接下一刀
            if (stateInfo.normalizedTime > 0.6f && _comboCount < 3)
            {
                if (_ctx.ConsumeAttackPressed())
                {
                    _comboCount = _ctx.GetNextCombo();
                    _ctx.SetAttackTrigger(_comboCount);
                }
            }
            else
            {
                _ctx.ConsumeAttackPressed(); //窗口未开或连段已满：丢弃按键，防滞留
            }
        }       
        //结束:Animatior已切回Locomotion->按输入决定去向
        if(stateInfo.IsName("Locomotion"))
        {
            _ctx.RecordAttackLength(stateInfo.length);
            if(_ctx.MoveInput != Vector2.zero)
            {
                _sm.ChangeState(new PlayerMoveState(_ctx, _sm));//按着方向键->接着跑步
            }
            else
            {
                _sm.ChangeState(new PlayerIdleState(_ctx,_sm));//待机
            }
        }

    }
    public override void Exit() 
    { 
    }
}
