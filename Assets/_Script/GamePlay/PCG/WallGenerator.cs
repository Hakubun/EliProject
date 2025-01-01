using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList); //store the list of side wall position, pass the floor position and a list of up down left and right vector2 list
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList); //store the list of corner wall position
        CreateBasicWall(tilemapVisualizer, basicWallPositions, floorPositions);
        CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);
    }



    public static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in cornerWallPositions)
        {
            string neighboursBinaryType = "";

            foreach (var direction in Direction2D.eightDirectionsList) //same as createBasicWall function except checking all 8 direction to see what to put in it
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }

            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }

    public static void CreateInnerWal(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> innerWallPositions, HashSet<Vector2Int> floor)
    {
        foreach (var position in innerWallPositions)
        {
            string floorBinaryType = "";
            foreach (var diagnoal in Direction2D.diagonalDirectionsList)
            {
                var floorPosition = position + diagnoal;
                if (floor.Contains(floorPosition))
                {
                    floorBinaryType += "1";
                }
                else
                {
                    floorBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintInnerWall(position, floorBinaryType);
        }
    }
    //PaintCornerWall

    public static void CreateCornerWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> allWallPositions, HashSet<Vector2Int> floor)
    {
        foreach (var position in cornerWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (allWallPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            string floorBinaryType = "";
            foreach (var diagnoal in Direction2D.diagonalDirectionsList)
            {
                var floorPosition = position + diagnoal;
                if (floor.Contains(floorPosition))
                {
                    floorBinaryType += "1";
                }
                else
                {
                    floorBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintCornerWall(position, neighboursBinaryType, floorBinaryType);
        }
    }
    public static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPositions) //go through the wall position list
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
        }
    }

    public static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>(); //using hashset to make sure there are no duplicate vector2
        foreach (var position in floorPositions) //go through each floor position
        {
            foreach (var direction in directionList) //check four direction of each floor position
            {
                var neighbourPosition = position + direction;// get the exact position of the direction (x,y) + (1,0) = left side of the floor
                if (floorPositions.Contains(neighbourPosition) == false) //check if the floor list contains the wall position 
                    wallPositions.Add(neighbourPosition); //if the wall floor list does not contain the wall position, add the position to wall list
            }
        }
        return wallPositions;
    }

    public static HashSet<Vector2Int> FilterSimpleWalls(HashSet<Vector2Int> wallPositions, HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        int neighbourFloorCounter = 0;
        HashSet<Vector2Int> basicaWalls = new HashSet<Vector2Int>();
        foreach (var position in wallPositions)
        {
            neighbourFloorCounter = 0;
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighbourFloorCounter += 1;
                }
            }
            if (neighbourFloorCounter == 1)
            {
                basicaWalls.Add(position);
            }
        }
        return basicaWalls;
    }

    public static HashSet<Vector2Int> FilterCornerWalls(HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> allWallsPositions, List<Vector2Int> directionList)
    {
        int neighbourFloorCounter = 0;
        HashSet<Vector2Int> cornerWalls = new HashSet<Vector2Int>();
        foreach (var position in cornerWallPositions)
        {
            neighbourFloorCounter = 0;
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (allWallsPositions.Contains(neighbourPosition))
                {
                    neighbourFloorCounter += 1;
                }
            }
            if (neighbourFloorCounter == 2)
            {
                cornerWalls.Add(position);
            }
        }
        return cornerWalls;
    }
}
