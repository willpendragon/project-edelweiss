using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static GridTargetingController;
using static TileController;
using UnityEngine.UI;
using Unity.VisualScripting.ReorderableList;
using System.Security.Cryptography;

public class MovePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    private int destinationTileXCoordinate;
    private int destinationTileYCoordinate;
    public void Select(TileController selectedTile)
    {
        if (selectedTile != null && selectionLimiter == 1 && selectedTile != GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile
            && selectedTile.detectedUnit == null)
        //Beware: Magic Number
        {
            var destinationTile = selectedTile;
            //Check if the distance is available
            //If yes, check distance and create the path over the grid
            destinationTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.blue;
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
        //If a saved destination doesn't esist, by clicking on THAT tile, this will get back to selection mode
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
            //RevertToSelectionUnitPlayerAction();
        }
        else if (GridManager.Instance.currentPlayerUnit != null & savedSelectedTile == null)
        {
            Debug.Log("Resetting Current Active Player Selection");
            RevertToSelectionUnitPlayerAction();
        }
        //If a saved destination exist, by clicking on any tile, it will call the RevertToSelectionUnitPlayerAction method
        else if (savedSelectedTile == null)
        {
            RevertToSelectionUnitPlayerAction();
        }
    }
    public void Execute()
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        //Retrieve Active Current Player
        activePlayerUnit.unitOpportunityPoints--;

        if (activePlayerUnit.unitOpportunityPoints > 0 && activePlayerUnit.GetComponent<UnitStatusController>().unitCurrentStatus != UnitStatus.stun
            && savedSelectedTile.currentSingleTileCondition == SingleTileCondition.free)
        {

            //Use Grid Logic to Move the Player to Destination            
            activePlayerUnit.GetComponentInChildren<Animator>().SetTrigger(FindAnimationTrigger(activePlayerUnit, savedSelectedTile));
            activePlayerUnit.ownedTile.detectedUnit = null;
            activePlayerUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            activePlayerUnit.MoveUnit(destinationTileXCoordinate, destinationTileYCoordinate);
            //activePlayerUnit.SetPosition(destinationTileXCoordinate, destinationTileYCoordinate);
            GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
            activePlayerUnit.ownedTile = savedSelectedTile;
            activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;

            UpdateActivePlayerUnitProfile(activePlayerUnit);

            Debug.Log("Moving Character Execution Logic");
        }
        else
        {
            Debug.Log("Not enough Opportunity Points on Active Player Unit");
        }
    }

    public void RevertToSelectionUnitPlayerAction()
    {
        //Loops through all the grids on the Grid Map, reactivates the select Player Unit Action Mode, deactivates the spell menu, resetc the current Active Player Unit
        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new SelectUnitPlayerAction();
        }
        GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
        foreach (var playerUISpellButton in playerUISpellButtons)
        {
            Destroy(playerUISpellButton);
        }
        Destroy(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel);
        GridManager.Instance.currentPlayerUnit.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;
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
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }
}
