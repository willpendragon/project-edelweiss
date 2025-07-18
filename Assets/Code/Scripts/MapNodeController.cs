using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MapNodeController : MonoBehaviour, IPointerClickHandler
{
    public EnemySelection enemySelection;

    public enum LockStatus
    {
        levelLocked,
        levelUnlocked
    }

    public enum MapNodeType
    {
        RegularBattleNode,
        BossBattleNode
    }

    public MapNodeType type;
    public LockStatus currentLockStatus;
    [SerializeField] List<Vector2> playerUnitsBossBattleStartingCoords;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleTileSelection();
        }
    }

    public void HandleTileSelection()
    {
        if (currentLockStatus == LockStatus.levelLocked)
            return;

        switch (type)
        {
            case MapNodeType.RegularBattleNode:
                HandleRegularBattle();
                break;
            case MapNodeType.BossBattleNode:
                HandleBossBattle();
                break;
        }
    }
    private void HandleRegularBattle()
    {
        enemySelection.SelectMapNode();
        GameManager.Instance.GetComponentInChildren<SceneLoader>().ChangeScene();
    }
    private void HandleBossBattle()
    {

    }
}
