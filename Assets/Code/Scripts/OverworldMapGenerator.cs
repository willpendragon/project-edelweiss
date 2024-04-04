using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverworldMapGenerator : MonoBehaviour
{
    public Level[] levelList;
    public GameObject mapNode;
    public Transform mapNodeTransform;
    public float maxPositionVariation = 2f; // Maximum variation in position
    public float minDistanceApart = 3f; // Minimum distance between nodes
    public int randomSeed = 12345; // Seed for the random number generator

    private LineRenderer lineRenderer;
    private List<Vector3> nodePositions = new List<Vector3>();

    // Awake is called before the first frame update
    void Awake()
    {
        Random.InitState(randomSeed); // Initialize the random number generator with a seed for consistency
        Vector3 initialPosition = mapNodeTransform.position;

        // Load game data to determine the highest unlocked level
        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

        // Setup LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = levelList.Length;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;

        for (int i = 0; i < levelList.Length; i++)
        {
            Vector3 newPosition;
            bool isTooClose;
            int attempt = 0;

            do
            {
                isTooClose = false;
                // Create random variation in position
                Vector3 variation = new Vector3(
                    Random.Range(-maxPositionVariation, maxPositionVariation),
                    0,
                    Random.Range(-maxPositionVariation, maxPositionVariation)
                );
                newPosition = initialPosition + new Vector3(5 * i, 0, 0) + variation;

                // Check that the newPosition is not too close to other nodes
                foreach (var pos in nodePositions)
                {
                    if (Vector3.Distance(newPosition, pos) < minDistanceApart)
                    {
                        isTooClose = true;
                        break; // Break out of the foreach loop
                    }
                }

                attempt++;
                if (attempt > 100) // Prevent an infinite loop in case a suitable position cannot be found
                {
                    Debug.LogError("Could not find a suitable position for the node that isn't too close to others.");
                    break;
                }
            }
            while (isTooClose);

            if (!isTooClose)
            {
                // Instantiate the node
                GameObject newNode = Instantiate(mapNode, newPosition, Quaternion.identity);
                newNode.GetComponent<EnemySelection>().EnemyTypeIds = levelList[i].EnemyTypeIds;
                newNode.GetComponent<EnemySelection>().EnemyCoordinates = levelList[i].UnitCoordinates;
                newNode.GetComponent<EnemySelection>().levelData = levelList[i];
                newNode.GetComponent<EnemySelection>().levelNumber = i; // Assign level number

                // Update the material color and lock status based on level progression
                if (i == highestUnlockedLevel)
                {
                    newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                    newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
                }
                else
                {
                    newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                    newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelLocked;
                }

                // Add the position to the list of node positions
                nodePositions.Add(newPosition);

                // Update the LineRenderer with the new node position
                lineRenderer.SetPosition(i, newPosition);
            }
        }
    }
}