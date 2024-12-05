using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private AbstractDungeonGenerator generator;
    [SerializeField]
    private BossRoomGenerator bossRoomGenerator;


    public GameObject playerPref; //this is use to spawn player
    public GameObject portalPref;
    private GameObject player; //this is used to move once it's spawn
    public FixedJoystick movement;
    public FixedJoystick attack;

    public int currentLevel;

    [SerializeField]
    private CinemachineVirtualCamera _cam;

    private bool spawned;

    public UnityEvent OnStart;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        currentLevel = 1;
    }

    private void Start()
    {
        OnStart?.Invoke();
        
    }
    public void SpawnPlayer(Vector2 pos)
    {
        if (playerPref == null)
        {
            Debug.LogError("Player prefab is not assigned.");
            return;
        }

        if (!spawned)
        {
            player = Instantiate(playerPref, pos, Quaternion.identity);
            PlayerStats.SetLives(5);
            HUDManager.instance.UpdateHUD();

            spawned = true;
            if (_cam != null)
            {
                FocusCameraOnThePlayer(player.transform);
            }
            else
            {
                Debug.LogError("Camera controller (_cam) is not assigned.");
            }
        }
        else
        {
            player.transform.position = pos;
        }


        // Setup the player's joystick for movement (assuming `movement` is your joystick controller)
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        ArrowControll attackControl = player.GetComponentInChildren<ArrowControll>();
        BulletSpawner bulletSpawn = player.GetComponentInChildren<BulletSpawner>();
        if (playerMovement != null)
        {
            //playerMovement.SetUpJoyStick(movement);
            attackControl.SetupJoyStick(attack);
            bulletSpawn.SetupJoyStick(attack);
        }
        else
        {
            Debug.LogError("PlayerMovement component is missing on the player prefab.");
        }

    }

    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        _cam.LookAt = playerTransform;
        _cam.Follow = playerTransform;
    }
    
    public void nextLevel()
    {
        currentLevel++;
        if (currentLevel % 5 == 0)
        {
            Debug.Log("level boss");
            bossRoomGenerator.generateBossRoom();
        }
        else
        {
            generator.GenerateDungeon();
        }
    }

    public void spawnPortal(Vector3 pos)
    {
        GameObject portal = Instantiate(portalPref, pos, Quaternion.identity);
    }
}
