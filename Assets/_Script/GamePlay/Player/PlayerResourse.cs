using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class PlayerResourse : MonoBehaviour
{
    public static PlayerResourse Instance;
    public int key;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void obtainKey()
    {
        key++;
    }

    public int GetKey() { return key; }
    public void useKey()
    {
        key--;
    }
}

