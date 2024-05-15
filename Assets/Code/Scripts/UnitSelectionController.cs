using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using static GridTargetingController;

public class UnitSelectionController : MonoBehaviour
{
    public enum UnitSelectionStatus
    {
        unitSelected,
        unitDeselected,
        unitTemporarilySelected,
        unitAttacking,
        unitWaiting
    }

    public delegate void UnitWaiting();
    public static event UnitWaiting OnUnitWaiting;

    public GameObject activeCharacterSelectorIcon;
    public GameObject moveButton;
    public GameObject waitButton;
    public UnitSelectionStatus currentUnitSelectionStatus;
    public SpellUIController unitSpellUIController;
    public SpriteRenderer unitSprite;

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

    //public void GenerateGameplayButtons()
    //{
    //    GenerateWaitButton();
    //}

    public void GenerateWaitButton()
    {
        if (unitSpellUIController != null)
        {
            GameObject newWaitButton = Instantiate(waitButton, unitSpellUIController.spellMenuContainer);
        }
    }
    public void StopUnitAction()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = "Player";
        //unitSprite.material.color = Color.grey;
        GridManager.Instance.currentPlayerUnit = null;
        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
        OnUnitWaiting();
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
        }
    }
}
