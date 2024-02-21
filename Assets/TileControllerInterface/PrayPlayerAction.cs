using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrayPlayerAction : IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public delegate void PlayerPrayer();
    public static event PlayerPrayer OnPlayerPrayer;
    public void Select(TileController selectedTile)
    {
        if (selectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
        {
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            Debug.Log("Praying for the Selected Deity");
            savedSelectedTile = selectedTile;
            selectionLimiter--;
        }

    }

    public void Deselect()
    {
        savedSelectedTile = null;
        savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        selectionLimiter++;
    }

    public void Execute()
    {
        if (savedSelectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
        {
            OnPlayerPrayer();
        }
    }
}
