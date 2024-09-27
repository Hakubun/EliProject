using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (PlayerResourse.Instance.GetKey() > 0)
            {
                OpenDoor();
                PlayerResourse.Instance.useKey();
            }
        }
    }
    public void OpenDoor()
    {
        Destroy(gameObject);
    }
    

}
