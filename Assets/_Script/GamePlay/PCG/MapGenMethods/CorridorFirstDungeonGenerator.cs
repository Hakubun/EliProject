using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    //PCG parameters
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5; //length: how long the corrido will be for one iteration. Count: how many iteration to run
    [SerializeField]
    [Range(0.1f, 1)]
    private float roomPercent = 0.8f; //percent of room we create

    public bool WideCorridor;

    //PCG Data
    //storing room into a dictionary for access
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary
        = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    //store each floor position and corridorposition for later access, hashset will remove duplicate and faster access
    private HashSet<Vector2Int> floorPositions, corridorPositions;

    //Gizmos Data
    private List<Color> roomColors = new List<Color>();
    [SerializeField]
    private bool showRoomGizmo = false, showCorridorsGizmo;

    //Events
    public UnityEvent<DungeonData> OnDungeonFloorReady;

    //overallfunction for outside script to call
    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration(); //running corridorFirstGeneration function
        DungeonData data = new DungeonData //initilaiz dungeondata to pass over once the dungeon is created
        {
            roomsDictionary = this.roomsDictionary, //this is the dictionary of a bunch of room where the key being the center point of the room and value being all the vecter2 position of room
            corridorPositions = this.corridorPositions, //once dungeon generated, pass the corridorPosition hashset
            floorPositions = this.floorPositions //once dungeon generated, pass the floorPosition hashset
        };
        OnDungeonFloorReady?.Invoke(data); //pass the data through unityevent
    }

    private void CorridorFirstGeneration()
    {
        floorPositions = new HashSet<Vector2Int>(); //initilize hashset for storing vector2 position
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>(); //create a hashset to store position for potential rooms

        CreateCorridors(floorPositions, potentialRoomPositions); //pass both hashset to the function
        GenerateRooms(potentialRoomPositions); //from the Create corridor function we got a hashset of position for room so we can now pass it in to create room
        //StartCoroutine(GenerateRoomsCoroutine(potentialRoomPositions));
    }

    private void ExpandCorridors()
    {
        foreach (var corridorPosition in corridorPositions)
        {
            floorPositions.UnionWith(Direction2D.GetEightDirection(corridorPosition));
        }
    }

    private void GenerateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions); //create a hashset of room position

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions); //pass in the floorPosition hashset before it is merged with all the room we created, so at this point the floorPosition only storing position of the corridors

        CreateRoomsAtDeadEnd(deadEnds, roomPositions); //since we got the list of deadend position, we can create a room at each deadend

        floorPositions.UnionWith(roomPositions); //"merge the room position and corridor position for tile painting
        if (WideCorridor)
        {
            ExpandCorridors();
        }
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        //tilemapVisualizer.PaintPotentialRoom(potentialRoomPositions);

    }

    private IEnumerator GenerateRoomsCoroutine(HashSet<Vector2Int> potentialRoomPositions)
    {
        yield return new WaitForSeconds(2);
        tilemapVisualizer.Clear();
        GenerateRooms(potentialRoomPositions);
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions
        };
        OnDungeonFloorReady?.Invoke(data);
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if (roomFloors.Contains(position) == false) //this is a check to see if the position in the deadend list already have a room created, hense pass over the roomFloors list which is a list of room we created earlier
            {
                var room = RunRandomWalk(randomWalkParameters, position); //once we are sure there is no room created at the position, we run the randomwalk function to create a room
                SaveRoomData(position, room);
                roomFloors.UnionWith(room); //adding the created room into the roomFloors
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>(); //create a list to store deadends
        foreach (var position in floorPositions)
        //so this for each loop will check every single one of the vector2 position aka the corridor position to find the "deadend" in this case the vector2 position that only has one neighbour position stored in the floorposition list
        {
            int neighboursCount = 0; //this is just a counter
            foreach (var direction in Direction2D.cardinalDirectionsList) //check four direction of the position to see if that position is stored in the hashset, there should be at least 1 position that is in the hashset
            {
                if (floorPositions.Contains(position + direction))
                    neighboursCount++;

            }
            if (neighboursCount == 1) //if there's only one neighbour position in the list, that means the position is a deadend
                deadEnds.Add(position); //adding the position to deadend list
        }
        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent); //decide how many rooms we should generate

        //this line below will randomly pick the "roomToCreateCount" amount of room position from the potentialRoomPositions and store it in a list for creation
        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList(); //randomize the potentialroom position list
        // OrderBy() = orders the elements base on what goes into the ()
        // Guid.NewGuid() = generates a new globally unique identifier
        // .Take(roomToCreateCount) = take "roomToCreateCount" amount of element from the potentialRoomPositions that is rearranged by OrderBy() function
        // ToList() = convert the elements we took from previous function to list 

        ClearRoomData(); //clean up the dictionary data that we will pass over later
        foreach (var roomPosition in roomsToCreate)
        {
            //since we already got the potential room list, we can run the randomwalk function at the potential room position to create a random shape room
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);

            SaveRoomData(roomPosition, roomFloor); //room position is the local var of a vector2Int and roomFloor is the hashset of vector2
            roomPositions.UnionWith(roomFloor); //adding the room space to roomposition hashset - all the rooms
        }
        //tilemapVisualizer.PaintActualRoom(roomsToCreate);
        return roomPositions;
    }

    private void ClearRoomData()
    {
        roomsDictionary.Clear();
        roomColors.Clear();
    }

    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
        roomColors.Add(UnityEngine.Random.ColorHSV());
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition; //setup the starting point
        potentialRoomPositions.Add(currentPosition);//add the current position into the potentialroom position hashset

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(floorPositions, currentPosition, corridorLength); //this will return a list of position that is a corridor
            currentPosition = corridor[corridor.Count - 1]; //get the last position of the corridor and set it as next starting point
            potentialRoomPositions.Add(currentPosition); //each end of the corridor will be stored as a potential room position
            floorPositions.UnionWith(corridor); //add the corridor list as a hashset
        }
        corridorPositions = new HashSet<Vector2Int>(floorPositions); //add all the "floor position" into a new hashset as corridor position hashset
    }

    private void OnDrawGizmosSelected()
    {
        if (showRoomGizmo)
        {
            int i = 0;
            foreach (var roomData in roomsDictionary)
            {
                Color color = roomColors[i];
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawSphere((Vector2)roomData.Key, 0.5f);
                foreach (var position in roomData.Value)
                {
                    Gizmos.DrawCube((Vector2)position + new Vector2(0.5f, 0.5f), Vector3.one);
                }
                i++;
            }
        }
        if (showCorridorsGizmo && corridorPositions != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var corridorTile in corridorPositions)
            {
                Gizmos.DrawCube((Vector2)corridorTile + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }
}
