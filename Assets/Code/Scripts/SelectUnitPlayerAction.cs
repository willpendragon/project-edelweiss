using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectUnitPlayerAction : MonoBehaviour, IPlayerAction
{
    public GameObject newCurrentlySelectedUnitPanel;
    public GameObject selectedUnit;
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null && selectedTile.detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus != UnitSelectionController.UnitSelectionStatus.unitWaiting
            && selectedTile.detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected)
        {
            selectedUnit = selectedTile.detectedUnit;
            CreateActivePlayerUnitProfile(selectedUnit);
            //selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
            //selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
        }
    }

    public void Deselect()
    {
        selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
        GridManager.Instance.currentPlayerUnit.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;
        Destroy(newCurrentlySelectedUnitPanel);
        Debug.Log("Deselected Unit");
        ResetCharacterSpellsMenu();
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentSingleTileStatus = SingleTileStatus.basic;
            Debug.Log("Switching Tiles to Character Selection Mode");
        }
    }

    public void Execute()
    {
        //Debug.Log("Create Player Unit Profile");
        //CreateActivePlayerUnitProfile(selectedUnit);
    }

    public void CreateActivePlayerUnitProfile(GameObject detectedUnit)
    {
        //Spawns an information panel with Active Character Unit details on the Lower Left of the Screen

        newCurrentlySelectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
        newCurrentlySelectedUnitPanel.tag = "ActiveCharacterUnitProfile";
        newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;
        //The newly spawned Unit Profile Panel becomes the Detected Unit Profile Panel
        //OnClickedTileWithUnit(detectedUnit);
        //The UI Panel shows the detected Unit details
        Debug.Log("Clicked on a Tile with Player Unit on it");

        //Unit becomes the Active Player Unit in the GridManager
        GridManager.Instance.currentPlayerUnit = detectedUnit;
        //The Unit tag becomes ActivePlayerUnit
        detectedUnit.tag = "ActivePlayerUnit";
        detectedUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
        //Gameplay and Spells Buttons are generated
        detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
        detectedUnit.GetComponent<UnitSelectionController>().GenerateGameplayButtons();
        detectedUnit.GetComponent<MoveUIController>().AddMoveButton();
        detectedUnit.GetComponent<MeleeUIController>().AddMeleeButton();
        detectedUnit.GetComponent<SpellUIController>().PopulateCharacterSpellsMenu(detectedUnit);
        detectedUnit.GetComponent<TrapTileUIController>().AddTrapButton();
        detectedUnit.GetComponent<SummoningUIController>().AddSummonButton();
        detectedUnit.GetComponent<CapsuleCrystalUIController>().AddPlaceCaptureCrystalButton();
    }
    public void ResetCharacterSpellsMenu()
    {
        GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
        foreach (var playerUISpellButton in playerUISpellButtons)
        {
            Destroy(playerUISpellButton);
        }
    }
}
