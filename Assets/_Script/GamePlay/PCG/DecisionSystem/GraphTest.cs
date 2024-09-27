using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    Graph graph;

    bool graphReady = false;

    private Dictionary<Vector2Int, int> dijkstraResult;
    public Dictionary<Vector2Int, int> DijkstraResult => dijkstraResult; // Expose via a public read-only property
    int highestValue;

    public void RunDijkstraAlgorithm(Vector2Int playerPosition, IEnumerable<Vector2Int> floorPositions)
    {
        graphReady = false;
        graph = new Graph(floorPositions);
        dijkstraResult = DijkstraAlgorithm.Dijkstra(graph, playerPosition);
        highestValue = dijkstraResult.Values.Max();
        graphReady = true;
    }


    private void OnDrawGizmosSelected()
    {
        if (graphReady && dijkstraResult != null)
        {
            foreach (var item in dijkstraResult)
            {
                Color color = Color.Lerp(Color.green, Color.red, (float)item.Value / highestValue);
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawCube(item.Key + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }
}
