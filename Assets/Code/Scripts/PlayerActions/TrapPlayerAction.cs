using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TrapPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public float trapCreationCost = 5;
    public int trapCreationRange = 1;

    public delegate void TrapPlaced();
    public static event TrapPlaced OnTrapPlaced;

    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        TrapController selectedTiletrapController = selectedTile.GetComponentInChildren<TrapController>();
        TrapTileUIController trapTileUIController = activePlayerUnit.GetComponent<TrapTileUIController>();

        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();

        int distanceFromSelectedTile = gridMovementController.GetDistance(activePlayerUnit.ownedTile, selectedTile);

        if (distanceFromSelectedTile > trapCreationRange)
        {
            selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            Debug.Log("This tile is not available to set a trap");
            return;
        }
        else if (selectedTile != null && selectedTile.currentSingleTileCondition == SingleTileCondition.free)
        {
            if (selectedTiletrapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active && trapTileUIController.trapTileSelectionIsActive == true)
            {
                selectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                trapTileUIController.trapTileSelectionIsActive = false;
            }
        }
    }
    public void Deselect()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        TrapTileUIController trapTileUIController = activePlayerUnit.GetComponent<TrapTileUIController>();
        trapTileUIController.trapTileSelectionIsActive = true;

        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            BattleInterface.Instance.DeactivateActionInfoPanel();
            Debug.Log("Deselecting Currently Selected Tile");
        }
        else if (savedSelectedTile == null)
        {
            BattleInterface.Instance.DeactivateActionInfoPanel();
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentPlayerAction = new SelectUnitPlayerAction();
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
            GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
            foreach (var playerUISpellButton in playerUISpellButtons)
            {
                Destroy(playerUISpellButton);
            }
            Destroy(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel);
            GridManager.Instance.currentPlayerUnit.tag = "Player";
            GridManager.Instance.currentPlayerUnit = null;

            GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
            movesContainer.transform.localScale = new Vector3(0, 0, 0);
            Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
            GridManager.Instance.ClearPath();
            BattleInterface.Instance.DeactivateActionInfoPanel();

        }
    }
    public void Execute()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints == 0)
        {
            Debug.Log("Can't place Trap: No opportunity points left");
            return;
        }

        TrapController trapController = savedSelectedTile.GetComponentInChildren<TrapController>();

        if (trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active)
        {
            trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;

            GameObject newTrap = Instantiate((GameObject)Resources.Load("TrapTileVFX"), savedSelectedTile.transform);
            // Instantiate 3D Model

            activePlayerUnit.unitOpportunityPoints--;
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;

            if (activePlayerUnit.unitManaPoints - trapCreationCost >= 0)
            {
                activePlayerUnit.unitManaPoints -= trapCreationCost;
                activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
                ResetTrapSelectionLimiter();
            }
            else
            {
                Debug.Log("Can't place Trap: Not enough mana points");
                activePlayerUnit.unitOpportunityPoints++;
                ResetTrapSelectionLimiter();
            }
            OnTrapPlaced();
        }
    }
    public void ResetTrapSelectionLimiter()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        TrapTileUIController trapTileUIController = activePlayerUnit.GetComponent<TrapTileUIController>();
        trapTileUIController.trapTileSelectionIsActive = true;
    }
}
