using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPlacementHelper
{
    Dictionary<PlacementType, HashSet<Vector2Int>> tileByType = new Dictionary<PlacementType, HashSet<Vector2Int>>();
    HashSet<Vector2Int> roomFloorNoCorridor;

    public ItemPlacementHelper(HashSet<Vector2Int> roomFloor,
        HashSet<Vector2Int> roomFloorNoCorridor)
    {
        Graph graph = new Graph(roomFloor);
        this.roomFloorNoCorridor = roomFloorNoCorridor;

        foreach (var position in roomFloorNoCorridor) //go through all the positions except corridor position
        {
            int neighboursCount8Dir = graph.GetNeighbours8Directions(position).Count; //this will check how many vector2int position are the position that is in the floor list
            PlacementType type = neighboursCount8Dir < 8 ? PlacementType.NearWall : PlacementType.OpenSpace; //this is to set the tile as if it has 8 open space, it will be open space or it will be near wall
            //less than 8 = nearwall, equal 8 is openspace, there's no way it's greater than 8 since we checking 8 direction

            if (tileByType.ContainsKey(type) == false) //this basically create a key of nearwall or openspace dictionary key and can use to store the vector2 value that can use to store items
                tileByType[type] = new HashSet<Vector2Int>(); //this will be run the very first time or it will just skip

            if (type == PlacementType.NearWall && graph.GetNeighbours4Directions(position).Count == 4) //this is checking if a space is near wall but have all 4 direction : up, right, down, left open up avaliable
                continue; // if that is the case, it will not be add to the dictionary at all
            tileByType[type].Add(position);
        }
    }

    public Vector2? GetItemPlacementPosition(PlacementType placementType, int iterationsMax, Vector2Int size, bool addOffset)
    {
        int itemArea = size.x * size.y; //calculate how many tiles(in this case vector 2 positions) it will take
        if (tileByType[placementType].Count < itemArea) //if the stored position count is less than the size of the object, return null as the function can't find a position to spawn the object
            return null;

        int iteration = 0; //reset iteration
        while (iteration < iterationsMax) //start trying to find a position to place the item
        {
            //! this while loop might not be ideal since the random range might pick the same number. 
            iteration++;
            int index = UnityEngine.Random.Range(0, tileByType[placementType].Count);  //randomlly pick a vector position in the tileByTYpe dictionary list - this is geting the index, not the position itself since the key is different in dictionary
            Vector2Int position = tileByType[placementType].ElementAt(index); //get the position of the randomly picked element

            if (itemArea > 1) //this is to check if the item is larger than 1 vecter2 position
            {
                var (result, placementPositions) = PlaceBigItem(position, size, addOffset); //try to get the position that can be place

                if (result == false)
                    continue; //retry to find a position

                tileByType[placementType].ExceptWith(placementPositions); //this will exacute when the position is find, so we remove the position from the dictionary element so no other items can be attempt to place on this position
                tileByType[PlacementType.NearWall].ExceptWith(placementPositions); //in case the position is also stored in both open space and nearwall, remove the position from nearwall space as well
            }
            else
            {
                tileByType[placementType].Remove(position); //since it's a size 1 item, we can just find a vector2 position and place it and spawn
            }


            return position;
        }
        return null;
    }

    private (bool, List<Vector2Int>) PlaceBigItem(Vector2Int originPosition, Vector2Int size, bool addOffset)
    {
        List<Vector2Int> positions = new List<Vector2Int>() { originPosition };//this is pretty much a math problem, no need to explain?
        int maxX = addOffset ? size.x + 1 : size.x;
        int maxY = addOffset ? size.y + 1 : size.y;
        int minX = addOffset ? -1 : 0;
        int minY = addOffset ? -1 : 0;

        for (int row = minX; row <= maxX; row++)
        {
            for (int col = minY; col <= maxY; col++)
            {
                if (col == 0 && row == 0)
                    continue; // skip 0,0 position, since originPosition + 0,0 is the originPosition and we know it's in the hashset of floorlist, we can skip that to save some time 
                Vector2Int newPosToCheck = new Vector2Int(originPosition.x + row, originPosition.y + col); //calculating the position
                if (roomFloorNoCorridor.Contains(newPosToCheck) == false) //if the calculated position is not in the floor list, we just call off the entire function
                    return (false, positions); //call off the function and return result
                positions.Add(newPosToCheck); //if it is in the floor hashset, add to the position list so we can return it later
            }
        }
        return (true, positions);
    }

}

public enum PlacementType
{
    OpenSpace,
    NearWall
}
