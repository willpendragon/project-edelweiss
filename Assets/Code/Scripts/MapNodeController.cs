using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MapNodeController : MonoBehaviour, IPointerClickHandler
{
    public EnemySelection enemySelection;
    //public CheckRequirement checkRequirement;

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
        if (type == MapNodeType.RegularBattleNode && currentLockStatus == LockStatus.levelUnlocked)
        {
            enemySelection.SelectMapNode();
            GameManager.Instance.currentEnemySelectionComponent = GetComponentInParent<EnemySelection>();
            GameManager.Instance.GetComponentInChildren<SceneLoader>().ChangeScene();
            Debug.Log("Player chose to enter in Regular Battle");
        }
        else if (type == MapNodeType.BossBattleNode && currentLockStatus == LockStatus.levelUnlocked)
        {
            //enemySelection.SelectMapNode();
            Debug.Log("Player chose to enter in a Boss Battle");
            GameObject[] bossBattleCurrentEnemySelection = GetComponentInParent<EnemySelection>().enemySelection;
            List<Vector2> bossBattleEnemySelectionCoords = GetComponentInParent<EnemySelection>().EnemyCoordinates;

            GameManager.Instance.currentEnemySelection = bossBattleCurrentEnemySelection;
            GameManager.Instance.currentEnemySelectionCoords = bossBattleEnemySelectionCoords;

            Debug.Log("Load Boss Battle Sequence with handpicked Enemy Selection");
            SceneManager.LoadScene("boss_battle_prototype");
            //GameManager.Instance.GetComponentInChildren<SceneLoader>().ChangeScene();
        }
    }
}
