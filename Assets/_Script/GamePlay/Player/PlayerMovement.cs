using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed;
    private Vector2 _movement;
    private Vector2 _face;
    private Vector2 _aim;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";

    private const string _facehorizontal = "FaceHorizontal";
    private const string _facevertical = "FaceVertical";

    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator _animator;

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

        _movement.Set(InputManager.Movement.x, InputManager.Movement.y);
        _face.Set(InputManager.Face.x, InputManager.Face.y);
        _aim.Set(InputManager.Aim.x, InputManager.Aim.y);

        rb.velocity = _movement * playerSpeed;

        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);


        if (_movement != Vector2.zero)
        {
            _animator.SetFloat(_facehorizontal, _movement.x);
            _animator.SetFloat(_facevertical, _movement.y);

            if (InputManager.shooting > 0)
            {
                if (_face == Vector2.zero)
                {
                    Vector2 NormalizedAim = NormalizeScreenXY(_aim);
                    _animator.SetFloat(_horizontal, NormalizedAim.x);
                    _animator.SetFloat(_vertical, NormalizedAim.y);
                }
                else
                {

                    _animator.SetFloat(_horizontal, _face.x);
                    _animator.SetFloat(_vertical, _face.y);
                }
            }
        }




    }

    // public void SetUpJoyStick(FixedJoystick _js)
    // {
    //     js = _js;
    // }

    public void UpdateSpeed(float newSpeed) => playerSpeed = newSpeed;

    private Vector2 NormalizeScreenXY(Vector2 Pos)
    {
        float normalizedX = (Pos.x / Screen.width) * 2 - 1;
        float normalizedY = (Pos.y / Screen.height) * 2 - 1;

        return new Vector2(normalizedX, normalizedY);
    }
}
