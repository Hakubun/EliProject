using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom, bossRoom;

    List<GameObject> spawnedObjects = new List<GameObject>();

    private Vector2Int playerRoomPos;

    [SerializeField]
    private GraphTest graphTest;


    public Transform itemParent;

    [SerializeField]
    private CinemachineVirtualCamera cinemachineCamera;

    public UnityEvent RegenerateDungeon;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in spawnedObjects)
            {
                Destroy(item);
            }
            RegenerateDungeon?.Invoke();
        }
    }
    public void GenerateRoomContent(DungeonData dungeonData) //once the dungeon is generated, calling this function
    {
        if (GameManager.instance.currentLevel % 5 == 0)
        {
            foreach (GameObject item in spawnedObjects) //remove all spawned objects to make sure the list is empty
            {
                Destroy(item);
            }
            spawnedObjects.Clear();
        }
        else
        {
            foreach (GameObject item in spawnedObjects) //remove all spawned objects to make sure the list is empty
            {
                Destroy(item);
            }
            spawnedObjects.Clear();

            SelectPlayerSpawnPoint(dungeonData);
            SelectEnemySpawnPoints(dungeonData);

            foreach (GameObject item in spawnedObjects)
            {
                if (item != null)
                    item.transform.SetParent(itemParent, false);
            }
        }
        
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        int randomRoomIndex = UnityEngine.Random.Range(0, dungeonData.roomsDictionary.Count); //randomly select a number between 0 to the amount of rooms created
        Vector2Int playerSpawnPoint = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex); //set the number selected as the room we will pick as player spawnroom

        graphTest.RunDijkstraAlgorithm(playerSpawnPoint, dungeonData.floorPositions); //this is a debug graph to check the distence of each room

        Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex); //pre-store the index so we don't have to write the long line when calling it later

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(playerSpawnPoint, //pass center point of the room
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex), //pass all the floor position of the room
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex) //pass all the floor position without corridor
            ); //create a list of gameObject to place in this room


        spawnedObjects.AddRange(placedPrefabs); //this is basiaclly adding all the elements in placedprefabs which is a list of objects into the list spawnedObjects list, combineing the list basically

        FindFurtherestRoom(playerSpawnPoint, dungeonData);

        dungeonData.roomsDictionary.Remove(playerSpawnPoint); //remove the player room from dictionary
    }


    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        {
            spawnedObjects.AddRange(
                defaultRoom.ProcessRoom(
                    roomData.Key,
                    roomData.Value,
                    dungeonData.GetRoomFloorWithoutCorridors(roomData.Key)
                    )
            );

        }
    }

    private void FindFurtherestRoom(Vector2Int playerSpawnPoint, DungeonData dungeonData)
    {
        // Run the Dijkstra algorithm to find the distances from the player spawn point to all other positions
        graphTest.RunDijkstraAlgorithm(playerSpawnPoint, dungeonData.floorPositions);

        // Initialize variables to keep track of the furthest room
        Vector2Int furthestRoomPosition = Vector2Int.zero;
        int maxDistance = int.MinValue;

        // Iterate through each room in the dungeon data dictionary
        foreach (var roomData in dungeonData.roomsDictionary)
        {
            Vector2Int roomPosition = roomData.Key;

            // Get the distance to the current room's position using the Dijkstra result
            int distanceToRoom = graphTest.DijkstraResult[roomPosition];

            // Check if this room is further than the currently recorded furthest room
            if (distanceToRoom > maxDistance)
            {
                maxDistance = distanceToRoom;
                furthestRoomPosition = roomPosition;
            }
        }

        // Set up the boss room at the furthest room position
        List<GameObject> placedBossRoomPrefabs = bossRoom.ProcessRoom(
            furthestRoomPosition,
            dungeonData.roomsDictionary[furthestRoomPosition],
            dungeonData.GetRoomFloorWithoutCorridors(furthestRoomPosition)
        );

        // Add the boss room prefabs to the list of spawned objects
        spawnedObjects.AddRange(placedBossRoomPrefabs);

        // Remove the boss room from the rooms dictionary to prevent it from being used elsewhere
        dungeonData.roomsDictionary.Remove(furthestRoomPosition);
    }
}
