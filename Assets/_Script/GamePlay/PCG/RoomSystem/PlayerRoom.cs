using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRoom : RoomGenerator
{
    public GameObject player;

    public List<ItemPlacementData> itemData;

    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public override List<GameObject> ProcessRoom(
        Vector2Int roomCenter,
        HashSet<Vector2Int> roomFloor,
        HashSet<Vector2Int> roomFloorNoCorridors)
    {

        //this basically pass in the room floor and floor without corridors and return a dictionary of 2 type of vector2 hashset, one is a hashset of vector2 that has all 8 direction avaliable, or a hashset of vector2 that hase only up down left right 4 direction open
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        //this function will place all the items
        List<GameObject> placedObjects = prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        //set the player character spawn position in the center point of the room
        Vector2Int playerSpawnPoint = roomCenter;

        //spawning the player character
        //GameObject playerObject = prefabPlacer.CreateObject(player, playerSpawnPoint + new Vector2(0.5f, 0.5f));
        //Adding the player prefab into the list so when cleaning all object, we can clean delete it as well, 
        //? might wanna change this up so the player don't get deleted so we can save the player data when in scene but we can also do it through gamemanager or something else.
        GameManager.instance.SpawnPlayer(playerSpawnPoint + new Vector2(0.5f, 0.5f));
        //placedObjects.Add(playerObject);

        return placedObjects;
    }
}

public abstract class PlacementData
{
    [Min(0)]
    public int minQuantity = 0;
    [Min(0)]
    [Tooltip("Max is inclusive")]
    public int maxQuantity = 0;
    public int Quantity => UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
}

[Serializable]
public class ItemPlacementData : PlacementData
{
    public ItemData itemData;
}

[Serializable]
public class EnemyPlacementData : PlacementData
{
    public GameObject enemyPrefab;
    public Vector2Int enemySize = Vector2Int.one;
}

