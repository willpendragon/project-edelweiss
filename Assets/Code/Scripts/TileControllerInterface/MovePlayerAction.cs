using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TileController;
using UnityEngine.UI;

public class MovePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    private int destinationTileXCoordinate;
    private int destinationTileYCoordinate;
    public void Select(TileController selectedTile)
    {
        //Beware: Magic Number
        int currentSelectionLimiter = 1;
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (selectedTile != null && currentSelectionLimiter == 1 && selectedTile != GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile
            && selectedTile.detectedUnit == null)
        {
            var destinationTile = selectedTile;
            //Check if the distance is available

            //If yes, check distance and create the path over the grid
            activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementSelectedSFX.Invoke();
            destinationTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.blue;

            destinationTileXCoordinate = destinationTile.tileXCoordinate;
            destinationTileYCoordinate = destinationTile.tileYCoordinate;
            savedSelectedTile = selectedTile;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;

            selectionLimiter--;

            Debug.Log("Move Destination Selection Logic Execution");
        }
        else
        {
            Debug.Log("Can't Select Destination");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;

        //If a saved destination doesn't esist, by clicking on that tile, this will get back to selection mode
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
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

    // Use this method to check if the Enemy is surrounded. Currently not used.
    public bool IsSurrounded(Unit unit)
    {
        List<TileController> neighbours = GridManager.Instance.gridMovementController.GetNeighbours(unit.ownedTile);
        foreach (TileController neighbour in neighbours)
        {
            if (neighbour.currentSingleTileCondition == SingleTileCondition.free)
            {
                return false; // There's at least one free tile, so the unit is not surrounded
            }
        }
        return true; // All neighbouring tiles are occupied, unit is surrounded
    }
    public void Execute()
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        //Retrieve Active Current Player

        if (activePlayerUnit.unitOpportunityPoints > 0 && activePlayerUnit.GetComponent<UnitStatusController>().unitCurrentStatus != UnitStatus.stun)
        {
            //if (IsSurrounded(activePlayerUnit))
            //{
            //    Debug.Log("Unit is surrounded and cannot move.");
            //    // Here to add any additional logic for when the unit is surrounded
            //}

            if (activePlayerUnit.MoveUnit(destinationTileXCoordinate, destinationTileYCoordinate))
            {
                //Use Grid Logic to Move the Player to Destination
                activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementConfirmedSFX.Invoke();

                activePlayerUnit.GetComponentInChildren<Animator>().SetTrigger(FindAnimationTrigger(activePlayerUnit, savedSelectedTile));
                activePlayerUnit.ownedTile.detectedUnit = null;
                activePlayerUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
                activePlayerUnit.ownedTile = savedSelectedTile;
                activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;

                activePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitProfile(activePlayerUnit);


                Debug.Log("Moving Character Execution Logic");
            }
            else
            {
                Debug.Log("Destination tile is not valid.");
            }
        }
        else
        {
            UpdateActivePlayerUnitProfile(activePlayerUnit);
            Debug.Log("Not enough Opportunity Points or Unit is stunned.");
        }
    }

    public void RevertToSelectionUnitPlayerAction()
    {
        //Loops through all the grids on the Grid Map, reactivates the select Player Unit Action Mode, deactivates the spell menu, reset the current Active Player Unit

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