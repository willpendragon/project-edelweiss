using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyController : MonoBehaviour
{
    public GameObject[] playerUnitsOnBattlefield;

    private void Awake()
    {
        // 1. Explicitly fetch the exact active party instances from GameManager, preserving the recruit order
        List<GameObject> activeUnitGOs = new List<GameObject>();
        if (GameManager.Instance != null && GameManager.Instance.playerPartyMembersInstances != null)
        {
            foreach (Unit u in GameManager.Instance.playerPartyMembersInstances)
            {
                if (u != null) activeUnitGOs.Add(u.gameObject);
            }
        }
        else
        {
            // Fallback for editor testing without standard GameManager flow
            activeUnitGOs.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        }

        playerUnitsOnBattlefield = activeUnitGOs.ToArray();

        // 2. Fetch the painted spawn slots directly from the Map Data
        List<MapData.SpawnData> spawnPoints = (GameManager.Instance != null && GameManager.Instance.CurrentMap != null) 
            ? GameManager.Instance.CurrentMap.playerSpawnPositions 
            : new List<MapData.SpawnData>();

        // 3. Assign coordinates dynamically based on deployment index
        for (int i = 0; i < playerUnitsOnBattlefield.Length; i++)
        {
            Unit playerUnit = playerUnitsOnBattlefield[i].GetComponent<Unit>();
            if (playerUnit != null)
            {
                if (i < spawnPoints.Count)
                {
                    // Map Editor Z axis translates to Grid Map Y axis (horizontal plane depth)
                    playerUnit.startingXCoordinate = spawnPoints[i].position.x;
                    playerUnit.startingYCoordinate = spawnPoints[i].position.z;
                }
                else
                {
                    Debug.LogWarning($"Not enough player spawn points painted on the map for Unit: {playerUnit.name}. Check MapEditor!");
                }

                // Lock in the tile logic for other scripts initializing simultaneously
                TileController playerUnitTileController = GridManager.Instance.GetTileControllerInstance(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
                if (playerUnitTileController != null)
                {
                    playerUnitTileController.detectedUnit = playerUnitsOnBattlefield[i];
                    playerUnit.ownedTile = playerUnitTileController;
                }
            }
        }
    }
}