using UnityEngine;
using UnityEngine.UI;

public class SelectUnitPlayerAction : MonoBehaviour, IPlayerAction
{
    public GameObject newCurrentlySelectedUnitPanel;
    public GameObject selectedUnit;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;

    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    public void Select(TileController selectedTile)
    {
        if (!IsSelectionValid(selectedTile)) return;
        {
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                Debug.Log("Switching Tiles to Waiting for Confirmation Mode");
            }
            selectedTile.detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected;
            GameObject playerSelectorIconIstance = Instantiate(Resources.Load("PlayerCharacterSelectorIcon") as GameObject, selectedTile.detectedUnit.transform);

            Vector3 playerSelectionInstanceOffset = new Vector3(0, 2.5f, 0);
            playerSelectorIconIstance.transform.localPosition += playerSelectionInstanceOffset;

            selectedUnit = selectedTile.detectedUnit;

            if (selectedTile.detectedUnit.GetComponent<BattleFeedbackController>() != null)
            {
                BattleFeedbackController battleFeedbackController = selectedTile.detectedUnit.GetComponent<BattleFeedbackController>();
                battleFeedbackController.PlaySelectionSFX.Invoke();
            }
        }
    }
    private bool IsSelectionValid(TileController selectedTile)
    {
        if (selectedTile == null) return false;

        UnitSelectionController unitSelection = selectedTile.detectedUnit.GetComponent<UnitSelectionController>();
        Unit unit = selectedTile.detectedUnit.GetComponent<Unit>();

        if (unitSelection == null || unit == null) return false;
        if (unitSelection.currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting) return false;
        if (unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitAlive) return false;
        if (unit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless)
        {
            // Display negative feedback for invalid Selection.
            Debug.Log("Can't select Faithless Unit");
            return false;
        }
        if (selectedTile.detectedUnit.CompareTag("Enemy")) return false;
        if (GridManager.Instance.currentPlayerUnit != null) return false;

        return true;
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

                Button endTurnButton = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
                endTurnButton.interactable = true;
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
        selectedUnit.GetComponent<Unit>().ownedTile.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ClearReachableTiles(0, 0.2f, Color.white);
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

                Button endTurnButton = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
                endTurnButton.interactable = false;
            }
        }
        else
        {
            Debug.Log("No selectable Unit found");
        }
    }
    public void CreateActivePlayerUnitProfile(GameObject detectedUnit)
    {
        if (detectedUnit.tag == "Player" && GridManager.Instance.currentPlayerUnit == null)
        {
            // Spawns an information panel with Active Character Unit details on the Lower Left of the Screen.
            newCurrentlySelectedUnitPanel = Instantiate(Resources.Load("CurrentlySelectedUnit") as GameObject, GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform);
            newCurrentlySelectedUnitPanel.tag = "ActiveCharacterUnitProfile";
            newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;

            // The newly spawned Unit Profile Panel becomes the Detected Unit Profile Panel.
            OnClickedTileWithUnit(detectedUnit);
            Debug.Log("Clicked on a Tile with Unit standing on it");

            // If the Unit is a Player Unit, it becomes the Active Player Unit in the GridManager.
            GridManager.Instance.currentPlayerUnit = detectedUnit;
            GridManager.Instance.tileSelectionPermitted = true;
            // The Unit GameObject tag becomes "ActivePlayerUnit".
            detectedUnit.tag = "ActivePlayerUnit";
            detectedUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;

            // Gameplay and Spells Buttons generation.
            GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
            movesContainer.transform.localScale = new Vector3(0.9521077f, 0.9521077f, 0.9521077f);

            detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
            detectedUnit.GetComponent<UnitSelectionController>().GenerateWaitButton();
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

        GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
        movesContainer.transform.localScale = new Vector3(0, 0, 0);
    }
}