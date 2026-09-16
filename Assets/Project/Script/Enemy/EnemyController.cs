using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private float moveSpeed = 2.5f;//移动速度
    [SerializeField] private float detectRange = 8f;//索敌半径
    [SerializeField] private float attackRange = 1.8f;//攻击半径
    [SerializeField] private float attackCooldown = 1.5f;//攻击间隔
    [SerializeField] private float rotateSpeed = 8f;//转向速度
    [SerializeField] private float maxHp = 100f;//最大生命
    [SerializeField] private float attackDamage = 15f;//攻击力
    [SerializeField] private float respawnDelay = 4f;//死亡后重生延时(秒)
    [SerializeField] private float maxPosture = 100f;//最大架势值
    [SerializeField] private float posturePerHit = 50f;//每次受击增加的架势
    [SerializeField] private float postureBreakDamage = 20f;//架势打满时的破防附加伤害

    private Transform _player;//玩家引用
    private EnemyStateMachine _stateMachine;
    private Rigidbody _rb;
    private Animator _animator;
    private float _lastAttackTime = -10f;//上次攻击时间(冷却起点)
    private bool _isDead;//死亡标志
    private float _currentHp;//当前生命
    private Collider _collider;//碰撞体引用
    private Vector3 _spawnPos;//出生点位置
    private Quaternion _spawnRot;//出生点朝向
    private float _posture;//当前架势值
    private float _lastPostureTime = -10f;//上次架势变化时间(衰减计时)
    private const float PostureDecayDelay = 3f;//受击后开始衰减的间隔(秒)
    private const float PostureDecaySpeed = 20f;//架势衰减速度(每秒)

    //上下文属性
    public Rigidbody Rb => _rb;
    public Animator Animator => _animator;
    public Transform Player => _player;
    public float MoveSpeed => moveSpeed;

    //与玩家的距离
    public float DistanceToPlayer => Vector3.Distance(transform.position, _player.position);
    //是否在索敌/攻击范围内
    public bool PlayerInDetectRange => DistanceToPlayer <= detectRange;
    public bool PlayerInAttackRange => DistanceToPlayer <= attackRange;
    public float RotateSpeed => rotateSpeed;
    public bool IsDead => _isDead;
    public float AttackDamage => attackDamage;
    public float CurrentHp => _currentHp;//当前生命(UI读取)
    public float MaxHp => maxHp;//最大生命(UI读取)
    public float CurrentPosture => _posture;//当前架势(UI读取)
    public float MaxPosture => maxPosture;//最大架势(UI读取)
    //冷却是否结束(距离够+冷却好才能再次攻击)
    public bool CanAttack => Time.time - _lastAttackTime > attackCooldown;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        _player = FindObjectOfType<PlayerController>().transform;
        _currentHp = maxHp;//初始化生命
        _spawnPos = transform.position;//记录出生点
        _spawnRot = transform.rotation;
    }
    //记录攻击时间(作为冷却起点)
    public void RecordAttackTime()
    {
        _lastAttackTime = Time.time;
    }
    //触发攻击/受击/死亡动画
    public void SetAttackTrigger()
    {
        _animator.SetTrigger("Attack");
    }
    public void SetHitTrigger()
    {
        _animator.SetTrigger("Hit");
    }
    public void SetDeathTrigger()
    {
        _animator.SetTrigger("Death");
    }
    //受击入口
    public void TriggerHit()
    {
        if (_isDead) return;
        _stateMachine.ChangeState(new EnemyHitState(this, _stateMachine));
    }
    //死亡入口
    public void TriggerDeath()
    {
        if (_isDead) return;
        _isDead = true;
        //冻结物理：尸体稳定躺地(防浮空漂移)，关闭碰撞体(尸体不挡路)
        _rb.velocity = Vector3.zero;
        _rb.isKinematic = true;
        _collider.enabled = false;
        _stateMachine.ChangeState(new EnemyDeathState(this, _stateMachine));
        Invoke(nameof(Respawn), respawnDelay);//定时自动重生(测试循环用)
    }
    //重生：复位位置/血量/物理/动画，回到待机
    private void Respawn()
    {
        _isDead = false;
        _currentHp = maxHp;
        _posture = 0f;//清空架势
        transform.SetPositionAndRotation(_spawnPos, _spawnRot);
        _rb.isKinematic = false;//恢复物理
        _rb.velocity = Vector3.zero;
        _collider.enabled = true;//恢复碰撞
        _animator.Rebind();//重置Animator：清除死亡状态的定格
        _animator.Update(0f);
        _stateMachine.ChangeState(new EnemyIdleState(this, _stateMachine));
    }
    //伤害结算入口(实现 IDamageable)：所有对敌人的伤害都从这里进
    public AttackResult TakeDamage(float damage)
    {
        if (_isDead) return AttackResult.Ignored;//已经死了：无效
        _currentHp -= damage;
        _lastPostureTime = Time.time;//刷新架势衰减计时
        _posture += posturePerHit;//受击积累架势
        if (_posture >= maxPosture)
        {
            //架势打满：破防——追加伤害并清空架势
            _posture = 0f;
            _currentHp -= postureBreakDamage;
        }
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
    private void Start()
    {
        _stateMachine = new EnemyStateMachine();
        _stateMachine.ChangeState(new EnemyIdleState(this, _stateMachine));
    }
    private void FixedUpdate()
    {
        //架势衰减：受击后静置一段时间开始回落
        if (!_isDead && _posture > 0f && Time.time - _lastPostureTime > PostureDecayDelay)
        {
            _posture = Mathf.Max(0f, _posture - PostureDecaySpeed * Time.fixedDeltaTime);
        }
        _stateMachine.Update();
    }
}
