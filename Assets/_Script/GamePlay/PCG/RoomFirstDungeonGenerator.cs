using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4; //set the minimal wide and heigh for single room
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20; //set the total space for spliting
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1; //prevent room being connected without corridor, this is basically a adjustment point so the room creation will start at the value added the offset instead of the actual position itself, example: starting point 0,0 -> 1,1 because the offset is 1 so we left a 2 empty space (1 from each room) out for corridor to connect room with
    [SerializeField]
    private bool randomWalkRooms = false; //if we want to use random walk algo to create room

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight); //pass in all the value

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>(); //create a hashset to store all the vector2int floor position

        if (randomWalkRooms) //this should be easy to understand
        {
            floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }


        List<Vector2Int> roomCenters = new List<Vector2Int>(); //create a list of room center position so we can connect them
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters); //from the foreach loop we went though each room and find the room center, start connect the dots for connection
        floor.UnionWith(corridors); //adding those position to floor so we can paint tiles on it

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++) //go through each room in rooms list
        {
            var roomBounds = roomsList[i]; //get the Bounds from each element which is 2 position: min and max
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y)); //find the center of the space
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter); //from the center position start random walk
            foreach (var position in roomFloor)
            {
                //this is to check if the random walk creating a space that is outside of the space we split 
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin + offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)]; //randomly pick a position from the roomCenters list
        roomCenters.Remove(currentRoomCenter); //remove the position from the list since we already pick the position

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters); //from the position we picked, find the closest point from this point
            roomCenters.Remove(closest); //remove that closest point since we already connected it
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest); //connect 2 points
            currentRoomCenter = closest; //overwrite the currentroomcenter to the closest point so we can find the next closest
            corridors.UnionWith(newCorridor); //adding the corridor to the list
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y) //start with y position, check if we need to move up or down
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x) //check x position to move left and right
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero; //setup closest as 0,0
        float distance = float.MaxValue; //set the distance to max value 
        foreach (var position in roomCenters) //this is basically a bubble sort to go through each position
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter); //compare 2 position
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++) //instead of from zero we start from the offset value
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }


}
