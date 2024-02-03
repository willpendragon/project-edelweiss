using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverworldMapGenerator : MonoBehaviour
{
    public Level[] levelList;
    public GameObject mapNode;
    public Transform mapNodeTransform;
    // Start is called before the first frame update
    void Awake()
    {
        Vector3 initialPosition = mapNodeTransform.position; // Initial position
        Vector3 offset = new Vector3(5, 0, 0); // Example offset: adjust as needed
        int index = 0; // Keep track of the index to multiply the offset

        foreach (var level in levelList)
        {
            mapNode.GetComponent<EnemySelection>().EnemyTypeIds = level.EnemyTypeIds;
            mapNode.GetComponent<EnemySelection>().EnemyCoordinates = level.UnitCoordinates;
            mapNode.GetComponent<EnemySelection>().levelData = level;

            // Calculate new position with offset
            Vector3 mapNodePosition = initialPosition + offset * index;
            Instantiate(mapNode, mapNodePosition, Quaternion.identity);

            index++; // Increment index to increase the offset for the next node
        }
    }
}
