using System;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls _controls;
    [SerializeField] private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;
    public event Action OnAttackPressed;//攻击
    public event Action OnBlockPressed;//格挡
    public event Action OnDodgePressed;//闪避
    public event Action OnHitPressed;//受击（测试用）
    public event Action OnDeathPressed;//死亡（测试用）
    public event Action OnLockPressed;//锁定切换
    //是否按住格挡
    public bool IsBlockHeld => _controls.Gameplay.Block.IsPressed();
    private void Awake()
    {
        _controls = new PlayerControls();//实例化
        _controls.Gameplay.Attack.performed += _ => OnAttackPressed?.Invoke();
        _controls.Gameplay.Block.performed += _ => OnBlockPressed?.Invoke();
        _controls.Gameplay.Dodge.performed += _ => OnDodgePressed?.Invoke();
        _controls.Gameplay.Hit.performed += _ => OnHitPressed?.Invoke();
        _controls.Gameplay.Death.performed += _ => OnDeathPressed?.Invoke();
        _controls.Gameplay.Lock.performed += _ => OnLockPressed?.Invoke();
    }
    private void OnEnable()
    {
        _controls.Enable();
    }
    private void OnDisable()
    {
        _controls.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        _moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
    }
}
