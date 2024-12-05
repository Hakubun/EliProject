using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed;
    private Vector2 _movement;
    bool isShooting = false;
    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _facehorizontal = "FaceHorizontal";
    private const string _facevertical = "FaceVertical";

    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator _animator;
    
    [SerializeField] PlayerAnimation _anim;
    [Header("PC Debug")]
    public bool keyboardInput; //this is for debug
    private
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _animator.SetFloat(_facevertical, -1);
        

    }

    // Update is called once per frame
    void Update()
    {
        // Vector2 velocity;
        // if (keyboardInput)
        // {
        //     velocity = new Vector2(Input.GetAxisRaw("Horizontal") * playerSpeed, Input.GetAxisRaw("Vertical") * playerSpeed);
        // }
        // else
        // {
        //     velocity = new Vector2(js.Horizontal * playerSpeed, js.Vertical * playerSpeed);
        // }

        // rb.velocity = velocity;

        // _anim.SetMovementDir(velocity.x, velocity.y);

        _movement.Set(InputManager.Movement.x, InputManager.Movement.y);

        rb.velocity = _movement * playerSpeed;

        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);

        float normalizedX = (InputManager.Face.x / Screen.width) * 2 - 1;
        float normalizedY = (InputManager.Face.y / Screen.height) * 2 - 1;
        if (_movement != Vector2.zero)
        {
            _animator.SetFloat(_facehorizontal, _movement.x);
            _animator.SetFloat(_facevertical, _movement.y);
        }
        if (InputManager.shooting > 0 && _movement == Vector2.zero) {
            
            _animator.SetFloat(_facehorizontal, normalizedX);
            _animator.SetFloat(_facevertical, normalizedY);
        }
        if (InputManager.shooting > 0 && _movement != Vector2.zero)
        {
            _animator.SetFloat(_horizontal, normalizedX);
            _animator.SetFloat(_vertical, normalizedY);
        }
        else {
            _animator.SetFloat(_facehorizontal, normalizedX);
            _animator.SetFloat(_facevertical, normalizedY);
        }
        


    }

    // public void SetUpJoyStick(FixedJoystick _js)
    // {
    //     js = _js;
    // }

    public void UpdateSpeed(float newSpeed) => playerSpeed = newSpeed;


}
