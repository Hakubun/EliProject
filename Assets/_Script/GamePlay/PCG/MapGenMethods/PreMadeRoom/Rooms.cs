using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rooms : MonoBehaviour
{
    // Reference to the tilemap associated with the room
    public Tilemap tilemap;

    // Center of the room in grid coordinates
    public Vector2Int Center;

    // Dimensions of the room
    public int Width;  // Width of the room
    public int Height; // Height of the room

    // Separate tilemaps for floor and wall tiles
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    // Array of connection points in grid coordinates
    [SerializeField]
    private Vector2Int[] connectionPoints;

    private void Start()
    {
        // Convert the grid-based Center to world coordinates
        Vector3Int gridCenter = new Vector3Int(Center.x, Center.y, 0); // Grid position of the center
        Vector3 worldCenter = tilemap.CellToWorld(gridCenter);        // Convert grid to world position

        // Update the Center to use world coordinates
        Center = new Vector2Int(Mathf.RoundToInt(worldCenter.x), Mathf.RoundToInt(worldCenter.y));
    }

    // Function to get the size of the room as a Vector2Int
    public Vector2Int getSize()
    {
        return new Vector2Int(Width, Height); // Return width and height as a Vector2Int
    }

    // Function to get the room's center in world coordinates
    public Vector2Int getCenter()
    {
        return Center; // Return the center of the room
    }

    // Function to return connection points as an array of world positions
    public Vector3[] GetConnectionPointsInWorld()
    {
        // Create an array to hold world positions of connection points
        Vector3[] worldPositions = new Vector3[connectionPoints.Length];

        // Loop through each connection point
        for (int i = 0; i < connectionPoints.Length; i++)
        {
            // Convert each connection point from grid to world coordinates
            Vector3Int gridPoint = new Vector3Int(connectionPoints[i].x, connectionPoints[i].y, 0); // Grid position
            worldPositions[i] = tilemap.CellToWorld(gridPoint); // Convert to world position

            // Log the converted world position for debugging
            Debug.Log($"{this.gameObject.name} worldPosition {i}: {worldPositions[i]}");
        }

        return worldPositions; // Return the array of world positions
    }

    // Function to get the floor tilemap associated with the room
    public Tilemap getFloorTile()
    {
        return floorTilemap; // Return the floor tilemap
    }

    // Function to get the wall tilemap associated with the room
    public Tilemap getWallTile()
    {
        return wallTilemap; // Return the wall tilemap
    }
}
