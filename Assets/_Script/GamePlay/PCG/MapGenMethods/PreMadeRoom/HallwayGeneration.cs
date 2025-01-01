using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HallwayGeneration : MonoBehaviour
{
    private Vector2Int StartRoomCenter;
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> actualRoomPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> roomWallPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> startRoomConnectionPoints = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> fullFloorList = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> regularRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> bossRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> ConnectedRooms = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> ConnectedPoints = new HashSet<Vector2Int>();

    private HashSet<Vector2Int> hallWaySpace = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> MidPoints = new HashSet<Vector2Int>();
    public TilemapVisualizer tilemapVisualizer;
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap ConnectionPoints; // Optional: Visualize connection points
    public TileBase floorTile, wallUp, wallDown, wallLeft, wallRight;
    public TileBase connectionPointTile;

    private HashSet<Vector2Int> allWallPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> basicWallPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> cornerWallPositions = new HashSet<Vector2Int>();

    private HashSet<Vector2Int> temp = new HashSet<Vector2Int>();

    public float roomMaxDist = 10;
    public void GenerateHallway(RoomsData data)
    {
        // Initialize Data
        InitializeData(data);

        // Connect start room to the nearest regular room connection points
        ConnectStartRoom();

        ConnectBossRoom();

        ConnectRegularRooms();

        ExpandAllHallways();

        PatchWalls();

        GenerateWall();


    }

    private void InitializeData(RoomsData data)
    {
        StartRoomCenter = data.StartRoomCenter;
        startRoomConnectionPoints = new HashSet<Vector2Int>(data.startRoomConnectionPoints);
        regularRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>(data.regularRoomConnectionPoints);
        bossRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>(data.bossRoomConnectionPoints);
        occupiedPositions = new HashSet<Vector2Int>(data.occupiedPositions);
        actualRoomPositions = new HashSet<Vector2Int>(data.actualRoomPositions);
        roomWallPositions = new HashSet<Vector2Int>(data.roomWallPositions);

    }

    private void GenerateWall()
    {
        //wallTilemap.ClearAllTiles(); //clear all existing walls

        fullFloorList = GetTilePositions(floorTilemap); //get all position of floor

        allWallPositions = WallGenerator.FindWallsInDirections(fullFloorList, Direction2D.diagonalDirectionsList); //get all the postions that needs wall tile

        basicWallPositions = WallGenerator.FilterSimpleWalls(allWallPositions, fullFloorList, Direction2D.cardinalDirectionsList); //find wall that is on the side.

        temp = allWallPositions.Except(basicWallPositions).ToHashSet();

        cornerWallPositions = allWallPositions.Except(basicWallPositions).ToHashSet();

        cornerWallPositions = WallGenerator.FilterCornerWalls(cornerWallPositions, allWallPositions, Direction2D.cardinalDirectionsList);

        temp.ExceptWith(cornerWallPositions);

        WallGenerator.CreateBasicWall(tilemapVisualizer, basicWallPositions, fullFloorList);

        WallGenerator.CreateCornerWall(tilemapVisualizer, cornerWallPositions, allWallPositions, fullFloorList);

        WallGenerator.CreateInnerWal(tilemapVisualizer, temp, fullFloorList);

        // WallGenerator.CreateCornerWalls(tilemapVisualizer, allWallPositions, fullFloorList);
    }

    private void ConnectStartRoom()
    {
        foreach (var connectionPoint in startRoomConnectionPoints)
        {
            Vector2Int faceDir = GetConnectionDir(connectionPoint);

            Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir);

            if (closestRoomConnectionPoint != Vector2Int.zero)
            {
                DrawHallway(connectionPoint, closestRoomConnectionPoint);
                RemoveConnectionPoint(closestRoomConnectionPoint);
            }
        }
    }

    private void ConnectBossRoom()
    {
        foreach (var bossRoom in bossRoomConnectionPoints)
        {
            foreach (var connectionPoint in bossRoom.Value)
            {
                Vector2Int faceDir = GetConnectionDir(connectionPoint);
                if (bossRoom.Key != -faceDir)
                {
                    //patch the wall;
                    PatchConnectionPoint(connectionPoint);
                }
                else
                {
                    Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir);

                    if (closestRoomConnectionPoint != Vector2Int.zero)
                    {
                        DrawHallway(connectionPoint, closestRoomConnectionPoint);
                        RemoveConnectionPoint(closestRoomConnectionPoint);
                    }
                }
            }

        }
    }

    private void ConnectRegularRooms()
    {
        // Create a list to store keys to remove
        var keysToRemove = new List<Vector2Int>();

        foreach (var currentRoom in regularRoomConnectionPoints)
        {
            if (!ConnectedRooms.ContainsKey(currentRoom.Key))
            {
                ConnectedRooms.Add(currentRoom.Key, new HashSet<Vector2Int>());
            }

            foreach (var connectionPoint in currentRoom.Value)
            {
                if (!ConnectedPoints.Contains(connectionPoint))
                {


                    Vector2Int facing = GetConnectionDir(connectionPoint);

                    Vector2Int closestRoomConnectionPoint = FindRegularRoomConnectionPoint(currentRoom.Key, connectionPoint, facing);

                    if (closestRoomConnectionPoint != Vector2Int.zero)
                    {
                        ConnectedPoints.Add(connectionPoint);
                        DrawHallway(connectionPoint, closestRoomConnectionPoint);
                        RemoveConnectionPoint(closestRoomConnectionPoint);
                    }
                }
            }

            // If the current room has no remaining connection points, mark it for removal
            if (currentRoom.Value.Count == 0)
            {
                keysToRemove.Add(currentRoom.Key);
            }
        }

        // Remove keys after the loop
        foreach (var key in keysToRemove)
        {
            regularRoomConnectionPoints.Remove(key);
        }
    }

    private void PatchWalls()
    {
        foreach (var room in regularRoomConnectionPoints)
        {
            foreach (var connectionPoint in room.Value)
            {
                if (!ConnectedPoints.Contains(connectionPoint))
                {
                    PatchConnectionPoint(connectionPoint);
                }
            }
        }
    }



    private Vector2Int FindRegularRoomConnectionPoint(Vector2Int currentCenter, Vector2Int currentPoint, Vector2Int facingDirection)
    {
        Vector2Int closestPoint = Vector2Int.zero;
        Vector2Int ClosestRoom = Vector2Int.zero;
        float minDistance = float.MaxValue;
        float minRoomDist = float.MaxValue;
        currentPoint += facingDirection * 3;

        foreach (var room in regularRoomConnectionPoints)
        {
            float roomdistance = (currentPoint - room.Key).sqrMagnitude;
            if (roomdistance < minRoomDist)
            {
                minRoomDist = roomdistance;
            }
            foreach (var roomConnectionPoint in room.Value)
            {
                // Calculate vector from currentPoint to roomConnectionPoint
                Vector2Int directionToConnection = roomConnectionPoint - currentPoint;

                if (ConnectedPoints.Contains(roomConnectionPoint))
                    continue;                // Only consider points generally in the facing direction
                if (!CheckPosition(currentPoint, facingDirection, roomConnectionPoint)) //
                    continue;
                if (GetConnectionDir(roomConnectionPoint) == facingDirection)
                    continue;
                if (ConnectedRooms.ContainsKey(room.Key) && ConnectedRooms[room.Key].Contains(currentCenter))
                {
                    Debug.Log($"{room.Key} already connected with {currentCenter}");
                    continue;
                }
                float distance = (currentPoint - roomConnectionPoint).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = roomConnectionPoint;
                    ClosestRoom = room.Key;
                    //minRoomDist = roomdistance;

                }
            }
        }

        if (minDistance < minRoomDist)
        {
            if (!ConnectedRooms.ContainsKey(ClosestRoom))
            {
                ConnectedRooms.Add(ClosestRoom, new HashSet<Vector2Int> { currentCenter });
            }
            else
            {
                ConnectedRooms[ClosestRoom].Add(currentCenter);
            }
        }

        else
        {
            foreach (var space in hallWaySpace)
            {
                if (!CheckPosition(currentPoint, facingDirection, space))
                    continue;
                float distance = (currentPoint - space).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = space;
                }
            }
        }


        return closestPoint;

    }

    private Vector2Int FindClosestConnectionPoint(Vector2Int currentPoint, Vector2Int facingDirection)
    {
        Vector2Int closestPoint = Vector2Int.zero;
        float minDistance = float.MaxValue;

        foreach (var room in regularRoomConnectionPoints)
        {
            if (room.Value.Contains(currentPoint))
            {
                continue;
            }
            foreach (var roomConnectionPoint in room.Value)
            {
                // Calculate vector from currentPoint to roomConnectionPoint
                Vector2Int directionToConnection = roomConnectionPoint - currentPoint;

                // Only consider points generally in the facing direction
                if (Vector2.Dot(facingDirection, directionToConnection) <= 0)
                    continue;

                float distance = (currentPoint - roomConnectionPoint).sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = roomConnectionPoint;
                }
            }
        }

        return closestPoint;
    }

    private void DrawHallway(Vector2Int start, Vector2Int end)
    {

        //insteady of calling settile, actually store into a hashset
        // Extend from both sides by 3 tiles and connect in the middle
        Vector2Int currentStart = ExtendHallway(start, GetConnectionDir(start), 3);
        Vector2Int currentEnd = ExtendHallway(end, GetConnectionDir(end), 3);

        ConnectToMidpoint(currentStart, currentEnd);
    }

    private Vector2Int ExtendHallway(Vector2Int point, Vector2Int direction, int length)
    {
        for (int i = 0; i < length; i++)
        {
            point += direction;
            PlaceFloorTile(point);
        }
        return point;
    }

    private void ConnectToMidpoint(Vector2Int start, Vector2Int end)
    {
        // Vector2Int midpoint = CalculateMidpoint(start, end);
        // // ConnectionPoints.SetTile(new Vector3Int(midpoint.x, midpoint.y, 0), connectionPointTile);
        // MidPoints.Add(midpoint);

        //check if

        // Connect start to midpoint
        DrawPath(start, end);

        // Connect end to midpoint
        // DrawPath(end, midpoint);
    }

    private void DrawPath(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        bool preferX = Mathf.Abs(to.x - from.x) > Mathf.Abs(to.y - from.y); // Decide initial axis preference
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>(); // Track visited positions to prevent infinite loops
        int safetyCounter = 0; // Counter to prevent infinite loops
        int maxIterations = 1000; // Arbitrary limit to break out of potential infinite loops

        while (current != to)
        {
            if (safetyCounter++ > maxIterations)
            {
                Debug.LogWarning("Max iterations reached in DrawPath. Redirecting to nearest hallway space.");
                to = FindNearestHallwaySpace(current);
                if (to == Vector2Int.zero)
                {
                    Debug.LogError("No valid hallway space found. Aborting path generation.");
                    break; // No valid hallway space to redirect to
                }
                safetyCounter = 0; // Reset safety counter for the new target
            }

            Vector2Int nextStep = current;

            // Decide movement based on preferred axis
            if (preferX && current.x != to.x)
            {
                nextStep.x += current.x < to.x ? 1 : -1;
            }
            else if (!preferX && current.y != to.y)
            {
                nextStep.y += current.y < to.y ? 1 : -1;
            }
            else
            {
                // If preferred axis is fully traversed, switch to the other
                preferX = !preferX;
                continue;
            }

            // Check if the next step is blocked
            if (IsBlocked(nextStep))
            {
                Debug.LogWarning($"Blocked at {nextStep}. Switching axis.");
                preferX = !preferX; // Switch axis preference if blocked
                continue;
            }

            // Mark the current position as visited
            if (visitedPositions.Contains(nextStep))
            {
                Debug.LogWarning($"Pathfinding oscillation detected. Adjusting strategy at {nextStep}.");
                preferX = !preferX; // Adjust axis preference to avoid oscillation
                continue;
            }
            visitedPositions.Add(nextStep);

            // Move to the next step
            current = nextStep;

            // Place a floor tile at the current position
            PlaceFloorTile(current);
        }
    }





    [Range(0, 3)] // Slider in the Unity Editor
    public int blockCheckRange = 1; // Default range to 1

    private bool IsBlocked(Vector2Int position)
    {
        // Use the adjustable range from the editor
        int range = blockCheckRange;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int checkPosition = position + new Vector2Int(x, y);
                if (actualRoomPositions.Contains(checkPosition)) // Check if this position is occupied
                {
                    return true;
                }
            }
        }
        return false;
    }


    private Vector2Int FindNearestHallwaySpace(Vector2Int position)
    {
        Vector2Int nearestSpace = Vector2Int.zero;
        float minDistance = float.MaxValue;

        foreach (var space in hallWaySpace)
        {
            float distance = (position - space).sqrMagnitude; // Use squared distance for efficiency
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSpace = space;
            }
        }

        if (nearestSpace != Vector2Int.zero)
        {
            Debug.Log($"Redirecting to nearest hallway space at {nearestSpace}");
        }
        return nearestSpace;
    }




    private void RemoveConnectionPoint(Vector2Int connectionPoint)
    {
        ConnectedPoints.Add(connectionPoint);
    }


    private Vector2Int GetConnectionDir(Vector2Int point)
    {
        Vector2Int facingDir = Vector2Int.zero;

        Vector2Int[] directions = new Vector2Int[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        };

        int counter = 0;
        foreach (Vector2Int dir in directions)
        {
            if (!actualRoomPositions.Contains(point + dir)) // Corrected Contains method
            {
                counter += 1;
                facingDir = dir;
            }
        }

        if (counter != 1)
        {
            return Vector2Int.zero;
        }
        else
        {
            return facingDir;
        }

    }

    private bool CheckPosition(Vector2Int currentPoint, Vector2Int direction, Vector2Int connectionPoint)
    {
        bool result = false;
        if (direction == Vector2Int.up)
        {
            result = connectionPoint.y > currentPoint.y;
        }
        else if (direction == Vector2Int.down)
        {
            result = connectionPoint.y < currentPoint.y;
        }
        else if (direction == Vector2Int.left)
        {
            result = connectionPoint.x < currentPoint.x;
        }
        else if (direction == Vector2Int.right)
        {
            result = connectionPoint.x > currentPoint.x;
        }
        else
        {
            // Debug.LogWarning($"Unexpected connection direction at position {currentPoint}: {direction}");
            return false;
        }

        if (!result)
        {
            // Debug.Log($"{connectionPoint} is not at the {direction} side of the {currentPoint} ");
        }

        return result;
    }


    private Vector2Int CalculateMidpoint(Vector2Int start, Vector2Int end)
    {
        Vector2Int midPoint = new Vector2Int((start.x + end.x) / 2, (start.y + end.y) / 2);
        MidPoints.Add(midPoint);
        return midPoint;
    }

    private void PlaceFloorTile(Vector2Int position)
    {

        floorTilemap.SetTile(new Vector3Int(position.x, position.y, 0), floorTile);
        //ConnectionPoints.SetTile(new Vector3Int(position.x, position.y, 0), connectionPointTile);


        hallWaySpace.Add(position);
        //actualRoomPositions.Add(position);

    }

    private void ExpandAllHallways()
    {
        // Create a temporary set to avoid modifying hallWaySpace during iteration
        HashSet<Vector2Int> tilesToExpand = new HashSet<Vector2Int>(hallWaySpace);

        foreach (Vector2Int hallwayTile in tilesToExpand)
        {
            // Expand each hallway tile by painting its surrounding space
            ExpandHallwayTile(hallwayTile);
        }
    }

    private void ExpandHallwayTile(Vector2Int hallwayTile)
    {
        // Define all 8 possible directions
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up + Vector2Int.left,    // Top-left
        Vector2Int.up + Vector2Int.right,   // Top-right
        Vector2Int.down + Vector2Int.left,  // Bottom-left
        Vector2Int.down + Vector2Int.right  // Bottom-right
        };

        // Paint surrounding tiles as floor
        foreach (Vector2Int direction in directions)
        {
            Vector2Int surroundingTile = hallwayTile + direction;

            // Ensure this tile is not already part of the room or hallway
            if (!actualRoomPositions.Contains(surroundingTile) && !hallWaySpace.Contains(surroundingTile))
            {
                PlaceFloorTile(surroundingTile); // Paint the floor
            }
        }
    }


    #region Patching function
    private void PatchConnectionPoint(Vector2Int position)
    {
        Vector2Int direction = GetConnectionDir(position);

        if (direction == Vector2Int.up)
        {
            PatchUp(position);
        }
        else if (direction == Vector2Int.down)
        {
            PatchDown(position);
        }
        else if (direction == Vector2Int.left)
        {
            PatchLeft(position);
        }
        else if (direction == Vector2Int.right)
        {
            PatchRight(position);
        }
        else
        {
            // Debug.LogWarning($"Unexpected connection direction at position {position}: {direction}");
        }
    }

    private HashSet<Vector2Int> GetTilePositions(Tilemap tilemap)
    {
        HashSet<Vector2Int> floors = new HashSet<Vector2Int>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned!");
            return floors;
        }

        // Iterate through the bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(pos) != null) // Check if there's a tile
                {
                    floors.Add((Vector2Int)pos);
                }
            }
        }

        return floors;
    }


    private void PatchLeft(Vector2Int pos)
    {
        AddWallTile(pos, wallLeft);
        AddWallTile(pos + Vector2Int.up, wallLeft);
        AddWallTile(pos + Vector2Int.down, wallLeft);
    }

    private void PatchRight(Vector2Int pos)
    {
        AddWallTile(pos, wallRight);
        AddWallTile(pos + Vector2Int.up, wallRight);
        AddWallTile(pos + Vector2Int.down, wallRight);
    }

    private void PatchUp(Vector2Int pos)
    {
        AddWallTile(pos, wallUp);
        AddWallTile(pos + Vector2Int.left, wallUp);
        AddWallTile(pos + Vector2Int.right, wallUp);
    }

    private void PatchDown(Vector2Int pos)
    {
        AddWallTile(pos, wallDown);
        AddWallTile(pos + Vector2Int.left, wallDown);
        AddWallTile(pos + Vector2Int.right, wallDown);
    }

    private void AddWallTile(Vector2Int position, TileBase wallTile)
    {
        // Set the tile in the tilemap
        wallTilemap.SetTile((Vector3Int)position, wallTile);
        floorTilemap.SetTile((Vector3Int)position, null);

        // Add the position to the roomWallPosition HashSet
        roomWallPositions.Add(position);
    }


    #endregion

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // foreach (Vector2Int pos in allWallPositions)
        // {
        //     Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        //     Gizmos.DrawWireCube(worldPos, Vector3.one);
        // }

        // Gizmos.color = Color.green;
        // foreach (Vector2Int pos in temp)
        // {
        //     Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        //     Gizmos.DrawSphere(worldPos, .5f);
        // }
    }
}
