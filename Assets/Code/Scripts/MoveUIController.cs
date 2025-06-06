using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveUIController : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public GameObject moveButtonPrefab;
    public Transform spellMenuContainer;
    private string buttonName;
    string leftMouseButtonInstructionsText = "LMB - Select/Confirm Destination Tile";
    string rightMouseButtonInstructionsText = "RMB - Deselect Destination Tile";


    public const string reachableTilesVisualizer = "ReachableTilesVisualizer";

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype" || scene.name == "battle_tutorial")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public void AddMoveButton()
    {
        //Instantiates the Move Button
        GameObject moveButtonInstance = Instantiate(moveButtonPrefab, spellMenuContainer);
        Button currentMoveButton = moveButtonInstance.GetComponent<Button>();
        currentMoveButton.onClick.AddListener(() => SwitchTilesToMoveMode());
    }

    public void SwitchTilesToMoveMode()
    {
        MoveInfoController.Instance.HideActionInfoPanel();
        DestroyMagnet();
        DeactivateTrapSelection();

        // Creates a new instance of the Move Player Action on each Tile
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new MovePlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;

            Debug.Log("Switching Tiles to Move Mode");
        }
        ResetTileControllersGlow();
        GameObject.FindGameObjectWithTag(reachableTilesVisualizer).GetComponent<ReachableTilesVisualizer>().ShowReachableTiles();
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
    public void ResetTileControllersGlow()
    {
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            if (tile.currentSingleTileCondition != SingleTileCondition.occupiedByDeity)
            {
                Debug.Log("Resetting Tile Glow");
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
        }
    }
    public void DeactivateTrapSelection()
    {
        GridManager.Instance.RemoveTrapSelection();
    }
}