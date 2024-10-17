using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    [SerializeField] Animator anim;
    [SerializeField] bool isShooting;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInteger(string _param, int _id)
    {
        anim.SetInteger(_param, _id);
    }

    public void setShootingStatus(bool status)
    {
        isShooting = status;
    }

    public void SetMovementDir(float x, float y)
    {
        if (!isShooting)
        {
            if (x == 0 && y == 0)
            {
                anim.SetInteger("direction", 0);  // 0 for idle
            }
            else if (y > 0)
            {
                anim.SetInteger("direction", 1);  // 1 for up
            }
            else if (y < 0)
            {
                anim.SetInteger("direction", 2);  // 2 for down
            }
            else if (x > 0)
            {
                anim.SetInteger("direction", 3);  // 3 for right
            }
            else if (x < 0)
            {
                anim.SetInteger("direction", 4);  // 4 for left
            }
        }
    }

    public void SetAttackDirection(float x, float y)
    {
        if (x == 0 && y == 0)
        {
            anim.SetInteger("direction", 0);  // 0 for idle
        }
        else if (y > 0 && -0.5f <= x && x <= 0.5f)
        {
            anim.SetInteger("direction", 1);  // 1 for up
        }
        else if (y < 0 && -0.5f <= x && x <= 0.5f)
        {
            anim.SetInteger("direction", 2);  // 2 for down
        }
        else if (x > 0)
        {
            anim.SetInteger("direction", 3);  // 3 for right
        }
        else if (x < 0)
        {
            anim.SetInteger("direction", 4);  // 4 for left
        }
    }
}
