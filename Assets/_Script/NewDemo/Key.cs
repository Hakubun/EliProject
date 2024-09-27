using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerResourse.Instance.obtainKey();
            destroyKey();
        }
    }
    public void destroyKey()
    {
        Destroy(gameObject);
    }
}
