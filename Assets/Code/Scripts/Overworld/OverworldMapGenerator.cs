using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMapGenerator : MonoBehaviour
{
    public List<Domain> domains = new List<Domain>();
    public GameObject mapNode;
    public Transform mapNodeTransform;
    public float maxPositionVariation = 2f;
    public float minDistanceApart = 3f; // Minimum distance between nodes.
    public int randomSeed = 12345; // Seed for the random number generator.

    public Transform currentMapNodeTransform;

    private LineRenderer lineRenderer;
    private List<Vector3> nodePositions = new List<Vector3>();

    public GameObject[] partyMemberIcons;
    public float iconZOffset = 1f; // This should be updated after clearing a domain.
    private int currentDomainId = 0;

    //void Awake()
    //{
    //    GenerateLevel(domains[currentDomainId]);
    //}

    public void GenerateLevel(Domain domainLevelSelection)
    {

        Random.InitState(randomSeed);
        Vector3 initialPosition = mapNodeTransform.position;

        // Load game data to determine the highest unlocked level.
        GameSaveData gameSaveData = SaveStateManager.LoadGame();
        int highestUnlockedLevel = gameSaveData.highestUnlockedLevel;

        // LineRenderer setup (pertains to node visualisation).
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = domainLevelSelection.levelList.Length;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;

        for (int i = 0; i < domainLevelSelection.levelList.Length; i++)
        {
            Vector3 newPosition;
            bool isTooClose;
            int attempt = 0;

            do
            {
                isTooClose = false;
                // Create a random variation for the Node positioning (pertains to visual).
                Vector3 variation = new Vector3(
                    Random.Range(-maxPositionVariation, maxPositionVariation),
                    0,
                    Random.Range(-maxPositionVariation, maxPositionVariation)
                );
                newPosition = initialPosition + new Vector3(5 * i, 0, 0) + variation;

                // Ensure that the newPosition is not too close to other nodes (pertains to visuals).
                foreach (var pos in nodePositions)
                {
                    if (Vector3.Distance(newPosition, pos) < minDistanceApart)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                attempt++;
                // Arbitrary number to avoid an infinite loop (if no suitable position for the node is found).
                if (attempt > 100)
                {
                    break;
                }
            }
            while (isTooClose);

            if (!isTooClose)
            {
                GameObject newNode = Instantiate(mapNode, newPosition, Quaternion.identity);
                newNode.GetComponent<EnemySelection>().enemyParty = domainLevelSelection.levelList[i].enemyPartyData;
                newNode.GetComponent<EnemySelection>().levelNumber = domainLevelSelection.levelList[i].levelNumber;

                // Unlocks levels based on the current state of level progression.
                if (i == highestUnlockedLevel)
                {
                    currentMapNodeTransform = newNode.transform;
                    UpdateNodeVisuals(newNode);
                    UnlockLevel(newNode);
                    UpdatePartyMemberVisuals(newNode);
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

    private void UpdatePartyMemberVisuals(GameObject mapNode)
    {
        Vector3 partyMemberIconPosition = mapNode.transform.position + new Vector3(0, 0, iconZOffset);
        float horizontalOffset = 2; // The horizontal offset distance between icons.
        float startOffset = -(partyMemberIcons.Length - 1) * horizontalOffset * 0.5f; // Align icons to the centre.

        for (int j = 0; j < partyMemberIcons.Length; j++)
        {
            // Calculate the offset of a single party member icon.
            Vector3 offsetPosition = new Vector3(startOffset + horizontalOffset * j, 0, 0);
            Instantiate(partyMemberIcons[j], partyMemberIconPosition + offsetPosition, Quaternion.identity);
        }
    }

    private void UnlockLevel(GameObject mapNode)
    {
        mapNode.GetComponentInChildren<MapNodeController>().currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
    }

    private void UpdateNodeVisuals(GameObject mapNode)
    {
        mapNode.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
}