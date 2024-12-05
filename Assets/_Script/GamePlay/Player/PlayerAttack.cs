using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAnimation _anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         // Get mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;

        // Normalize the position between 0 and 1
        float normalizedX = (mousePosition.x / Screen.width) * 2 - 1;
        float normalizedY = (mousePosition.y / Screen.height) * 2 - 1;
        if(Input.GetMouseButton(0))
        {
            _anim.setShootingStatus(true);
            _anim.SetAttackDirection(normalizedX, normalizedY);
        }
        else
        {
            _anim.setShootingStatus(false);
        }
    }


}
