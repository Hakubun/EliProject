using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PreMadeRoomGen : MonoBehaviour
{
    public GameObject startroom; // Prefab for the start room
    public List<GameObject> roomPrefabs; // List of room prefabs to pick from
    public GameObject bossRoomPrefab; // Prefab for the boss room
    public Vector2Int startRoomPos; // Transform for the start room position
    public int roomsPerLayer = 10; // Number of rooms to generate per layer
    public int numberOfLayers = 3; // Number of layers to generate
    public int initialMinRadius = 30; // Minimum spawn radius for the first layer
    public int initialMaxRadius = 35; // Maximum spawn radius for the first layer
    public float radiusExpansionFactor = 1.5f; // Factor to expand radius for each new layer
    public int gapBetweenRooms = 1; // Gap between rooms
    [Range(1, 4)]
    public int numberOfBossRooms = 1; // Number of boss rooms to generate (1 to 4)

    public Tilemap floorTilemap, wallTilemap; // Tilemaps to merge tiles into

    private List<RectInt> placedRoomBounds = new List<RectInt>(); // List of placed room bounds

    private RectInt bossRoomBounds = new RectInt(); // Stores the boss room bounds
    private List<RectInt> bossLayerRoomBounds = new List<RectInt>(); // Stores the boss layer room bounds

    private int bossLayerDistance = 0; // Calculated distance for the boss layer

    void Start()
    {
        // Spawn and merge the start room
        MergeRoomTilemaps(startroom, startRoomPos);

        // Generate layers of rooms
        bossLayerDistance = GenerateLayers();

        // Generate the boss layer
        GenerateBossLayer();
    }

    int GenerateLayers()
    {
        int minRadius = initialMinRadius;
        int maxRadius = initialMaxRadius;

        for (int layer = 1; layer <= numberOfLayers; layer++)
        {
            GenerateRooms(minRadius, maxRadius, roomsPerLayer);

            // Expand the radius for the next layer
            minRadius = maxRadius;
            maxRadius = Mathf.RoundToInt(maxRadius * radiusExpansionFactor); // Expand radius for the next layer
        }

        return maxRadius; // Return the maximum radius for the boss layer
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

            while (attempt < 10) // Limit the number of attempts
            {
                randomRoomPosition = GetRandomPosition(startRoomPos, minRadius, maxRadius);
                selectedRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
                roomSize = GetRoomSize(selectedRoomPrefab);

                if (!IsOverlapping(randomRoomPosition, roomSize, gapBetweenRooms))
                {
                    positionFound = true;
                    break; // Exit the while loop if a valid position is found
                }

                attempt++;
            }

            if (positionFound)
            {
                MergeRoomTilemaps(selectedRoomPrefab, randomRoomPosition);
                placedRoomBounds.Add(CreateRoomBounds(randomRoomPosition, roomSize, gapBetweenRooms));
            }
        }
    }

    void GenerateBossLayer()
    {
        Vector2Int[] cardinalOffsets = new Vector2Int[]
        {
            new Vector2Int(0, bossLayerDistance), // Up
            new Vector2Int(0, -bossLayerDistance), // Down
            new Vector2Int(-bossLayerDistance, 0), // Left
            new Vector2Int(bossLayerDistance, 0) // Right
        };

        List<Vector2Int> selectedOffsets = new List<Vector2Int>();

        // Determine which cardinal directions to use based on the number of boss rooms
        if (numberOfBossRooms < 1 || numberOfBossRooms > 4)
        {
            numberOfBossRooms = 1;
        }

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
            Vector2Int bossRoomSize = GetRoomSize(bossRoomPrefab);

            bossRoomBounds = CreateRoomBounds(bossRoomPosition, bossRoomSize, gapBetweenRooms);

            if (!IsOverlapping(bossRoomPosition, bossRoomSize, gapBetweenRooms))
            {
                MergeRoomTilemaps(bossRoomPrefab, bossRoomPosition);
                RectInt roomBounds = CreateRoomBounds(bossRoomPosition, bossRoomSize, gapBetweenRooms);
                bossLayerRoomBounds.Add(roomBounds);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw boss room bounds
        Gizmos.color = Color.red;
        if (bossRoomBounds.size != Vector2Int.zero)
        {
            Gizmos.DrawWireCube(
                new Vector3(bossRoomBounds.center.x, bossRoomBounds.center.y, 0),
                new Vector3(bossRoomBounds.width, bossRoomBounds.height, 1)
            );
        }

        // Draw all boss layer room bounds
        Gizmos.color = Color.green;
        foreach (RectInt roomBounds in bossLayerRoomBounds)
        {
            Gizmos.DrawWireCube(
                new Vector3(roomBounds.center.x, roomBounds.center.y, 0),
                new Vector3(roomBounds.width, roomBounds.height, 1)
            );
        }
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

        Tilemap floorTilemapInRoom = roomComponent.getFloorTile();
        Tilemap wallTilemapInRoom = roomComponent.getWallTile();

        if (floorTilemapInRoom != null)
        {
            MergeTilemap(floorTilemapInRoom, floorTilemap, spawnPosition);
        }

        if (wallTilemapInRoom != null)
        {
            MergeTilemap(wallTilemapInRoom, wallTilemap, spawnPosition);
        }

        Destroy(roomInstance); // Remove the temporary room instance
    }

    private void MergeTilemap(Tilemap sourceTilemap, Tilemap targetTilemap, Vector2Int spawnPosition)
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        TileBase[] allTiles = sourceTilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int localPlace = new Vector3Int(x + bounds.x, y + bounds.y, 0);

                TileBase tile = sourceTilemap.GetTile(localPlace);

                if (tile != null)
                {
                    Vector3Int worldPosition = new Vector3Int(
                        localPlace.x + spawnPosition.x,
                        localPlace.y + spawnPosition.y,
                        0
                    );

                    Matrix4x4 transformMatrix = sourceTilemap.GetTransformMatrix(localPlace);
                    targetTilemap.SetTile(worldPosition, tile);
                    targetTilemap.SetTransformMatrix(worldPosition, transformMatrix);
                }
            }
        }
    }

    Vector2Int GetRandomPosition(Vector2Int centerPosition, int minRadius, int maxRadius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
        int radius = Random.Range(minRadius, maxRadius); // Random radius within the range

        int offsetX = Mathf.RoundToInt(radius * Mathf.Cos(angle));
        int offsetY = Mathf.RoundToInt(radius * Mathf.Sin(angle));

        return new Vector2Int(centerPosition.x + offsetX, centerPosition.y + offsetY);
    }

    Vector2Int GetRoomSize(GameObject roomPrefab)
    {
        Rooms roomComponent = roomPrefab.GetComponent<Rooms>();
        if (roomComponent != null)
        {
            return roomComponent.getSize();
        }

        return Vector2Int.one; // Default size if no Rooms component is found
    }

    RectInt CreateRoomBounds(Vector2Int position, Vector2Int size, int gap)
    {
        Vector2Int expandedSize = size + new Vector2Int(gap, gap);
        return new RectInt(position.x - expandedSize.x / 2, position.y - expandedSize.y / 2, expandedSize.x, expandedSize.y);
    }

    bool IsOverlapping(Vector2Int position, Vector2Int size, int gap)
    {
        RectInt newRoomBounds = CreateRoomBounds(position, size, gap);
        foreach (RectInt existingBounds in placedRoomBounds)
        {
            if (newRoomBounds.Overlaps(existingBounds))
            {
                return true;
            }
        }
        return false;
    }
}
