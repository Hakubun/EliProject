using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsData
{
    public Vector2Int StartRoomCenter;
    public HashSet<Vector2Int> occupiedPositions;

    public HashSet<Vector2Int> actualRoomPositions;
    public HashSet<Vector2Int> startRoomConnectionPoints;
    public Dictionary<Vector2Int, HashSet<Vector2Int>> bossRoomConnectionPoints;

    public Dictionary<Vector2Int, HashSet<Vector2Int>> regularRoomConnectionPoints;
}
