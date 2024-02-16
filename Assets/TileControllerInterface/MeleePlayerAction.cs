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
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null)
        {
            currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();

            if (currentTarget.gameObject.tag == "Enemy")
            {
                selectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                //Switch Selected tile to WaitingForConfirmationStatus
                Debug.Log("Melee Select Logic");
            }
            else
            {
                Debug.Log("Can't Select Friendly Unit for Melee Attack");
            }

        }
    }

    public void Deselect()
    {
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
