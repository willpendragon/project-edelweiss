using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapNodeController : MonoBehaviour, IPointerClickHandler
{
    public EnemySelection enemySelection;
    public CheckRequirement checkRequirement;

    public enum LockStatus
    {
        levelLocked,
        levelUnlocked
    }

    public LockStatus currentLockStatus;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleTileSelection();
            Debug.Log("Left Click Registered");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //HandleTileDeselection();
        }
    }
    public void HandleTileSelection()
    {
        if (currentLockStatus == LockStatus.levelUnlocked)
        {
            enemySelection.SelectedMapNode();
            //checkRequirement.CheckPreviousNode();
            //checkRequirement.SetCurrentNode();*/
            GameManager._instance.currentEnemySelectionComponent = GetComponentInParent<EnemySelection>();
            GameManager._instance.GetComponentInChildren<SceneLoader>().ChangeScene();
        }
    }
}
