using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{

    [SerializeField]
    protected SimpleRandomWalkSO randomWalkParameters;


    protected override void RunProceduralGeneration() //overall function for outside script to call
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition); //call the randomwalk function
        tilemapVisualizer.Clear(); //clear out previous tiles, just to make sure there aren't any left
        tilemapVisualizer.PaintFloorTiles(floorPositions); //painting new floor tiles
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer); //creating wall
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO parameters, Vector2Int position) //from scriptable object and position starting randomwalk
    {
        var currentPosition = position; //store the position passed from the function
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>(); //initilize floorposition hashset
        for (int i = 0; i < parameters.iterations; i++) //for loop that will control how many time the simplerandomwalk function being called, basically how big the random map will be
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength); //store the returned hashset of vector2int in a list
            floorPositions.UnionWith(path); //add the stored vector2int hashset to floorPosition, UnionWith making sure there are no duplicate position aka Vector2Int
            if (parameters.startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count)); //randomly pick a position from the already created hashset to start the next iteration
        }
        return floorPositions;
    }

}
