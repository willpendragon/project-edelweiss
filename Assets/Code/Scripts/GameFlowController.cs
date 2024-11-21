using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] Transform bossLevelTowerSpawnpoint;
    [SerializeField] GameObject bossLevelTowerPrefab;
    [SerializeField] int bossLevelRequirement;
    [SerializeField] RectTransform endOfDemoPanel;

    void Start()
    {
        UnlockBossFight();

    }
    private void UnlockBossFight()
    {
        int currentSessionHighestUnlockedLevel = SaveStateManager.saveData.highestUnlockedLevel;
        if (currentSessionHighestUnlockedLevel == bossLevelRequirement)
        {
            //GameObject newBossLevelTower = Instantiate(bossLevelTowerPrefab, bossLevelTowerSpawnpoint);
            //Debug.Log("Boss Jacob's Ladder appears");
            endOfDemoPanel.localScale = Vector3.one;
        }
    }
}
