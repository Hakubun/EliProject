using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public Vector3 offset;
    public float smoothSpeed = 0.125f;
    public Transform player;

    public bool follow;

    private void Awake()
    {
        follow = false;
    }

    private void Start()
    {
        locatePlayer(player.gameObject);
    }

    void Update()
    {
        if (follow)
        {

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, player.position + offset, smoothSpeed);
            transform.position = smoothedPosition;

            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    public void locatePlayer(GameObject target)
    {
        player = target.transform;
        follow = true;
    }
}
