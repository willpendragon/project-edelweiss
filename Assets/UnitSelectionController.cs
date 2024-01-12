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
        unitDeselected
    }
    public delegate void ActiveCharacterSelected();
    public static event ActiveCharacterSelected OnActiveCharacterSelected;
    
    public delegate void ActiveCharacterDeselected();
    public static event ActiveCharacterDeselected OnActiveCharacterDeselected;
    
    public GameObject activeCharacterSelectorIcon;
    public GameObject moveButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpellUIController unitSpellUIController;

    public void Start()
    {
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }
    private void OnMouseDown()
    {
        if (currentUnitSelectionStatus == UnitSelectionStatus.unitDeselected && GridManager.Instance.currentPlayerUnit == null)
        {
            this.gameObject.tag = "ActivePlayerUnit";
            GameObject newActiveCharacterSelectorIcon = Instantiate(activeCharacterSelectorIcon, transform.localPosition + (transform.up * 3), Quaternion.identity);
            GenerateMoveButton();
            currentUnitSelectionStatus = UnitSelectionStatus.unitSelected;
            GridManager.Instance.currentPlayerUnit = this.gameObject;
            unitSpellUIController.PopulateCharacterSpellsMenu();
            OnActiveCharacterSelected();
        }
        else if (currentUnitSelectionStatus == UnitSelectionStatus.unitSelected)
        {
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            unitSpellUIController.ResetCharacterSpellsMenu();
            this.gameObject.tag = "Player";
            GridManager.Instance.currentPlayerUnit = null;
            currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
            OnActiveCharacterDeselected();
        }
    }
    public void GenerateMoveButton()
    {
        GameObject newMoveButton = Instantiate(moveButton, unitSpellUIController.spellMenuContainer);
    }
}
