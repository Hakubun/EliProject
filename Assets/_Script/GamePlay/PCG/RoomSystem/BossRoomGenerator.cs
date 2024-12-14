using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossRoomGenerator : MonoBehaviour
{
    [SerializeField]
    private int roomLength, roomHeight;
    [SerializeField]
    private TilemapVisualizer tilemapVisualizer = null;

    //Events
    public UnityEvent<DungeonData> OnBossDungeonReady;

    public List<GameObject> bossPrefabs;


    public void generateBossRoom()
    {
        tilemapVisualizer.Clear();
        DungeonData data = new DungeonData //initilaiz dungeondata to pass over once the dungeon is created
        {
            
        };
        OnBossDungeonReady?.Invoke(data);
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int col = 0; col < roomHeight; col++) //instead of from zero we start from the offset value
        {
            for (int row = 0; row < roomLength; row++)
            {
                Vector2Int position = new Vector2Int(col, row);
                floor.Add(position);
            }
        }
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        //boss spawn
        Vector2Int roomCenter = new Vector2Int(roomLength / 2, roomHeight / 2);
        placeBoss(roomCenter);
        //player spawn
        Vector2Int playerSpawn = new Vector2Int(roomLength / 2, 1);
        GameManager.instance.SpawnPlayer(playerSpawn + new Vector2(0.5f, 0.5f));
    }

    private GameObject placeBoss(Vector2Int roomCenter)
    {
        int currentLevel = GameManager.instance.currentLevel;
        int bossIndex = (currentLevel / 5) % bossPrefabs.Count;

        GameObject bossPrefab = bossPrefabs[bossIndex];

        if (bossPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(roomCenter.x, roomCenter.y, 0);
            GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            return boss;
        }

        return null;

    }
}
