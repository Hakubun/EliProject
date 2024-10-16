using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed;
    public Rigidbody2D rb;
    public FixedJoystick js;
    private SpriteRenderer spriteRenderer;
    [Header("PC Debug")]
    public bool keyboardInput; //this is for debug
    private
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity;
        if (keyboardInput)
        {
            velocity = new Vector2(Input.GetAxisRaw("Horizontal") * playerSpeed, Input.GetAxisRaw("Vertical") * playerSpeed);
        }
        else
        {
            velocity = new Vector2(js.Horizontal * playerSpeed, js.Vertical * playerSpeed);
        }

        rb.velocity = velocity;

        if (velocity.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (velocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    public void SetUpJoyStick(FixedJoystick _js)
    {
        js = _js;
    }

    public void UpdateSpeed(float newSpeed) => playerSpeed = newSpeed;


}
