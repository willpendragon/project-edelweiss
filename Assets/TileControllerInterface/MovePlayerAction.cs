using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;
using static TileController;
using UnityEngine.UI;
using Unity.VisualScripting.ReorderableList;
using System.Security.Cryptography;

public class MovePlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    private int destinationTileXCoordinate;
    private int destinationTileYCoordinate;
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null && selectionLimiter == 1)
        {
            var destinationTile = selectedTile;
            //Check if the distance is available
            //If yes, check distance and create the path over the grid
            destinationTile.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
            destinationTileXCoordinate = destinationTile.tileXCoordinate;
            destinationTileYCoordinate = destinationTile.tileYCoordinate;
            savedSelectedTile = selectedTile;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;

            selectionLimiter--;
            Debug.Log("Move Destination Selection Logic");
        }
        else
        {
            Debug.Log("Can't Select Destination");
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
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        //Retrieve Active Current Player

        if (activePlayerUnit.unitOpportunityPoints > 0)
        {
            //Use Grid Logic to Move the Player to Destination            
            activePlayerUnit.GetComponentInChildren<Animator>().SetTrigger(FindAnimationTrigger(activePlayerUnit, savedSelectedTile));
            activePlayerUnit.ownedTile.detectedUnit = null;
            activePlayerUnit.MoveUnit(destinationTileXCoordinate, destinationTileYCoordinate);
            activePlayerUnit.SetPosition(destinationTileXCoordinate, destinationTileYCoordinate);
            activePlayerUnit.ownedTile = savedSelectedTile;
            activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;

            activePlayerUnit.unitOpportunityPoints--;
            Debug.Log("Moving Character Execution Logic");
        }
        else
        {
            Debug.Log("Not enough Opportunity Points on Active Player Unit");
        }
    }

    public string FindAnimationTrigger(Unit activePlayerUnit, TileController destinationTile)
    {
        if (activePlayerUnit.ownedTile.transform.localPosition.x > destinationTile.transform.localPosition.x)
        {

            Debug.Log("Changing Animation");
            return "leftAnimationTrigger";
        }
        else
        {
            return null;
        }
    }
}
