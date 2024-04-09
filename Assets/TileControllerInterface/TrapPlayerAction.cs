using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrapPlayerAction : IPlayerAction
{

    public TileController savedSelectedTile;
    public int selectionLimiter = 1;

    public void Select(TileController selectedTile)
    {
        if (selectedTile != null)
        {
            if (selectedTile.currentSingleTileCondition == SingleTileCondition.free)
            {
                selectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.clear;
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
        TrapController trapController = savedSelectedTile.GetComponentInChildren<TrapController>();
        trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
        savedSelectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
        Debug.Log("This Tile is now a Trap");
    }

}
