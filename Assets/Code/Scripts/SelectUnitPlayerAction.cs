using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static PlayerProfileController;
using TMPro;

public class SelectUnitPlayerAction : MonoBehaviour, IPlayerAction
{
    public GameObject newCurrentlySelectedUnitPanel;
    public GameObject selectedUnit;
    private GameObject playerSelectorIconInstance;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null && selectedTile.detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus != UnitSelectionController.UnitSelectionStatus.unitWaiting
            && selectedTile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
        {
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                Debug.Log("Switching Tiles to Waiting for Confirmation Mode");
            }
            selectedTile.detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected;
            GameObject playerSelectorIconIstance = Instantiate(Resources.Load("PlayerCharacterSelectorIcon") as GameObject, selectedTile.detectedUnit.transform);
            //Beware: Magic Number
            playerSelectorIconIstance.transform.localPosition += new Vector3(0, 2.5f, 0);
            selectedUnit = selectedTile.detectedUnit;

            if (selectedTile.detectedUnit.GetComponent<BattleFeedbackController>() != null)
            {
                BattleFeedbackController battleFeedbackController = selectedTile.detectedUnit.GetComponent<BattleFeedbackController>();
                battleFeedbackController.PlaySelectionSFX.Invoke();
            }
        }
    }

    public void Deselect()
    {
        if (this.selectedUnit != null)
        {


            Debug.Log("Deselecting Unit");
            if (selectedUnit.tag != "Enemy")
            {
                selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
                if (GridManager.Instance.currentPlayerUnit != null)
                {
                    GridManager.Instance.currentPlayerUnit.tag = "Player";
                    GridManager.Instance.currentPlayerUnit = null;
                    Destroy(newCurrentlySelectedUnitPanel);
                    ResetCharacterSpellsMenu();

                    Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
                }
                else
                {
                    Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
                    selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;

                }
                foreach (var tile in GridManager.Instance.gridTileControllers)
                {
                    tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
                    Debug.Log("Switching Tiles back to Selection Mode");
                }
            }
            else if (selectedUnit.tag == "Enemy")
            {
                Debug.Log("Deselecting Unit");
                selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
                Destroy(newCurrentlySelectedUnitPanel);
                Debug.Log("Deselected Unit");
                ResetCharacterSpellsMenu();
                foreach (var tile in GridManager.Instance.gridTileControllers)
                {
                    tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
                    Debug.Log("Switching Tiles to Character Selection Mode");
                }
                Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            }

        }

    }
    public void Execute()
    {
        if (this.selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected)
            {
                CreateActivePlayerUnitProfile(selectedUnit);
                GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon").GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;

                if (selectedUnit.GetComponent<BattleFeedbackController>() != null)
                {
                    BattleFeedbackController battleFeedbackController = selectedUnit.GetComponent<BattleFeedbackController>();
                    battleFeedbackController.PlaySelectionWaitingConfirmationSFX.Invoke();
                }
            }

        }
        else
        {
            Debug.Log("No selectable Unit found");
        }
    }

    public void CreateActivePlayerUnitProfile(GameObject detectedUnit)
    {
        //Spawns an information panel with Active Character Unit details on the Lower Left of the Screen

        newCurrentlySelectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
        newCurrentlySelectedUnitPanel.tag = "ActiveCharacterUnitProfile";
        newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
        detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;
        //The newly spawned Unit Profile Panel becomes the Detected Unit Profile Panel
        OnClickedTileWithUnit(detectedUnit);


        //Call a method that popoulates the Active Player Unit Profile (detected Unit) details
        Debug.Log("Clicked on a Tile with Player Unit on it");

        //Unit becomes the Active Player Unit in the GridManager
        if (detectedUnit.tag == "Player")
        {
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
            detectedUnit.GetComponent<FlightUIController>().AddRunButton();

        }

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
