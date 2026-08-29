using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;//移速
    [SerializeField] private float rotateSpeed = 10f;//转向速度
    [SerializeField] private float acceleration = 15f;   // 单位：米/秒²，每秒速度增加 15

    private PlayerStateMachine _stateMachine; //状态机
    private Rigidbody _rb; //刚体引用
    private PlayerInputHandler _input; //输入引用
    private Animator _animator;//动画引用
    //上下文属性
    public Rigidbody Rb => _rb;
    public Vector2 MoveInput => _input.MoveInput;
    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;
    public float Acceleration => acceleration;
    public Animator Animator => _animator;
    private void Awake()
    {
        //获取同一物体上的组件
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputHandler>();
        _animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _stateMachine = new PlayerStateMachine();
        _stateMachine.ChangeState(new PlayerIdleState(this,_stateMachine));
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _stateMachine.Update();
    }
}
