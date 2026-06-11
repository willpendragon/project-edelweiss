using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using ProjectEdelweiss.Utils;

public class EnemyInfoPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _enemyUnitPanel;
    [SerializeField] private GameObject _enemyGameObject;
    public delegate void HoverMouseOnEnemy(GameObject enemyGameObject);
    public static event HoverMouseOnEnemy OnHoverMouseOnEnemy;
    public void OnPointerEnter(PointerEventData eventData)
    {
        //ShowEnemyInfo();
        //Debug.Log("Display Enemy Info");
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyEnemyUnitProfile();
    }
    public void ShowEnemyInfo()
    {
        if (_enemyGameObject != null && CheckEnemyStatus(_enemyGameObject))
        {
            CreateEnemyUnitProfile(_enemyGameObject);
            Debug.Log($"Showing {_enemyGameObject} Information");
        }
    }
    private bool CheckEnemyStatus(GameObject enemyGameObject)
    {
        Unit enemyGOUnit = enemyGameObject?.GetComponent<Unit>();
        if (enemyGOUnit == null || enemyGOUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            return false;
        else
            return true;
    }
    private void CreateEnemyUnitProfile(GameObject hoveredEnemyGameObject)
    {
        //_enemyUnitPanel = Instantiate(Resources.Load(GameTags.ENEMY_PROFILE) as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
        //_enemyUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        //_enemyGameObject.GetComponent<Unit>().unitProfilePanel = _enemyUnitPanel;
        //_enemyUnitPanel.GetComponent<UnitProfileController>().ApplyProfileChanges(_enemyGameObject);
    }
    public void DestroyEnemyUnitProfile()
    {
        Destroy(_enemyUnitPanel);
    }
}