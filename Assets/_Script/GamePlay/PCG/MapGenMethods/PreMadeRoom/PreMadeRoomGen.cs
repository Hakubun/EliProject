using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class PreMadeRoomGen : MonoBehaviour
{
    public GameObject startroom;
    public List<GameObject> roomPrefabs;
    public GameObject bossRoomPrefab;
    public Vector2Int startRoomPos;
    public int roomsPerLayer = 10;
    public int numberOfLayers = 3;
    public int initialMinRadius = 30;
    public int initialMaxRadius = 35;
    public float radiusExpansionFactor = 1.5f;
    public int gapBetweenRooms = 1;
    [Range(1, 4)]
    public int numberOfBossRooms = 1;

    public Tilemap floorTilemap, wallTilemap;

    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> actualRoomPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> roomWallPositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> startRoomConnectionPoints = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> regularRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, HashSet<Vector2Int>> bossRoomConnectionPoints = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    private int bossLayerDistance = 0;

    public UnityEvent<RoomsData> OnRoomsReady;

    void Start()
    {
        MergeRoomTilemaps(startroom, startRoomPos);
        StoreConnectionPoints(startroom, startRoomPos, startRoomConnectionPoints);
        MarkActualRoomTiles(startRoomPos, GetRoomSize(startroom));

        bossLayerDistance = GenerateLayers();
        GenerateBossLayer();

        RoomsData data = new RoomsData
        {
            StartRoomCenter = this.startRoomPos,
            occupiedPositions = this.occupiedPositions,
            actualRoomPositions = this.actualRoomPositions,
            roomWallPositions = this.roomWallPositions,
            startRoomConnectionPoints = this.startRoomConnectionPoints,
            bossRoomConnectionPoints = this.bossRoomConnectionPoints,
            regularRoomConnectionPoints = this.regularRoomConnectionPoints
        };

        OnRoomsReady?.Invoke(data);
    }


    int GenerateLayers()
    {
        int minRadius = initialMinRadius;
        int maxRadius = initialMaxRadius;

        for (int layer = 1; layer <= numberOfLayers; layer++)
        {
            GenerateRooms(minRadius, maxRadius, roomsPerLayer);
            minRadius = maxRadius;
            maxRadius = Mathf.RoundToInt(maxRadius * radiusExpansionFactor);
        }

        return maxRadius;
    }

    void GenerateRooms(int minRadius, int maxRadius, int roomCount)
    {
        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int randomRoomPosition = Vector2Int.zero;
            GameObject selectedRoomPrefab = null;
            Vector2Int roomSize = Vector2Int.zero;
            int attempt = 0;
            bool positionFound = false;

            while (attempt < 10)
            {
                randomRoomPosition = GetRandomPosition(startRoomPos, minRadius, maxRadius);
                selectedRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
                roomSize = GetRoomSize(selectedRoomPrefab);

                if (!IsOverlapping(randomRoomPosition, roomSize))
                {
                    positionFound = true;
                    break;
                }

                attempt++;
            }

            if (positionFound)
            {
                MergeRoomTilemaps(selectedRoomPrefab, randomRoomPosition);
                MarkTilesAsOccupied(randomRoomPosition, roomSize); // Mark space with gap
                MarkActualRoomTiles(randomRoomPosition, roomSize); // Mark only the actual room space

                HashSet<Vector2Int> roomConnections = new HashSet<Vector2Int>();
                StoreConnectionPoints(selectedRoomPrefab, randomRoomPosition, roomConnections);
                regularRoomConnectionPoints.Add(randomRoomPosition, roomConnections);
            }

        }
    }

    Vector2Int GetRoomSize(GameObject roomPrefab)
    {
        Rooms roomComponent = roomPrefab.GetComponent<Rooms>();
        return roomComponent != null ? roomComponent.getSize() : Vector2Int.one;
    }

    void MarkActualRoomTiles(Vector2Int position, Vector2Int size)
    {
        int halfWidth = Mathf.FloorToInt(size.x / 2f);  // Half-width of the room
        int halfHeight = Mathf.FloorToInt(size.y / 2f); // Half-height of the room

        for (int x = position.x - halfWidth; x <= position.x + halfWidth; x++) // Inclusive bounds
        {
            for (int y = position.y - halfHeight; y <= position.y + halfHeight; y++) // Inclusive bounds
            {
                actualRoomPositions.Add(new Vector2Int(x, y));
            }
        }
    }


    Vector2Int GetRandomPosition(Vector2Int centerPosition, int minRadius, int maxRadius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        int radius = Random.Range(minRadius, maxRadius);
        return new Vector2Int(centerPosition.x + Mathf.RoundToInt(radius * Mathf.Cos(angle)),
                              centerPosition.y + Mathf.RoundToInt(radius * Mathf.Sin(angle)));
    }

    bool IsOverlapping(Vector2Int position, Vector2Int size)
    {
        int gap = gapBetweenRooms;
        int xStart = position.x - Mathf.FloorToInt(size.x / 2f) - gap;
        int xEnd = position.x + Mathf.CeilToInt(size.x / 2f) + gap;
        int yStart = position.y - Mathf.FloorToInt(size.y / 2f) - gap;
        int yEnd = position.y + Mathf.CeilToInt(size.y / 2f) + gap;

        for (int x = xStart; x < xEnd; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                if (occupiedPositions.Contains(new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void MergeRoomTilemaps(GameObject roomPrefab, Vector2Int spawnPosition)
    {
        GameObject roomInstance = Instantiate(roomPrefab, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);

        Rooms roomComponent = roomInstance.GetComponent<Rooms>();
        if (roomComponent == null)
        {

            Destroy(roomInstance);
            return;
        }

        MergeTilemap(roomComponent.getFloorTile(), floorTilemap, spawnPosition);
        //add the wallpositions to roomWallPositions
        MergeTilemap(roomComponent.getWallTile(), wallTilemap, spawnPosition);

        Destroy(roomInstance);
    }

    private void MergeTilemap(Tilemap sourceTilemap, Tilemap targetTilemap, Vector2Int spawnPosition)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int localPlace = new Vector3Int(x + bounds.x, y + bounds.y, 0);
                TileBase tile = sourceTilemap.GetTile(localPlace);
                if (tile != null)
                {
                    Vector3Int worldPlace = new Vector3Int(localPlace.x + spawnPosition.x, localPlace.y + spawnPosition.y, 0);
                    targetTilemap.SetTile(worldPlace, tile);
                    occupiedPositions.Add(new Vector2Int(worldPlace.x, worldPlace.y));
                    if (targetTilemap.tag == "Wall")
                    {
                        roomWallPositions.Add(new Vector2Int(worldPlace.x, worldPlace.y));
                    }
                }
            }
        }
    }

    void MarkTilesAsOccupied(Vector2Int position, Vector2Int size)
    {
        int gap = gapBetweenRooms;
        int halfWidth = Mathf.FloorToInt(size.x / 2f);  // Half-width of the room
        int halfHeight = Mathf.FloorToInt(size.y / 2f); // Half-height of the room

        for (int x = position.x - halfWidth - gap; x <= position.x + halfWidth + gap; x++) // Inclusive bounds
        {
            for (int y = position.y - halfHeight - gap; y <= position.y + halfHeight + gap; y++) // Inclusive bounds
            {
                occupiedPositions.Add(new Vector2Int(x, y));
            }
        }
    }


    void GenerateBossLayer()
    {
        Vector2Int[] cardinalOffsets = new Vector2Int[]
        {
            new Vector2Int(0, bossLayerDistance),
            new Vector2Int(0, -bossLayerDistance),
            new Vector2Int(-bossLayerDistance, 0),
            new Vector2Int(bossLayerDistance, 0)
        };

        List<Vector2Int> selectedOffsets = new List<Vector2Int>();

        while (selectedOffsets.Count < numberOfBossRooms)
        {
            Vector2Int offset = cardinalOffsets[Random.Range(0, cardinalOffsets.Length)];
            if (!selectedOffsets.Contains(offset))
            {
                selectedOffsets.Add(offset);
            }
        }

        foreach (Vector2Int offset in selectedOffsets)
        {
            Vector2Int bossRoomPosition = startRoomPos + offset;
            Vector2Int bossRoomDir = GetBossRoomDirection(offset);

            if (offset.x == 0)
            {
                bossRoomPosition.x += Random.Range(-bossLayerDistance / 2, bossLayerDistance / 2);
            }
            else if (offset.y == 0)
            {
                bossRoomPosition.y += Random.Range(-bossLayerDistance / 2, bossLayerDistance / 2);
            }

            Vector2Int bossRoomSize = GetRoomSize(bossRoomPrefab);

            if (!IsOverlapping(bossRoomPosition, bossRoomSize))
            {
                MergeRoomTilemaps(bossRoomPrefab, bossRoomPosition);
                MarkTilesAsOccupied(bossRoomPosition, bossRoomSize);
                MarkActualRoomTiles(bossRoomPosition, bossRoomSize);
                HashSet<Vector2Int> bossConnections = new HashSet<Vector2Int>();
                StoreConnectionPoints(bossRoomPrefab, bossRoomPosition, bossConnections, offset);
                bossRoomConnectionPoints.Add(bossRoomDir, bossConnections);
            }
        }
    }

    void StoreConnectionPoints(GameObject roomPrefab, Vector2Int spawnPosition, HashSet<Vector2Int> connectionSet, Vector2Int? bossDirection = null)
    {
        Rooms roomComponent = roomPrefab.GetComponent<Rooms>();
        if (roomComponent == null)
        {

            return;
        }

        Vector2Int[] roomConnections = roomComponent.getConnectionPoints();

        foreach (var connection in roomConnections)
        {
            Vector2Int worldPosition = connection + spawnPosition;

            if (bossDirection.HasValue)
            {
                Vector2Int relativeDirection = GetNormalizedDirection(worldPosition - spawnPosition);
                if (relativeDirection == bossDirection.Value)
                {
                    continue;
                }
            }

            connectionSet.Add(worldPosition);
        }
    }

    private Vector2Int GetBossRoomDirection(Vector2Int offset)
    {
        if (offset.x == 0) //up or down
        {
            if (offset.y > 0)
            {
                return Vector2Int.up;
            }
            else
            {
                return Vector2Int.down;
            }
        }
        else
        {
            if (offset.x > 0) //left or right
            {
                return Vector2Int.right;
            }
            else
            {
                return Vector2Int.left;
            }
        }
    }

    private Vector2Int GetNormalizedDirection(Vector2Int direction)
    {
        if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
        if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;
        return direction;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // foreach (Vector2Int pos in hallWaySpace)
        // {
        //     Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        //     Gizmos.DrawWireCube(worldPos, Vector3.one);
        // }

        // Gizmos.color = Color.green;
        // foreach (Vector2Int pos in )
        // {
        //     Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        //     Gizmos.DrawWireCube(worldPos, Vector3.one);
        // }
    }
}