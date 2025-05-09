using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMapGenerator : MonoBehaviour
{
    public Level[] levelList;
    //public Level[] levelListMapTwo;
    public GameObject mapNode;
    public Transform mapNodeTransform;
    public float maxPositionVariation = 2f; // Maximum variation in position
    public float minDistanceApart = 3f; // Minimum distance between nodes
    public int randomSeed = 12345; // Seed for the random number generator

    public Transform currentMapNodeTransform;

    private LineRenderer lineRenderer;
    private List<Vector3> nodePositions = new List<Vector3>();

    public GameObject[] partyMemberIcons;
    public float iconZOffset = 1f;

    void Awake()
    {
        GenerateLevel(levelList);
    }

    void GenerateLevel(Level[] levelSelection)
    {

        Random.InitState(randomSeed); // Initialize the random number generator with a seed for consistency
        Vector3 initialPosition = mapNodeTransform.position;

        // Load game data to determine the highest unlocked level
        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

        // Setup LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = levelSelection.Length;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;

        for (int i = 0; i < levelSelection.Length; i++)
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
                newNode.GetComponent<EnemySelection>().levelData = levelSelection[i];
                newNode.GetComponent<EnemySelection>().levelNumber = levelSelection[i].levelNumber;

                // Update the material color and lock status based on level progression
                if (i == highestUnlockedLevel)
                {
                    currentMapNodeTransform = newNode.transform;
                    newNode.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                    newNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
                    Vector3 partyMemberIconPosition = newNode.transform.position + new Vector3(0, 0, iconZOffset);
                    float horizontalOffset = 2; // The horizontal offset distance between icons
                    float startOffset = -(partyMemberIcons.Length - 1) * horizontalOffset * 0.5f; // Center the icons

                    for (int j = 0; j < partyMemberIcons.Length; j++)
                    {
                        // Calculate the offset for this particular icon
                        Vector3 offsetPosition = new Vector3(startOffset + horizontalOffset * j, 0, 0);

                        // Instantiate the icon with the offset position
                        Instantiate(partyMemberIcons[j], partyMemberIconPosition + offsetPosition, Quaternion.identity);
                    }
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
