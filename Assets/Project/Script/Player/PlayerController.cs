using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;//移速
    [SerializeField] private float rotateSpeed = 10f;//转向速度
    [SerializeField] private float acceleration = 15f;   // 单位：米/秒²，每秒速度增加 15
    private Vector2 _lastInput;//上一次输入
    private Vector3 _intentDir;//移动意图方向快照
    private Rigidbody _rd; //刚体引用
    private PlayerInputHandler _input; //输入引用
    private Animator _animator;//动画引用
    private void Awake()
    {
        //获取同一物体上的组件
        _rd = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputHandler>();
        _animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector2 input = _input.MoveInput;//获取移动意图
        //输入方向变化才改变意图方向（转身期间保持目标不变）
        if(input != _lastInput)
        {
            _lastInput = input;
            if (input != Vector2.zero) {
                _intentDir = Camera.main.transform.TransformDirection(input.x, 0, input.y);
                _intentDir.y = 0;
                _intentDir.Normalize();
            }
            else
            {
                _intentDir = Vector3.zero;
            }
        }
        Vector3 targetVelocity = new Vector3(moveSpeed * _intentDir.x, _rd.velocity.y, moveSpeed * _intentDir.z);
        _rd.velocity = Vector3.MoveTowards(_rd.velocity,targetVelocity,acceleration*Time.deltaTime);  
        if(_intentDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_intentDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        Vector3 horizontalVelocity = new Vector3(_rd.velocity.x, 0, _rd.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude / moveSpeed;//将速度映射到0-1区间
        _animator.SetFloat("Speed",currentSpeed,0.1f,Time.deltaTime);
    }
    void Update()
    {
        
    }
}
