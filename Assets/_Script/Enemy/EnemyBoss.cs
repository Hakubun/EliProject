using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum BossState
{
    Idle,
    Chasing,
    Attacking,
    Dying
}
public class EnemyBoss : MonoBehaviour
{
    public BossState currentState;
    public Transform player;
    public float chaseRange = 5.0f;
    public float attackRange = 1.5f;
    public float moveSpeed = 2.0f;
    public Transform attackPoint;
    public GameObject attackPrefab;

    [Header("Range Boss Option")]
    public bool rangeBoss;
    public GameObject bulletPrefab;


    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentState = BossState.Idle;
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        RotateToPlayer();
        switch (currentState)
        {
            case BossState.Idle:
                Idle();
                break;
            case BossState.Chasing:
                ChasePlayer();
                break;
            case BossState.Attacking:
                AttackPlayer();
                break;
            case BossState.Dying:
                Die();
                break;
        }
    }

    void Idle()
    {
        // Transition to Chasing state if player is within chase range
        if (Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            currentState = BossState.Chasing;
            animator.SetBool("Walking", true);
        }
    }

    void ChasePlayer()
    {
        if (rangeBoss)
        {
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                currentState = BossState.Attacking;
            }
            else
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                {
                    // Move towards the player
                    Vector2 direction = (player.position - transform.position).normalized;
                    rb.velocity = direction * moveSpeed;
                }
            }
        }
        else
        {
            if (Mathf.Abs(player.position.y - transform.position.y) <= 0.5)
            {
                currentState = BossState.Attacking;
            }
            else
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                {
                    if (player.position.x > transform.position.x)
                    {
                        // Face right
                        Vector2 direction = ((player.position + Vector3.left) - transform.position).normalized;
                        rb.velocity = direction * moveSpeed;

                    }
                    else if (player.position.x < transform.position.x)
                    {
                        // Face left
                        Vector2 direction = ((player.position + Vector3.right) - transform.position).normalized;
                        rb.velocity = direction * moveSpeed;
                    }
                }
            }
        }
    }

    void AttackPlayer()
    {
        rb.velocity = Vector2.zero; // Stop moving to attack
        animator.SetBool("Walking", false);
        Debug.Log(rb.velocity);
        if (true)
        {
            animator.SetTrigger("Attack");
        }
        if (rangeBoss)
        {
            // Return to chasing if the player moves out of attack range
            if (Vector2.Distance(transform.position, player.position) > attackRange)
            {

                currentState = BossState.Chasing;
                animator.SetBool("Walking", true);
            }
        }
        else
        {
            if (Mathf.Abs(player.position.y - transform.position.y) > 0.5)
            {
                currentState = BossState.Chasing;
                animator.SetBool("Walking", true);
            }
        }
    }

    void Die()
    {
        rb.velocity = Vector2.zero; // Stop any movement
        animator.SetTrigger("Die");
        // Additional logic for when the boss dies, e.g., dropping loot
        Destroy(gameObject, 2f); // Destroy the boss after 2 seconds
    }

    void RotateToPlayer()
    {
        Vector3 scale = transform.localScale;

        if (player.position.x > transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x); // Face right
        }
        else if (player.position.x < transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x); // Face left
        }

        transform.localScale = scale;
    }

    public void spawnAttackPrefab()
    {
        if (rangeBoss)
        {
            Vector2 direction = (player.transform.position - attackPoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
            bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            bullet.GetComponent<EnemyBossRangeAttackLogic>().SetupDestination(player.transform.position);


            //bullet.GetComponent<EnemyBossRangeAttackLogic>().SetupDMG(dmg);
        }
        else
        {
            GameObject AttackGO = Instantiate(attackPrefab, attackPoint.position, Quaternion.identity);
            //bullet.GetComponent<EnemyBossRangeAttackLogic>().SetupDMG(dmg);
            //AttackGO.transform.parent = attackPoint.transform;
            if (player.position.x < transform.position.x)
            {
                AttackGO.transform.Rotate(0f, 180f, 0f);
            }
            Destroy(AttackGO, 1f);
        }
    }


}

