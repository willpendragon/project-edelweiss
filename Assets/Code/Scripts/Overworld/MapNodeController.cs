using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapNodeController : MonoBehaviour, IPointerClickHandler
{
    public EnemySelection enemySelection;
    [SerializeField] CanvasGroup _locationCanvas;
    [SerializeField] CanvasGroup _iconCanvas;
    [SerializeField] private OverworldMapUIController _mapMenuController;
    [SerializeField] private int _dayCost = 1; // The time entering this node subtracts adds to the Calendar.

    public enum LockStatus
    {
        levelLocked,
        levelUnlocked,
        levelCleared
    }

    public NodeType type;
    public LockStatus currentLockStatus;
    [SerializeField] List<Vector2> playerUnitsBossBattleStartingCoords;
    
    // Identifier for tracking the map progression graph
    [HideInInspector] public int nodeId;
    [HideInInspector] public OverworldMapGenerator mapGenerator;

    void Start()
    {
        SetCanvasVisibility(0f, false, false, Vector3.zero);
        if (currentLockStatus == LockStatus.levelUnlocked)
        {
            _iconCanvas.alpha = 1f;
        }
        else if (currentLockStatus == LockStatus.levelCleared)
        {
            _iconCanvas.alpha = 0f;
        }
        _mapMenuController = FindAnyObjectByType<OverworldMapUIController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (mapGenerator != null && mapGenerator.currentNodeId == this.nodeId)
            {
                OpenLocationEnterPanel();
            }
            else if (mapGenerator != null)
            {
                mapGenerator.MoveToNode(this.nodeId);
            }
        }
    }

    private void OpenLocationEnterPanel()
    {
        // Prevent access if the node is locked or already cleared
        if (currentLockStatus == LockStatus.levelLocked || currentLockStatus == LockStatus.levelCleared)
            return;

        SetCanvasVisibility(1f, true, true, Vector3.one);
        Time.timeScale = 0f;
        SetOverworldUIVisibility(0.8f);
        _mapMenuController.SetArrowsVisibility(0f);
    }

    public void CloseLocationEnterPanel()
    {
        SetCanvasVisibility(0f, false, false, Vector3.zero);
        SetOverworldUIVisibility(1f);
        Time.timeScale = 1f;
        _mapMenuController.SetArrowsVisibility(1f);
    }

    private void SetCanvasVisibility(float alpha, bool blocksRaycasts, bool isInteractable, Vector3 scale)
    {
        _locationCanvas.alpha = alpha;
        _locationCanvas.blocksRaycasts = blocksRaycasts;
        _locationCanvas.interactable = isInteractable;
        _locationCanvas.transform.localScale = scale;
    }

    public void HandleBattleEntry()
    {
        switch (type)
        {
            case NodeType.RegularBattle:
            case NodeType.PuzzleBattle:
            case NodeType.MinibossBattle:
            case NodeType.BossBattle:
                HandleRegularBattle();
                break;
        }
    }
    
    private void HandleRegularBattle()
    {
        NodesUnlockManager nodesUnlockManager = GameManager.Instance.NodesUnlockManager;
        if (nodesUnlockManager.MapData.levelType == MapData.LevelType.Puzzle)
        {
            nodesUnlockManager.SpendKeyResource();
        }

        Time.timeScale = 1f;
        enemySelection.SelectMapNode();
        GameManager.Instance.GetComponentInChildren<SceneLoader>().ChangeScene();
        OverworldMapManager.Instance.CalendarController.IncreaseDaysCounter(_dayCost);
    }

    private void SetOverworldUIVisibility(float alpha)
    {
        var mapMenuController = FindAnyObjectByType<OverworldMapUIController>();
        if (mapMenuController != null && mapMenuController.transform.GetComponent<CanvasGroup>() != null)
            mapMenuController.transform.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public void SetCleared()
    {
        currentLockStatus = LockStatus.levelCleared;
        if (_iconCanvas != null) _iconCanvas.alpha = 0f;
    }

    public void SetUnlocked()
    {
        currentLockStatus = LockStatus.levelUnlocked;
        if (_iconCanvas != null) _iconCanvas.alpha = 1f;
    }
}