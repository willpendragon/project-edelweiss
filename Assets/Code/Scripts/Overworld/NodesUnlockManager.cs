using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodesUnlockManager : MonoBehaviour
{
    [SerializeField] private GameObject _mapNode;
    [SerializeField] private EnemyPartyData _enemies;
    [SerializeField] private MapData _mapData;

    void Start()
    {
        UnlockSecretNodes();
    }

    private void UnlockSecretNodes()
    {
        if (IsLevelKeyAvailable() == false)
            return;
        //SpendKeyResource();
        GenerateNode();
    }

    private bool IsLevelKeyAvailable()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;

        if (gameSaveData == null)
            return false;

        int unlockedLevelsKeys = gameSaveData.resourceData.puzzleLevelKeys;

        if (unlockedLevelsKeys >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void GenerateNode()
    {
        // Spawn the Node GameObject
        GameObject unlockedNode = Instantiate(_mapNode);
        // Add properties to it using Scriptable Object (hard-coded is OK for demo)
        var nodeController = unlockedNode.GetComponent<MapNodeController>();
        nodeController.currentLockStatus = MapNodeController.LockStatus.levelUnlocked;
        var enemySelection = unlockedNode.GetComponent<EnemySelection>();
        enemySelection.enemyParty = _enemies;
        enemySelection.mapData = _mapData;

    }
    private void SpendKeyResource()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        gameSaveData.resourceData.puzzleLevelKeys--;
        SaveStateManager.SaveGame(gameSaveData);
    }
}