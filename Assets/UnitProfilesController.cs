using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitProfilesController : MonoBehaviour
{
    GameObject newUnitPanel;
    public bool newUnitPanelExists;
    public static UnitProfilesController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void CreateEnemyUnitPanel(GameObject detectedUnit)
    {
        if (!newUnitPanelExists)
        {
            //Spawns an information panel with Active Character Unit details on the Lower Left of the Screen
            newUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
            newUnitPanel.tag = "Enemy";
            newUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = newUnitPanel;
            newUnitPanel.GetComponent<PlayerProfileController>().ApplyProfileChanges(detectedUnit, PlayerProfileController.ProfileOwner.enemyUnit);
            newUnitPanelExists = true;
        }
    }

    public void DestroyEnemyUnitPanel()
    {
        Destroy(newUnitPanel);
        newUnitPanelExists = false;
    }
    public void UpdateEnemyUnitPanel(GameObject detectedUnit)
    {
        detectedUnit.GetComponent<Unit>().unitProfilePanel.GetComponent<PlayerProfileController>().UpdateUnitProfile(detectedUnit);
    }
}