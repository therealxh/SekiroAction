using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine 
{
    private PlayerState _current;
    public void ChangeState(PlayerState newState)
    {
        _current?.Exit();//旧状态离开（为null时跳过）
        _current = newState;
        _current?.Enter();//新状态进入
    }
    public void Update()
    {
        _current?.Update();//每帧驱动当前状态
    }
}
