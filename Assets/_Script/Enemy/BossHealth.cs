using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BossHealth : Health
{
    protected override void Start()
    {
        maxHealth = 1000;
        currentHealth = maxHealth;
    }

    public override void GetHit(int amount)
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
            //this.gameObject.GetComponent<DamageFlash>().CallDissolveEffect();
            GameObject _object = gameObject.transform.parent.gameObject;
            Vector3 portalSpawnPos = _object.transform.position;
            Destroy(_object);
            GameManager.instance.spawnPortal(portalSpawnPos);
        }
    }

    public void passSpawnAttackPrefab()
    {
        EnemyBoss enemyBoss = GetComponentInParent<EnemyBoss>();
        enemyBoss.spawnAttackPrefab();
        
    }
}
