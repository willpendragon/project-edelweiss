using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnemyInfoPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject newCurrentlySelectedUnitPanel;
    [SerializeField] GameObject enemyGameObject;
    public delegate void HoverMouseOnEnemy(GameObject enemyGameObject);
    public static event HoverMouseOnEnemy OnHoverMouseOnEnemy;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GridManager.Instance.currentPlayerUnit == null)
        {
            ShowEnemyInfo();
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyEnemyUnitProfile();
    }
    public void ShowEnemyInfo()
    {
        CreateEnemyUnitProfile(enemyGameObject);
        Debug.Log("Showing Enemy Information");
    }
    private void CreateEnemyUnitProfile(GameObject hoveredEnemyGameObject)
    {
        // Spawns an information panel with Active Character Unit details on the Lower Left of the Screen.
        newCurrentlySelectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
        newCurrentlySelectedUnitPanel.tag = "ActiveCharacterUnitProfile";
        newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        enemyGameObject.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;
        OnHoverMouseOnEnemy(hoveredEnemyGameObject);
    }
    public void DestroyEnemyUnitProfile()
    {
        Destroy(newCurrentlySelectedUnitPanel);
    }
}