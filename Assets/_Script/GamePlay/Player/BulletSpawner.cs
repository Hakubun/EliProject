using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public float nextFireTime = 0f;
    public FixedJoystick directionJs;

    [SerializeField] PlayerAnimation _anim;

    void Update()
    {
        if (directionJs.Horizontal != 0 || directionJs.Vertical != 0)
        {
            _anim.setShootingStatus(true);
            _anim.SetAttackDirection(directionJs.Horizontal, directionJs.Vertical);
            
            if (Time.time > nextFireTime)
            {
                FireBullet();
                nextFireTime = Time.time + fireRate;
            }
        }
        else {
            _anim.setShootingStatus(false);
        }
    }

    void FireBullet()
    {
        Vector2 direction = new Vector2(directionJs.Horizontal, directionJs.Vertical).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * 10f;
        }
    }

    public void SetupJoyStick(FixedJoystick _joystick)
    {
        directionJs = _joystick;
    }


}
