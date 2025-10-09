using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapNodeController : MonoBehaviour, IPointerClickHandler
{
    public EnemySelection enemySelection;
    [SerializeField] CanvasGroup _locationCanvas;
    [SerializeField] CanvasGroup _iconCanvas;
    [SerializeField] private OverworldMapUIController _mapMenuController;

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

    void Start()
    {
        SetCanvasVisibility(0f, false, false, Vector3.zero);
        if (currentLockStatus == LockStatus.levelUnlocked)
        {
            _iconCanvas.alpha = 1f;
        }
        _mapMenuController = FindAnyObjectByType<OverworldMapUIController>();

    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OpenLocationEnterPanel();
        }
    }

    // UI Managements method must be moved in a dedicated class.

    private void OpenLocationEnterPanel()
    {
        if (currentLockStatus == LockStatus.levelLocked)
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
            case MapNodeType.RegularBattleNode:
                HandleRegularBattle();
                break;
                //case MapNodeType.BossBattleNode:
                //    HandleBossBattle();
                //    break;
        }
    }
    private void HandleRegularBattle()
    {
        Time.timeScale = 1f;
        enemySelection.SelectMapNode();
        GameManager.Instance.GetComponentInChildren<SceneLoader>().ChangeScene();
    }

    private void SetOverworldUIVisibility(float alpha)
    {
        var mapMenuController = FindAnyObjectByType<OverworldMapUIController>();
        mapMenuController.transform.GetComponent<CanvasGroup>().alpha = alpha;
    }

    //private void HandleBossBattle()
    //{

    //}
}