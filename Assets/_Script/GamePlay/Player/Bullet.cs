using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private GameObject HitEffect;
    private int bulletDMG;
    // Start is called before the first frame update
    void Start()
    {
        bulletDMG = 100;
    }

    // Update is called once per frame
    void Update()
    {


    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     Debug.Log("hit " + other.gameObject.tag);

    //     if (other.gameObject.CompareTag("Enemy"))
    //     {
    //         Destroy(other.gameObject);
    //     }


    // }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("hit " + other.gameObject.tag);
        Instantiate(HitEffect, this.gameObject.transform.position, Quaternion.identity);
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            HitObstacle(other);

        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            Health health = other.GetComponent<Health>();
            health.GetHit(bulletDMG);
            DamageFlash damageFlash = other.GetComponent<DamageFlash>();
            damageFlash.CallDamageFlash();
        }
        else if (other.gameObject.CompareTag("EnemyBoss"))
        {
            BossHealth health = other.GetComponent<BossHealth>();
            health.GetHit(bulletDMG);
            DamageFlash damageFlash = other.GetComponent<DamageFlash>();
            damageFlash.CallDamageFlash();
        }
        Destroy(this.gameObject);
    }

    private void HitObstacle(Collider2D collision)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right);
        if (hit.collider != null)
        {
            Instantiate(HitEffect, hit.point, Quaternion.identity);
        }

        if (collision.gameObject.TryGetComponent<Item>(out Item item))
        {
            item.GetHit(1, this.gameObject);
        }
    }
}
