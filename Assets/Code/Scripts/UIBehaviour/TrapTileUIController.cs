using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrapTileUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public TrapTileController trapTileController;
    public GameObject trapButtonPrefab;
    public Transform spellMenuContainer;
    public bool trapTileSelectionIsActive;
    private int trapPlacementRange = 1;
    string leftMouseButtonInstructionsText = "LMB - Select/Confirm Placement Tile";
    string rightMouseButtonInstructionsText = "RMB - Deselect Tile";

    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    private void Start()
    {
        trapTileSelectionIsActive = true;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype" || scene.name == "battle_tutorial")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public void AddTrapButton()
    {
        // Instantiate the Trap Button.
        GameObject trapTileButtonInstance = Instantiate(trapButtonPrefab, spellMenuContainer);
        Button currentTrapTileButton = trapTileButtonInstance.GetComponent<Button>();
        currentTrapTileButton.onClick.AddListener(() => SwitchTilesToTrapMode());
        currentTrapTileButton.onClick.AddListener(() => MoveInfoController.Instance.UpdateTrapMoveInfoPanelTexts());
        currentTrapTileButton.onClick.AddListener(() => GridManager.Instance.ClearPath());
    }
    public void SwitchTilesToTrapMode()
    {
        MoveInfoController.Instance.HideActionInfoPanel();
        DestroyMagnet();

        // After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode and the Tile Controller current Action to Trap
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new TrapPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            if (tile.currentSingleTileCondition != SingleTileCondition.occupiedByDeity)
            {
                tile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0, 0.2f, Color.white);
            }
            Debug.Log("Switching tiles to Trap Mode");
        }

        Unit currentActiveUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Color trapPlacementTileVisualizingColor = Color.yellow;
        GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ShowTargetableTiles(currentActiveUnit, trapPlacementRange, trapPlacementTileVisualizingColor);

        GridManager.Instance.tileSelectionPermitted = true;
        UpdateInstructionsPanel();
    }
    private void UpdateInstructionsPanel()
    {
        InstructionsPanelController.Instance.UpdateInstructions(leftMouseButtonInstructionsText, rightMouseButtonInstructionsText);
    }

    void DestroyMagnet()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit != null && activePlayerUnit.hasHookshot == true)
        {
            MagnetHelper magnetHelper = activePlayerUnit.gameObject.GetComponentInChildren<MagnetHelper>();
            magnetHelper.DestroyMagnet();
        }
    }
}
