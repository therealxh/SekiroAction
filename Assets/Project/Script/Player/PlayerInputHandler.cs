using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls _controls;
    [SerializeField] private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;
    public event Action OnAttackPressed;//¹¥»÷
    public event Action OnBlockPressed;//¸ñµ²
    public event Action OnDodgePressed;//ÉÁ±Ü
    private void Awake()
    {
        _controls = new PlayerControls();//ÊµÀý»¯
        _controls.Gameplay.Attack.performed += _ => OnAttackPressed?.Invoke();
        _controls.Gameplay.Block.performed += _ => OnBlockPressed?.Invoke();
        _controls.Gameplay.Dodge.performed += _ => OnDodgePressed?.Invoke();
    }
    // Start is called before the first frame update
    private void OnEnable()
    {
        _controls.Enable();   
    }
    private void OnDisable()
    {
        _controls.Disable();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
    }
}
