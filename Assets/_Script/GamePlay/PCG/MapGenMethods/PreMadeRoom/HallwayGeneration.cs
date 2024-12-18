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
    public TileBase floorTile;
    public TileBase connectionPointTile;

    public void GenerateHallway(RoomsData data)
    {
        // Initialize Data
        InitializeData(data);

        // Connect start room to the nearest regular room connection points
        ConnectStartRoom();

        ConnectBossRoom();


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
            Vector2Int closestRoomConnectionPoint = FindClosestConnectionPoint(connectionPoint, faceDir, true);

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
                if (bossRoom.Key == faceDir)
                {
                    Debug.Log($"boss room dirction is {bossRoom.Key} and connection point is facing {faceDir}, {bossRoom.Key == faceDir}");
                    //patch the wall;
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

    private Vector2Int FindClosestConnectionPoint(Vector2Int currentPoint, Vector2Int facingDirection, bool startRoom)
    {
        Vector2Int closestPoint = Vector2Int.zero;

        float minDistance = float.MaxValue;

        //look for middle point
        if (!startRoom)
        {
            foreach (var point in MidPoints)
            {
                float distance = Vector2Int.Distance(currentPoint, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = point;
                }
            }
        }

        foreach (var room in regularRoomConnectionPoints)
        {
            foreach (var roomConnectionPoint in room.Value)
            {
                // Exclude connection points that are facing the opposite direction
                if (GetConnectionDir(roomConnectionPoint) != -facingDirection)
                    continue;

                float distance = Vector2Int.Distance(currentPoint, roomConnectionPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = roomConnectionPoint;

                }
            }
        }

        return closestPoint; // Return as a tuple
    }


    private void DrawHallway(Vector2Int start, Vector2Int end)
    {
        // Extend from both sides by 3 tiles and connect in the middle
        Vector2Int currentStart = ExtendHallway(start, GetConnectionDir(start), 3);
        Vector2Int currentEnd = ExtendHallway(end, GetConnectionDir(end), 3);

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
        MidPoints.Add(midpoint);

        // Connect start to midpoint
        DrawPath(start, midpoint);

        // Connect end to midpoint
        DrawPath(end, midpoint);
    }

    private void DrawPath(Vector2Int from, Vector2Int to)
    {
        // Directions for movement: up, down, left, right
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // Track visited positions
        Queue<Vector2Int> queue = new Queue<Vector2Int>();       // Positions to explore
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(from);
        visited.Add(from);

        // BFS to find the path
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == to)
                break; // Target reached

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (!visited.Contains(next) && !actualRoomPositions.Contains(next)) // Valid move
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                    parent[next] = current; // Track the path
                }
            }
        }

        // Reconstruct the path
        if (!parent.ContainsKey(to))
        {
            Debug.LogWarning("No valid path found to target!");
            return; // No path found
        }

        Vector2Int currentPos = to;
        Stack<Vector2Int> path = new Stack<Vector2Int>();

        while (currentPos != from)
        {
            path.Push(currentPos);
            currentPos = parent[currentPos];
        }

        // Place tiles along the path
        while (path.Count > 0)
        {
            Vector2Int pos = path.Pop();
            PlaceFloorTile(pos);
        }
    }



    private void RemoveConnectionPoint(Vector2Int connectionPoint)
    {
        foreach (var room in regularRoomConnectionPoints)
        {
            if (room.Value.Remove(connectionPoint) && room.Value.Count == 0)
            {
                regularRoomConnectionPoints.Remove(room.Key);
                break;
            }
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
        ConnectionPoints.SetTile(new Vector3Int(position.x, position.y, 0), connectionPointTile);

        occupiedPositions.Add(position);
        hallWaySpace.Add(position);

    }

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
