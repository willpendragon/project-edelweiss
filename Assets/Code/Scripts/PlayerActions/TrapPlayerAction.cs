using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrapPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public float trapCreationCost = 5;

    public void Select(TileController selectedTile)
    {
        TrapController selectedTiletrapController = selectedTile.GetComponentInChildren<TrapController>();
        if (selectedTile != null && selectedTile.currentSingleTileCondition == SingleTileCondition.free)
        {
            if (selectedTiletrapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active)
            {
                selectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                selectionLimiter--;
            }
        }
        else
        {
            Debug.Log("This tile is not available to set a trap");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            Debug.Log("Deselecting Currently Selected Tile");
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
            if (activePlayerUnit.unitManaPoints - trapCreationCost >= 0)
            {
                activePlayerUnit.unitManaPoints -= trapCreationCost;
                activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
            }
            else
            {
                Debug.Log("Can't place Trap: Not enough mana points");
                // Optionally, you might want to revert the opportunity points deduction if the trap placement fails.
                activePlayerUnit.unitOpportunityPoints++;
            }
        }
    }
}
