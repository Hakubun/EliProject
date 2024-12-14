using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOutMethodGeneration : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4; // Minimum room dimensions
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20; // Dimensions for each quadrant
    [SerializeField]
    private int StartRoomWidth = 5, StartRoomHeight = 5; // Center room dimensions
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1; // Offset to prevent rooms from connecting without corridors
    [SerializeField]
    private bool randomWalkRooms = false; // Use random walk to generate rooms?

    protected override void RunProceduralGeneration()
    {
        CreateTopLeft();
        CreateTopRight();
        CreateBotLeft();
        CreateBotRight();
    }

    private void CreateTopLeft()
    {
        Vector3Int min = new Vector3Int(
            startPosition.x - dungeonWidth,
            startPosition.y,
            0
        );
        Vector3Int size = new Vector3Int(
            dungeonWidth - StartRoomWidth,
            dungeonHeight - StartRoomHeight,
            0
        );

        GenerateRooms(min, size);
    }

    private void CreateTopRight()
    {
        Vector3Int min = new Vector3Int(
            startPosition.x + StartRoomWidth,
            startPosition.y,
            0
        );
        Vector3Int size = new Vector3Int(
            dungeonWidth - StartRoomWidth,
            dungeonHeight - StartRoomHeight,
            0
        );

        GenerateRooms(min, size);
    }

    private void CreateBotLeft()
    {
        Vector3Int min = new Vector3Int(
            startPosition.x - dungeonWidth,
            startPosition.y - dungeonHeight,
            0
        );
        Vector3Int size = new Vector3Int(
            dungeonWidth - StartRoomWidth,
            dungeonHeight - StartRoomHeight,
            0
        );

        GenerateRooms(min, size);
    }

    private void CreateBotRight()
    {
        Vector3Int min = new Vector3Int(
            startPosition.x + StartRoomWidth,
            startPosition.y - dungeonHeight,
            0
        );
        Vector3Int size = new Vector3Int(
            dungeonWidth - StartRoomWidth,
            dungeonHeight - StartRoomHeight,
            0
        );

        GenerateRooms(min, size);
    }

    private void GenerateRooms(Vector3Int min, Vector3Int size)
    {
        // Create rooms using Binary Space Partitioning
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt(min, size),
            minRoomWidth,
            minRoomHeight
        );

        // Generate floor tiles
        HashSet<Vector2Int> floor = randomWalkRooms
            ? CreateRoomsRandomly(roomsList)
            : CreateSimpleRooms(roomsList);

        // Find room centers
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        // Connect rooms with corridors
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        // Paint tiles and generate walls
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var roomBounds in roomsList)
        {
            var roomCenter = new Vector2Int(
                Mathf.RoundToInt(roomBounds.center.x),
                Mathf.RoundToInt(roomBounds.center.y)
            );
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) &&
                    position.x <= (roomBounds.xMax - offset) &&
                    position.y >= (roomBounds.yMin + offset) &&
                    position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
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

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y)
        {
            position += (destination.y > position.y) ? Vector2Int.up : Vector2Int.down;
            corridor.Add(position);
        }

        while (position.x != destination.x)
        {
            position += (destination.x > position.x) ? Vector2Int.right : Vector2Int.left;
            corridor.Add(position);
        }

        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }
}
