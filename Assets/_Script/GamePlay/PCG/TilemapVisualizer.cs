using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft,
        wallTopRight, wallTopLeft, wallBotRight, wallBotLeft,
        wallTopRightAlt, wallTopLeftAlt, wallBotRightAlt, WallBotLeftAlt;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions) //using IEnumberable so the function accept collection, allow more flexibility instead of only accpeting hashset
    {
        PaintTiles(floorPositions, floorTilemap, floorTile); //call the paintTiles function, the reason for this is to be able to read properly in the next function since you are accpeting a hashset from outside script but you store the tilemap and tile within this script
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile) //same as the function above, go through each position and paint it, except this time we specify which tile should be paint 
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSiderLeft;
        }
        else if (WallTypesHelper.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    internal void PaintCornerWall(Vector2Int position, string binaryType, string altType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        int altInt = Convert.ToInt32(altType, 2);
        TileBase tile = null;
        if (WallTypesHelper.TopRight.Contains(typeAsInt))
        {
            if (WallTypesHelper.floorBotLeft.Contains(altInt))
            {
                tile = wallTopRight;
            }
            else
            {
                tile = wallTopRightAlt;
            }
        }
        else if (WallTypesHelper.TopLeft.Contains(typeAsInt))
        {
            if (WallTypesHelper.floorBotRight.Contains(altInt))
            {
                tile = wallTopLeft;
            }
            else
            {
                tile = wallTopLeftAlt;
            }
        }
        else if (WallTypesHelper.BotLeft.Contains(typeAsInt))
        {
            if (WallTypesHelper.floorTopRight.Contains(altInt))
            {
                tile = wallBotLeft;
            }
            else
            {
                tile = WallBotLeftAlt;
            }
        }
        else if (WallTypesHelper.BotRight.Contains(typeAsInt))
        {
            if (WallTypesHelper.floorTopLeft.Contains(altInt))
            {
                tile = wallBotRight;
            }
            else
            {
                tile = wallBotRightAlt;
            }
        }


        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    internal void PaintInnerWall(Vector2Int position, string altType)
    {
        int typeAsInt = Convert.ToInt32(altType, 2);
        TileBase tile = null;
        if (WallTypesHelper.floorBotLeft.Contains(typeAsInt))
        {

            tile = wallTopRight;


        }
        else if (WallTypesHelper.floorBotRight.Contains(typeAsInt))
        {

            tile = wallTopLeft;


        }
        else if (WallTypesHelper.floorTopRight.Contains(typeAsInt))
        {

            tile = wallBotLeft;

        }
        else if (WallTypesHelper.floorTopLeft.Contains(typeAsInt))
        {

            tile = wallBotRight;

        }


        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position); //convert the position we passover to the tilemap's position, because it takes a Vector3 due to tilemap also work with 3d, we will covert the vector2 to vector3
        // if (tilemap.GetTile(tilePosition) == null)
        // {
        // }
        tilemap.SetTile(tilePosition, tile);//paint the tile
    }

    public void Clear() //clear any existing tiles in the scene
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottmEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }
}
