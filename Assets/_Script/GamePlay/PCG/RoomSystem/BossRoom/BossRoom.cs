using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> enemyPlacementData;
    public List<ItemPlacementData> itemData;

    public List<GameObject> bossPrefabs;

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper =
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects =
            prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        placedObjects.AddRange(prefabPlacer.PlaceEnemies(enemyPlacementData, itemPlacementHelper));

        if(isBossRoom() && GameManager.instance.currentLevel > 1)
        {
            Debug.Log("boss");
        }
        else
        {
            GameObject boss = placeBoss(roomCenter);
            if (boss != null)
            {
                placedObjects.Add(boss);
            }
        }         

        return placedObjects;
    }

    private bool isBossRoom()
    {
        int currentLevel = GameManager.instance.currentLevel;
        return currentLevel % 5 == 0;
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
