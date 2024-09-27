using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProceduralGenerationAlgorithms
{

    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength) //startPosition is the starting point and walkLength is the number of steps the function will run, in this case how many time the for loop will loop
    //using Vector2Int instead of Vector2 because Vector2Int have override function of get hashcode that can work with hash set data

    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>(); //initilize the hashset

        path.Add(startPosition); //adding startingposition to the path hashset
        var previousPosition = startPosition; //setup position so the forloop will know what the previous value is

        for (int i = 0; i < walkLength; i++)
        {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection(); //from the previousPosition we setup, randomly pick a direction and move forward - up, down, left, right
            path.Add(newPosition); //add the position "vector2int" in this case to the path hashset
            previousPosition = newPosition; //set the previousPosition to the newposition so next loop won't start from "startposition"
        }
        return path;
    }

    public static List<Vector2Int> RandomWalkCorridor(HashSet<Vector2Int> corridors, Vector2Int startPosition, int corridorLength) //since we need the access to the last position to continue create corridor, we will be using list instead of hashset
    {
        //? we can edit this to make the corridor 3 block wide later

        List<Vector2Int> corridor = new List<Vector2Int>(); //initialize the list
        var currentPosition = startPosition; //setup position

        var direction = Direction2D.GetRandomCardinalDirection(); //get a random direction
        while (corridors.Contains(currentPosition + direction))
        {
            direction = Direction2D.GetRandomCardinalDirection();
        }
        corridor.Add(currentPosition); //add the position to the list
        //corridor.Add(CalculateAdditionalCorridorTile(currentPosition, direction));

        for (int i = 0; i < corridorLength; i++) //since we are creating a corridor, we will not randomize the direction once we set it, just continue walking towards that direction
        {
            currentPosition += direction; //one step forward to the direction we need to go
            corridor.Add(currentPosition); //store the position
            //corridor.Add(CalculateAdditionalCorridorTile(currentPosition, direction));
        }
        return corridor;
    }

    public static List<Vector2Int> RandomWalkCorridorNew(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        Vector2Int previousDirection = Vector2Int.zero;
        Vector2Int direction = Direction2D.GetRandomCardinalDirection();

        for (int i = 0; i < corridorLength; i++)
        {
            // Ensure the new direction is not the opposite of the previous direction
            while (direction == -previousDirection)
            {
                direction = Direction2D.GetRandomCardinalDirection();
            }

            currentPosition += direction;
            corridor.Add(currentPosition);

            previousDirection = direction;
        }

        return corridor;
    }

    private static Vector2Int CalculateAdditionalCorridorTile(Vector2Int currentPosition, Vector2Int direction)
    {
        Vector2Int offset = Vector2Int.zero;
        if (direction.y > 0)
            offset.x = 1;
        else if (direction.y < 0)
            offset.x = -1;
        else if (direction.x > 0)
            offset.y = -1;
        else
            offset.y = 1;
        return currentPosition + offset;
    }

    //BoundsInt is a struct that can be represent a area by 2 points : min and max point
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        //usually we use tree structure to create this algo but we can use Queue in this case
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>(); //first in first out
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit); //from the spaceToSplit we passed in, we create a queue
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue(); //Dequeue will return the element and remove it from the queue
            if (room.size.y >= minHeight && room.size.x >= minWidth) //check if the space is big enough for room generation, if the x and y is larger than min values, it means the space can be split into multiple space
            {
                if (Random.value < 0.5f) // 50/50 chance to split the room horizontally or vertically, the reason for this is so the code won't prioritize horizontal split throughout the generation process
                { //try to split it horizontally first
                    if (room.size.y >= minHeight * 2) //double check if the room can be split horizontally
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth * 2) //since the room can't be split horizontally, check if it can be split vertically
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight) //can't split the room anymore but can contain a room
                    {
                        roomsList.Add(room);
                    }
                }
                else
                { //try to split the room vertically first, the rest same as logic above
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var xSplit = Random.Range(1, room.size.x); //randomly pick a value between 1 to the size of x
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z)); //room min is the bottom left point x position, xSplit is the point where it split
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z)); //room.min.x + xSplit is the new x min point for room2
        roomsQueue.Enqueue(room1); //add the room1 to queue for check if it can split
        roomsQueue.Enqueue(room2); //add the room2 to queue for check if it can split
    }

    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room) //same logic as SplitVertically function just change the y point
    {
        var ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int> //create for direction position and store it to the list
    {
        new Vector2Int(0,1), //UP
        new Vector2Int(1,0), //RIGHT
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, 0) //LEFT
    };

    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(1,1), //UP-RIGHT
        new Vector2Int(1,-1), //RIGHT-DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 1) //LEFT-UP
    };

    public static List<Vector2Int> eightDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //UP
        new Vector2Int(1,1), //UP-RIGHT
        new Vector2Int(1,0), //RIGHT
        new Vector2Int(1,-1), //RIGHT-DOWN
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 0), //LEFT
        new Vector2Int(-1, 1) //LEFT-UP

    };

    public static Vector2Int GetRandomCardinalDirection() //give out a random direction from the list
    {
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }

    public static List<Vector2Int> GetEightDirection(Vector2Int position)
    {
        List<Vector2Int> surroundingSpaces = new List<Vector2Int>();

        for (int i = 0; i < eightDirectionsList.Count; i++)
        {
            surroundingSpaces.Add(position + eightDirectionsList[i]);
        }

        return surroundingSpaces;
    }
}