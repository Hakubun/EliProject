using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossRangeAttackLogic : MonoBehaviour
{
    private float _dmg;
    public float delay = 2.0f;
    private GameObject player;
    private float moveSpeed = 7.0f;
    private Vector3 moveDirection;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyObject", delay);
    }


    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void SetupDMG(float dmg)
    {

        _dmg = dmg;
    }

    public void SetupDestination(Vector3 pos)
    {
        moveDirection = (pos - this.gameObject.transform.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {

            //other.gameObject.GetComponent<Player>().takeDamage(_dmg);
            Destroy(this.gameObject);

        }
    }

    void DestroyObject()
    {
        Destroy(this.gameObject);
    }
}
