using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }
    [Header("Debug")]
    public bool manualOverWrite = false;
    public int platID = -1;
    //* 0 == Editor, 1 == PC, 2 == IOS, 3 == Android, 4 == other;


    private void Awake()
    {
        // Check if there is already an instance of PlatformManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy the duplicate
            return;
        }

        Instance = this; // Assign this instance
        DontDestroyOnLoad(gameObject); // Make it persistent across scenes
    }
    public int CheckPlatform()
    {
        if (!manualOverWrite)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    platID = 0; // Editor
                    break;

                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    platID = 1; // PC
                    break;

                case RuntimePlatform.IPhonePlayer:
                    platID = 2; // iOS
                    break;

                case RuntimePlatform.Android:
                    platID = 3; // Android
                    break;

                default:
                    platID = 4; // Other
                    break;
            }
        }

        Debug.Log($"Platform ID: {platID} ({Application.platform})");
        return platID;
    }
}
