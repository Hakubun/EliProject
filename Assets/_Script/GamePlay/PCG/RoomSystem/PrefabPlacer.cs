using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;

    public List<GameObject> PlaceEnemies(List<EnemyPlacementData> enemyPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var placementData in enemyPlacementData)
        {
            for (int i = 0; i < placementData.Quantity; i++)
            {
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    PlacementType.OpenSpace,
                    100,
                    placementData.enemySize,
                    false
                    );
                if (possiblePlacementSpot.HasValue)
                {

                    placedObjects.Add(CreateObject(placementData.enemyPrefab, possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f))); //Instantiate(placementData.enemyPrefab,possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f), Quaternion.identity)
                }
            }
        }
        return placedObjects;
    }

    public List<GameObject> PlaceBossEnemies(List<EnemyPlacementData> enemyPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();



        return placedObjects;
    }

    public List<GameObject> PlaceAllItems(List<ItemPlacementData> itemPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        //create a list to return after all object placed
        List<GameObject> placedObjects = new List<GameObject>();

        //reorder the list of item from largest to the smallest
        IEnumerable<ItemPlacementData> sortedList = new List<ItemPlacementData>(itemPlacementData).OrderByDescending(placementData => placementData.itemData.size.x * placementData.itemData.size.y);

        //go through the list and start placing items
        foreach (var placementData in sortedList)
        {
            // base on how many of the same items the room need to spawn
            for (int i = 0; i < placementData.Quantity; i++)
            {
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    placementData.itemData.placementType, //2 types: open space or near wall
                    100, //try 100 time to find the spot that can be place
                    placementData.itemData.size, //item size, a vector 2 value representing the wide and height
                    placementData.itemData.addOffset); // weather or not to add offset to it


                if (possiblePlacementSpot.HasValue) //if the item find a place to spawn successfully
                {

                    placedObjects.Add(PlaceItem(placementData.itemData, possiblePlacementSpot.Value)); //adding the item to the list and place the item in the room
                }
            }
        }
        return placedObjects;
    }
    private GameObject PlaceItem(ItemData item, Vector2 placementPosition)
    {
        GameObject newItem = CreateObject(itemPrefab, placementPosition); //spawn the item at the position we found 
        //GameObject newItem = Instantiate(itemPrefab, placementPosition, Quaternion.identity);
        newItem.GetComponent<Item>().Initialize(item); //set up item's value and stuff base on the scriptable object
        return newItem;
    }

    public GameObject CreateObject(GameObject prefab, Vector3 placementPosition)
    {
        if (prefab == null) //try to catch the error
            return null;
        GameObject newItem; //just a new object to return
        if (Application.isPlaying) //make sure the game is running
        {
            newItem = Instantiate(prefab, placementPosition, Quaternion.identity);
        }
        else
        {
            newItem = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newItem.transform.position = placementPosition;
            newItem.transform.rotation = Quaternion.identity;
        }

        return newItem;
    }
}
