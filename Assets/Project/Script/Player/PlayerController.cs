using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private float moveSpeed = 5f;//移速
    [SerializeField] private float rotateSpeed = 10f;//转向速度
    [SerializeField] private float acceleration = 15f;   // 单位：米/秒²，每秒速度增加 15
    [SerializeField] private float maxHp = 100f;//最大生命
    [SerializeField] private float attackDamage = 34f;//攻击力
    [SerializeField] private float lockRange = 12f;//锁敌搜索范围

    private float _lastAttackTime = -10f;//上一次出刀时间（初始负值保证第一刀从1开始）
    private float _lastAttackLength = 1f;//上一次攻击动画时长
    private int _comboCount; //连段记忆（跨状态存活）

    private PlayerStateMachine _stateMachine; //状态机
    private Rigidbody _rb; //刚体引用
    private PlayerInputHandler _input; //输入引用
    private Animator _animator;//动画引用
    private bool _attackQueued;//攻击输入缓冲
    private bool _dodgeQueued;//闪避输入缓冲
    private bool _isInvincible;//无敌帧标志(闪避期间为true)
    private bool _hitQueued;//受击输入缓冲(测试用)
    private bool _deathQueued;//死亡输入缓冲(测试用)
    private bool _isDead;//死亡标志(防重入)
    private float _currentHp;//当前生命
    private bool _isBlocking;//格挡中标志(受击结算免伤用)
    private EnemyController _lockTarget;//当前锁定目标(无则null)
    private bool _lockQueued;//锁定输入缓冲
    //上下文属性
    public Rigidbody Rb => _rb;
    public Vector2 MoveInput => _input.MoveInput;
    public float MoveSpeed => moveSpeed;
    public float RotateSpeed => rotateSpeed;
    public float Acceleration => acceleration;
    public Animator Animator => _animator;
    public bool IsBlockHeld => _input.IsBlockHeld;
    public bool IsInvincible => _isInvincible;
    public bool IsDead => _isDead;
    public float AttackDamage => attackDamage;
    public bool IsBlocking => _isBlocking;
    public bool IsLocking => _lockTarget != null;
    public EnemyController LockTarget => _lockTarget;
    public float CurrentHp => _currentHp;//当前生命(UI读取)
    public float MaxHp => maxHp;//最大生命(UI读取)
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
    //是否按了闪避键
    public bool ConsumeDodgePressed()
    {
        bool pressed = _dodgeQueued;
        _dodgeQueued = false;
        return pressed;
    }
    //受击/死亡缓冲消费（测试用）
    public bool ConsumeHit()
    {
        bool pressed = _hitQueued;
        _hitQueued = false;
        return pressed;
    }
    public bool ConsumeDeath()
    {
        bool pressed = _deathQueued;
        _deathQueued = false;
        return pressed;
    }
    //驱动格挡动画
    public void SetBlock(bool isBlocking)
    {
        _animator.SetBool("Block", isBlocking);
        _isBlocking = isBlocking;//记录格挡状态(受击结算时免伤)
    }
    //设置无敌帧状态
    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }
    //触发闪避
    public void SetDodgeTrigger()
    {
        _animator.SetTrigger("Dodge");
    }
    //触发受击动画
    public void SetHitTrigger()
    {
        _animator.SetTrigger("Hit");
    }
    //触发死亡动画
    public void SetDeathTrigger()
    {
        _animator.SetTrigger("Death");
    }
    //受击入口
    public void TriggerHit()
    {
        if (IsInvincible || _isDead) return;//无敌帧、已死亡：不响应
        _stateMachine.ChangeState(new PlayerHitState(this, _stateMachine));
    }
    //死亡入口
    public void TriggerDeath()
    {
        if (_isDead) return;
        _isDead = true;
        //冻结物理：尸体不受重力/碰撞干扰(防浮空漂移)，并关闭碰撞体(尸体不挡路)
        _rb.velocity = Vector3.zero;
        _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        _stateMachine.ChangeState(new PlayerDeathState(this,_stateMachine));
    }
    //伤害结算入口（实现 IDamageable）：所有对玩家的伤害都从这里进
    public AttackResult TakeDamage(float damage)
    {
        if (_isDead) return AttackResult.Ignored;        //已经死了：无效
        if (_isInvincible) return AttackResult.Ignored;  //闪避无敌帧：无效
        if (_isBlocking) return AttackResult.Blocked;    //格挡中：免伤
        _currentHp -= damage;
        if (_currentHp <= 0f)
        {
            _currentHp = 0f;
            TriggerDeath();
        }
        else
        {
            TriggerHit();
        }
        return AttackResult.Hit;
    }
    //锁定切换：已锁定->解锁；未锁定->锁定最近的存活敌人
    public void ToggleLock()
    {
        if (_isDead) return;//死亡后不响应
        if (_lockTarget != null)
        {
            _lockTarget = null;
            return;
        }
        _lockTarget = FindNearestEnemy();
    }
    //搜索范围内最近的存活敌人
    private EnemyController FindNearestEnemy()
    {
        EnemyController nearest = null;
        float nearestDist = lockRange;
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            if (enemy.IsDead) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
    //锁定中：平滑转向锁定目标（供各状态调用）；未锁定返回false
    public bool RotateTowardsLockTarget(float step)
    {
        if (_lockTarget == null) return false;
        Vector3 dir = _lockTarget.transform.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.0001f) return true;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, step);
        return true;
    }
    //锁定输入缓冲消费
    public bool ConsumeLock()
    {
        bool pressed = _lockQueued;
        _lockQueued = false;
        return pressed;
    }
    private void Awake()
    {
        //获取同一物体上的组件
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputHandler>();
        _animator = GetComponent<Animator>();
        _currentHp = maxHp;//初始化生命
        _input.OnAttackPressed += () => _attackQueued = true;
        _input.OnDodgePressed += () => _dodgeQueued = true;
        _input.OnHitPressed += () => _hitQueued = true;
        _input.OnDeathPressed += () => _deathQueued = true;
        _input.OnLockPressed += () => _lockQueued = true;
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
        if (ConsumeLock()) ToggleLock();//锁定切换
        //锁定目标死亡：自动解除锁定
        if (_lockTarget != null && _lockTarget.IsDead) _lockTarget = null;
        if (ConsumeDeath())
        {
            TriggerDeath();
        }else if (ConsumeHit())
        {
            TriggerHit();
        }
        _stateMachine.Update();
    }
}
