using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    protected int currentHealth, maxHealth;

    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    public UnityEvent<GameObject> OnHitWithReference, OnDeathWithReference;

    [SerializeField]
    protected bool isDead = false;

    protected virtual void Start()
    {
        maxHealth = 500;
        currentHealth = maxHealth;
        
    }

    public void InitializeHealth(int healthValue)
    {
        currentHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
    }

    public virtual void GetHit(int amount/*, GameObject sender*/)
    {
        if (isDead)
            return;
        /*if (sender.layer == gameObject.layer)
            return;*/
        spriteRenderer.transform.DOShakePosition(0.2f, 0.3f, 75, 1, false, true);

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            //OnHitWithReference?.Invoke(sender);

        }
        else
        {
            //OnDeathWithReference?.Invoke(sender);
            isDead = true;
            Destroy(gameObject);
        }
    }
}

