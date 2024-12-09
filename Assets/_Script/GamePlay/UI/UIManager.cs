using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject MobileObjects;
    // Start is called before the first frame update
    void Start()
    {
        if (PlatformManager.Instance.CheckPlatform() == 2 || PlatformManager.Instance.CheckPlatform() == 3)
        {
            MobileObjects.SetActive(true);
        } 
        else
        {
            MobileObjects.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
