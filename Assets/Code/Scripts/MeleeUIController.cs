using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MeleeUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public GameObject meleeButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;
    private int meleeRange = 2;
    string leftMouseButtonInstructionsText = "LMB - Select/Confirm Target";
    string rightMouseButtonInstructionsText = "RMB - Deselect Target";


    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype" || scene.name == "battle_tutorial")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }


    public void AddMeleeButton()
    {
        // Instantiate the Melee Button.

        GameObject meleeButtonInstance = Instantiate(meleeButtonPrefab, spellMenuContainer);
        Button currentMeleeButton = meleeButtonInstance.GetComponent<Button>();
        currentMeleeButton.onClick.AddListener(() => SwitchTilesToSelectionMode());
        currentMeleeButton.onClick.AddListener(() => MoveInfoController.Instance.DisplayActionInfoPanel());
        currentMeleeButton.onClick.AddListener(() => MoveInfoController.Instance.UpdateMeleeMoveInfoPanelTexts());

        Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (currentActiveUnit.hasHookshot == true)
        {
            currentMeleeButton.GetComponentInChildren<Text>().text = "Magnet";
        }
    }

    public void SwitchTilesToSelectionMode()
    {
        GridManager.Instance.ClearPath();
        MoveInfoController.Instance.HideActionInfoPanel();
        DeactivateTrapSelection();

        // After clicking the Melee Button, all of the Grid Map tiles switch to Selection Mode and switch to the Melee Player Action.

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new MeleePlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Switching tiles to Melee Mode");
            if (tile.currentSingleTileCondition != SingleTileCondition.occupiedByDeity)
            {
                tile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0, 0.2f, Color.white);
            }
            Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
            Color meleeTileVisualizingColor = new Color(0.75f, 0.1f, 0.1f);
            GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ShowTargetableTiles(currentActiveUnit, meleeRange, meleeTileVisualizingColor);
        }
        GridManager.Instance.tileSelectionPermitted = true;
        UpdateInstructionsPanel();
    }
    private void UpdateInstructionsPanel()
    {
        InstructionsPanelController.Instance.UpdateInstructions(leftMouseButtonInstructionsText, rightMouseButtonInstructionsText);
    }

    public void DeactivateTrapSelection()
    {
        GridManager.Instance.RemoveTrapSelection();
    }
}