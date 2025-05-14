using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public UnitIconsController unitIconsController;

    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

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

    public void GenerateWaitButton()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (unitSpellUIController != null && sceneName != "battle_tutorial")
        {
            GameObject newWaitButton = Instantiate(waitButton, unitSpellUIController.spellMenuContainer);
        }
    }
    public void StopUnitAction()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        unitIconsController?.DisplayWaitingIcon();
        Debug.Log("Display Waiting Icon on Unit");

        unitSpellUIController.ResetCharacterSpellsMenu();
        this.gameObject.tag = "Player";

        GridManager.Instance.currentPlayerUnit = null;
        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
        OnUnitWaiting();

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
        }

        Button endTurnButton = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
        endTurnButton.interactable = true;

        GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ClearReachableTiles(0, 0.2F, Color.white);
    }
}
