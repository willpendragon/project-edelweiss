using UnityEngine;
using UnityEngine.UI;
using Edelweiss.Core;
using ProjectEdelweiss.Utils;

public class SelectUnitPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public GameObject newCurrentlySelectedUnitPanel;
    public GameObject selectedUnit;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;

    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    public delegate void FaithlessCharacter(string faithlessCharacterMessage);
    public static event FaithlessCharacter OnFaithlessCharacter;

    public static class Tags
    {
        public const string ACTIVE_PLAYER_UNIT_ICON = "ActivePlayerCharacterSelectionIcon";
    }
    public void Select(TileController selectedTile)
    {
    }

    public void Deselect()
    {

    }
    public void Execute(TileController targetTile)
    {
        if (this.selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitTemporarilySelected)
            {
                CreateActivePlayerUnitProfile(selectedUnit);
                //GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon").GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;

                if (selectedUnit.GetComponent<BattleFeedbackController>() != null)
                {
                    BattleFeedbackController battleFeedbackController = selectedUnit.GetComponent<BattleFeedbackController>();
                    battleFeedbackController.PlaySelectionWaitingConfirmationSFX.Invoke();
                }

                Button endTurnButton = GameObject.FindGameObjectWithTag(GameTags.END_TURN_BUTTON).GetComponent<Button>();
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
        if (detectedUnit.tag == GameTags.Player && GridManager.Instance.currentPlayerUnit == null)
        {
            // Spawns an information panel with Active Character Unit details on the Lower Left of the Screen.
            newCurrentlySelectedUnitPanel = Instantiate(Resources.Load(GameTags.CurrentlySelectedUnit) as GameObject, GameObject.FindGameObjectWithTag(GameTags.BattleInterfaceCanvas).transform);
            newCurrentlySelectedUnitPanel.tag = GameTags.ActiveCharacterUnitProfile;
            newCurrentlySelectedUnitPanel.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerLeft;
            detectedUnit.GetComponent<Unit>().unitProfilePanel = newCurrentlySelectedUnitPanel;

            // If the Unit is a Player Unit, it becomes the Active Player Unit in the GridManager.
            GridManager.Instance.currentPlayerUnit = detectedUnit;
            GridManager.Instance.tileSelectionPermitted = true;
            // The Unit GameObject tag becomes "ActivePlayerUnit".
            detectedUnit.tag = GameTags.ActivePlayerUnit;
            detectedUnit.GetComponent<Unit>().ownedTile.currentSingleTileStatus = SingleTileStatus.selectedPlayerUnitOccupiedTile;
            // Set Unit as Selected.
            detectedUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitSelected;
        }
    }
}