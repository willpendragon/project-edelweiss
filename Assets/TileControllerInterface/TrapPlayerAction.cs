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
                selectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.clear;
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
        savedSelectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        savedSelectedTile = null;
        selectionLimiter++;
    }

    public void Execute()
    {
        TrapController trapController = savedSelectedTile.GetComponentInChildren<TrapController>();
        trapController.currentTrapActivationStatus = TrapController.TrapActivationStatus.active;
        savedSelectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        Debug.Log("This Tile is now a Trap");
    }

}
