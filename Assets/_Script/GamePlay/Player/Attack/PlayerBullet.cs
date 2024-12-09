using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    private float dmg;
    [SerializeField] private float speed;
    private Vector2 dir;

    //hit effect list


    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)dir * Time.deltaTime * speed;
    }

    public void SetDirection(Vector2 _dir)
    {
        dir = _dir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetDamage(float _dmg)
    {
        dmg = _dmg;
    }
}
