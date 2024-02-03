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
        Vector3 initialPosition = mapNodeTransform.position;
        Vector3 offset = new Vector3(5, 0, 0);
        int highestUnlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 1); // Default to 1 if not set

        for (int i = 0; i < levelList.Length; i++)
        {
            var level = levelList[i];

            // Setup node with level data
            GameObject newNode = Instantiate(mapNode, initialPosition + offset * i, Quaternion.identity);
            newNode.GetComponent<EnemySelection>().EnemyTypeIds = level.EnemyTypeIds;
            newNode.GetComponent<EnemySelection>().EnemyCoordinates = level.UnitCoordinates;
            newNode.GetComponent<EnemySelection>().levelData = level;
            newNode.GetComponent<EnemySelection>().levelNumber = i + 1; // Assign level number, assuming your levels start at 1

            // Disable or visually indicate the node is locked if it's beyond the highest unlocked level
            if (i + 1 > highestUnlockedLevel)
            {
                newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelLocked;
                // Example: newNode.GetComponent<Button>().interactable = false;
                // Or, set a locked visual state here
            }
            else if (i + 1 <= highestUnlockedLevel)
            {
                newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;

            }
        }
    }
    /*
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
    */
}
