using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;
using static TileController;
using UnityEngine.UI;


public class MeleePlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null && selectionLimiter == 1)
        {
            currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();

            if (currentTarget.gameObject.tag == "Enemy")
            {
                selectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                //Switch Selected tile to WaitingForConfirmationStatus
                Debug.Log("Melee Select Logic");
                selectionLimiter--;
            }
        }
        else
        {
            Debug.Log("Can't Select Unit");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;
        savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
        savedSelectedTile = null;
    }
    public void Execute()
    {
        currentTarget.HealthPoints -= 10;
        Debug.Log("Melee Execution Logic");
    }
}
