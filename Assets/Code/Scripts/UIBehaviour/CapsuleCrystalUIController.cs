using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CapsuleCrystalUIController : MonoBehaviour
{
    string leftMouseButtonInstructionsText = "LMB - Select/Confirm Placement Tile";
    string rightMouseButtonInstructionsText = "RMB - Deselect Placement Tile";
    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        if (scene.name == "battle_prototype" || scene.name == "boss_battle_prototype")
        {
            spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;
        }
    }

    public GameObject capsuleCrystalButtonPrefab;
    public Transform spellMenuContainer;

    public void AddPlaceCaptureCrystalButton()
    {
        //Instantiates the Trap Button.
        GameObject captureCrystalButtonInstance = Instantiate(capsuleCrystalButtonPrefab, spellMenuContainer);
        Button currentCaptureCrystalButton = captureCrystalButtonInstance.GetComponent<Button>();
        currentCaptureCrystalButton.onClick.AddListener(() => SwitchTilesToPlaceCaptureCrystal());
        currentCaptureCrystalButton.onClick.AddListener(() => GridManager.Instance.ClearPath());
    }
    public void SwitchTilesToPlaceCaptureCrystal()
    {
        MoveInfoController.Instance.HideActionInfoPanel();
        DestroyMagnet();
        DeactivateTrapSelection();

        // After clicking the Spell Button, all of the Grid Map tiles switch to Selection Mode and the Tile Controller current Action to Trap
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new PlaceCrystalPlayerAction();
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            if (tile.currentSingleTileCondition != SingleTileCondition.occupiedByDeity)
            {
                tile.gameObject.GetComponentInChildren<TileShaderController>().AnimateFadeHeight(0, 0.2f, Color.white);
            }
            Debug.Log("Switching tiles to Place Crystal Mode");

            GridManager.Instance.tileSelectionPermitted = true;
            UpdateInstructionsPanel();
        }
    }

    private void UpdateInstructionsPanel()
    {
        InstructionsPanelController.Instance.UpdateInstructions(leftMouseButtonInstructionsText, rightMouseButtonInstructionsText);
    }

    private void DeactivateTrapSelection()
    {
        GridManager.Instance.RemoveTrapSelection();
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
