using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] Transform bossLevelTowerSpawnpoint;
    [SerializeField] GameObject bossLevelTowerPrefab;
    void Start()
    {
        UnlockBossFight();

    }

    private void UnlockBossFight()
    {
        int currentSessionHighestUnlockedLevel = SaveStateManager.saveData.highestUnlockedLevel;
        int bossLevelRequirement = 2;
        if (currentSessionHighestUnlockedLevel > bossLevelRequirement)
        {
            GameObject newBossLevelTower = Instantiate(bossLevelTowerPrefab, bossLevelTowerSpawnpoint);
            Debug.Log("Boss Jacob's Ladder appears");
        }
    }
}
