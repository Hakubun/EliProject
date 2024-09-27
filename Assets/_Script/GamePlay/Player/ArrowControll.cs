using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowControll : MonoBehaviour
{
    public Transform player;
    public FixedJoystick directionJs;
    public float arrowRadius = 1.5f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    private void Update()
    {
        Vector2 direction = new Vector2(directionJs.Horizontal, directionJs.Vertical);

        if (direction.sqrMagnitude > 0.1f)
        {
            spriteRenderer.enabled = true;

            Vector2 targetPosition = (Vector2)player.position + direction.normalized * arrowRadius;

            transform.position = targetPosition;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90f));
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }

    public void SetupJoyStick(FixedJoystick _joystick)
    {
        directionJs = _joystick;
    }
}
