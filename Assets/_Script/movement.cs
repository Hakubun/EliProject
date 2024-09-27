using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Vector2 moveMent;

    public bool isGrounded;
    private Rigidbody2D rb;
    public GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {

        transform.Translate(moveMent * Time.deltaTime * moveSpeed);
    }

    /* private void FixedUpdate()
     {
         float moveInput = moveMent.x * Time.deltaTime * moveSpeed;
         Vector3 newPosition = player.transform.position;
         newPosition.x += moveInput;
         player.transform.position = newPosition;
         //Vector2 movement = new Vector2(moveInput * moveSpeed, rb.velocity.y);
         //rb.velocity = movement;
     }*/

    public void move(int input)
    {
        Debug.Log(input);
        if (input == 1)
        {
            moveMent = Vector2.right;
        }
        else if (input == -1)
        {
            moveMent = Vector2.left;
        }
        else
        {
            moveMent = Vector2.zero;
        }

    }

    public void jump()
    {
        if (isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }



}
