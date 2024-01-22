using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GridTargetingController;

public class UnitSelectionController : MonoBehaviour
{
    public enum UnitSelectionStatus
    {
        unitSelected,
        unitDeselected,
        unitWaiting
    }
    //public delegate void ActiveCharacterSelected();
    //public static event ActiveCharacterSelected OnActiveCharacterSelected;

    //public delegate void ActiveCharacterDeselected();
    //public static event ActiveCharacterDeselected OnActiveCharacterDeselected;

    public delegate void UnitWaiting();
    public static event UnitWaiting OnUnitWaiting;

    public GameObject activeCharacterSelectorIcon;
    public GameObject moveButton;
    public GameObject waitButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpellUIController unitSpellUIController;

    public void Start()
    {
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }

    public void ResetUnitSelection()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }

    public void GenerateGameplayButtons()
    {
        GenerateMoveButton();
        GenerateWaitButton();
    }
    public void GenerateMoveButton()
    {
        GameObject newMoveButton = Instantiate(moveButton, unitSpellUIController.spellMenuContainer);
    }
    public void GenerateWaitButton()
    {
        GameObject newWaitButton = Instantiate(waitButton, unitSpellUIController.spellMenuContainer);
    }
    public void StopUnitAction()
    {
        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitOpportunityPoints = 0;
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;
        currentUnitSelectionStatus = UnitSelectionStatus.unitWaiting;
        OnUnitWaiting();
    }
}
