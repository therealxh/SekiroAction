using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;//移速
    [SerializeField] private float rotateSpeed = 10f;//转向速度
    [SerializeField] private float acceleration = 15f;   // 单位：米/秒²，每秒速度增加 15

    private float _lastAttackTime = -10f;//上一次出刀时间（初始负值保证第一刀从1开始）
    private float _lastAttackLength = 1f;//上一次攻击动画时长
    private int _comboCount; //连段记忆（跨状态存活）

    private PlayerStateMachine _stateMachine; //状态机
    private Rigidbody _rb; //刚体引用
    private PlayerInputHandler _input; //输入引用
    private Animator _animator;//动画引用
    private bool _attackQueued;//攻击输入缓冲
    //上下文属性
    public Rigidbody Rb => _rb;
    public Vector2 MoveInput => _input.MoveInput;
    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;
    public float Acceleration => acceleration;
    public Animator Animator => _animator;
    public void RecordAttackLength(float length)
    {
        _lastAttackLength = length;
    }
    //得到是否继续连击
    public int GetNextCombo()
    {
        //窗口=实际动画时长+1.2秒缓冲
        float window = _lastAttackLength + 1.2f;
        //1.2秒内再攻击，接下段攻击；超时则回到第一刀
        if(Time.time - _lastAttackTime <= window && _comboCount < 3)
        {
            _comboCount++;
        }
        else
        {
            _comboCount = 1;
        }
        _lastAttackTime = Time.time;
        return _comboCount;
    }
    //攻击触发
    public void SetAttackTrigger (int comboIndex)
    {
        _animator.SetTrigger("Attack"+comboIndex);
    }
    //判断是否按了攻击键
    public bool ConsumeAttackPressed()
    {
        bool pressed = _attackQueued;
        _attackQueued = false;
        return pressed;
    }
    private void Awake()
    {
        //获取同一物体上的组件
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputHandler>();
        _animator = GetComponent<Animator>();
        _input.OnAttackPressed += () => _attackQueued = true;
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
