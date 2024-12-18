using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HallwayGeneration : MonoBehaviour
{
    private Vector2Int StartRoomCenter;
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> actualRoomPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> startRoomConnectionPoints = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> regularRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> bossRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    private HashSet<Vector2Int> hallWaySpace = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> MidPoints = new HashSet<Vector2Int>();

    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap ConnectionPoints; // Optional: Visualize connection points
    public TileBase floorTile, wallUp, wallDown, wallLeft, wallRight;
    public TileBase connectionPointTile;

    public float roomMaxDist = 10;
    public void GenerateHallway(RoomsData data)
    {
        // Initialize Data
        InitializeData(data);

        // Connect start room to the nearest regular room connection points
        ConnectStartRoom();

        ConnectBossRoom();

        ConnectRegularRooms();


    }

    private void InitializeData(RoomsData data)
    {
        StartRoomCenter = data.StartRoomCenter;
        startRoomConnectionPoints = new HashSet<Vector2Int>(data.startRoomConnectionPoints);
        regularRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>(data.regularRoomConnectionPoints);
        bossRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>(data.bossRoomConnectionPoints);
        occupiedPositions = new HashSet<Vector2Int>(data.occupiedPositions);
        actualRoomPositions = new HashSet<Vector2Int>(data.actualRoomPositions);
    }

    private void ConnectStartRoom()
    {
        foreach (var connectionPoint in startRoomConnectionPoints)
        {
            Vector2Int faceDir = GetConnectionDir(connectionPoint);

            // Use a tuple to receive two values
            Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir, false);

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
                    Debug.Log($"boss room dirction is {bossRoom.Key} and connection point is facing {faceDir}, {bossRoom.Key == faceDir}");
                    //patch the wall;
                    PatchConnectionPoint(connectionPoint);
                }
                else
                {
                    Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir, false);

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
        foreach (var room in regularRoomConnectionPoints)
        {
            foreach (var connectionPoint in room.Value)
            {
                Vector2Int faceDir = GetConnectionDir(connectionPoint);

                Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir, true);

                if (closestRoomConnectionPoint != Vector2Int.zero)
                {
                    DrawHallway(connectionPoint, closestRoomConnectionPoint);
                    RemoveConnectionPoint(closestRoomConnectionPoint);
                }

            }
        }
    }

    private Vector2Int FindClosestConnectionPoint(Vector2Int currentPoint, Vector2Int facingDirection, bool regularRoom)
    {
        Vector2Int closestPoint = Vector2Int.zero;
        float minDistance = float.MaxValue;
        if (regularRoom)
        {
            minDistance = roomMaxDist;
        }

        // Look for middle point if not in start room

        foreach (var room in regularRoomConnectionPoints)
        {
            if (room.Value.Contains(currentPoint))
            {
                continue;
            }
            foreach (var roomConnectionPoint in room.Value)
            {
                //currentPoint += facingDirection * 2;
                // Calculate vector from currentPoint to roomConnectionPoint
                Vector2Int directionToConnection = roomConnectionPoint - currentPoint;

                // Only consider points generally in the facing direction
                if (Vector2.Dot(facingDirection, directionToConnection) <= 0)
                    continue;

                float distance = Vector2Int.Distance(currentPoint, roomConnectionPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = roomConnectionPoint;
                }
            }
        }

        // if (regularRoom)
        // {
        //     foreach (var point in hallWaySpace)
        //     {
        //         float distance = Vector2Int.Distance(currentPoint, point);
        //         if (distance < minDistance)
        //         {
        //             minDistance = distance;
        //             closestPoint = point;
        //         }
        //     }
        // }

        return closestPoint;
    }



    private void DrawHallway(Vector2Int start, Vector2Int end)
    {
        // Extend from both sides by 3 tiles and connect in the middle
        Vector2Int currentStart = ExtendHallway(start, GetConnectionDir(start), 2);
        Vector2Int currentEnd = ExtendHallway(end, GetConnectionDir(end), 2);

        ConnectToMidpoint(currentStart, currentEnd);
    }

    private Vector2Int ExtendHallway(Vector2Int point, Vector2Int direction, int length)
    {
        //? might wanna do a random extension 
        for (int i = 0; i < length; i++)
        {
            point += direction;
            PlaceFloorTile(point);
        }
        return point;
    }

    private void ConnectToMidpoint(Vector2Int start, Vector2Int end)
    {
        Vector2Int midpoint = CalculateMidpoint(start, end);
        ConnectionPoints.SetTile(new Vector3Int(midpoint.x, midpoint.y, 0), connectionPointTile);
        MidPoints.Add(midpoint);

        // Connect start to midpoint
        DrawPath(start, midpoint);

        // Connect end to midpoint
        DrawPath(end, midpoint);
    }

    private void DrawPath(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        //? might wanna do something to check move y first or x first
        while (current != to)
        {
            if (current.x != to.x)
                current.x += current.x < to.x ? 1 : -1;
            else if (current.y != to.y)
                current.y += current.y < to.y ? 1 : -1;

            PlaceFloorTile(current);
        }
    }


    private void RemoveConnectionPoint(Vector2Int connectionPoint)
    {
        Vector2Int keyToRemove = Vector2Int.zero;
        bool shouldRemoveKey = false;

        // Iterate over a safe copy of the dictionary keys
        foreach (var room in regularRoomConnectionPoints)
        {
            if (room.Value.Contains(connectionPoint))
            {
                room.Value.Remove(connectionPoint); // Remove the connection point

                if (room.Value.Count == 0)
                {
                    keyToRemove = room.Key;
                    shouldRemoveKey = true;
                }
                break; // Exit once the connection point is found and removed
            }
        }

        // Remove the key outside of the loop
        if (shouldRemoveKey)
        {
            regularRoomConnectionPoints.Remove(keyToRemove);
        }
    }


    private Vector2Int GetConnectionDir(Vector2Int point)
    {
        Vector2Int[] directions = new Vector2Int[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        };

        foreach (Vector2Int dir in directions)
        {
            if (!actualRoomPositions.Contains(point + dir)) // Corrected Contains method
            {
                return dir;
            }
        }

        return Vector2Int.zero;
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

        occupiedPositions.Add(position);
        hallWaySpace.Add(position);

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
            Debug.LogWarning($"Unexpected connection direction at position {position}: {direction}");
        }
    }

    //! might need to pass wall tile accordingly
    private void PatchLeft(Vector2Int pos)
    {
        wallTilemap.SetTile((Vector3Int)pos, wallLeft);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.up), wallLeft);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.down), wallLeft);
    }
    private void PatchRight(Vector2Int pos)
    {
        wallTilemap.SetTile((Vector3Int)pos, wallRight);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.up), wallRight);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.down), wallRight);
    }
    private void PatchUp(Vector2Int pos)
    {
        wallTilemap.SetTile((Vector3Int)pos, wallUp);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.left), wallUp);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.right), wallUp);
    }
    private void PatchDown(Vector2Int pos)
    {
        wallTilemap.SetTile((Vector3Int)pos, wallDown);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.left), wallDown);
        wallTilemap.SetTile((Vector3Int)(pos + Vector2Int.right), wallDown);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector2Int pos in actualRoomPositions)
        {
            Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
            Gizmos.DrawWireCube(worldPos, Vector3.one);
        }
    }
}
