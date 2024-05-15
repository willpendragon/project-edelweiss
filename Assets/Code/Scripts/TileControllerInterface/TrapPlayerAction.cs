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

        TrapController trapController = savedSelectedTile.GetComponentInChildren<TrapController>();

        if (trapController.currentTrapActivationStatus != TrapController.TrapActivationStatus.active)
        {
            trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
            //savedSelectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
            GameObject newTrap = Instantiate((GameObject)Resources.Load("TrapTileVFX"), savedSelectedTile.transform);

            //Instantiate 3D Model Here

            activePlayerUnit.unitOpportunityPoints--;
            activePlayerUnit.unitManaPoints -= trapCreationCost;

            activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
            Debug.Log("This Tile is now a Trap");
        }
    }
}
