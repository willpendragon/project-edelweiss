using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MovePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    private int destinationTileXCoordinate;
    private int destinationTileYCoordinate;

    public delegate void UnitMovedToTile(TileController tileController);
    public static event UnitMovedToTile OnUnitMovedToTile;
    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (selectedTile != null && GridManager.Instance.tileSelectionPermitted == true && selectedTile != GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().ownedTile
            && selectedTile.detectedUnit == null)
        {
            var destinationTile = selectedTile;

            if (activePlayerUnit.CheckTileAvailability(selectedTile.tileXCoordinate, selectedTile.tileYCoordinate))
            {
                GridManager.Instance.tileSelectionPermitted = false;
                selectedTile.tileShaderController.AnimateFadeHeight(2.75f, 0.2f, Color.green);
                Debug.Log("Tile is within Character Movement Limit");
            }
            else if (!activePlayerUnit.CheckTileAvailability(selectedTile.tileXCoordinate, selectedTile.tileYCoordinate))
            {
                GridManager.Instance.tileSelectionPermitted = false;
                selectedTile.tileShaderController.AnimateFadeHeight(2.75f, 0.2f, Color.red);
                Debug.Log("Tile is not within Character Movement Limit");
            }

            activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementSelectedSFX.Invoke();

            destinationTileXCoordinate = destinationTile.tileXCoordinate;
            destinationTileYCoordinate = destinationTile.tileYCoordinate;

            List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(activePlayerUnit.currentXCoordinate, activePlayerUnit.currentYCoordinate, destinationTileXCoordinate, destinationTileYCoordinate);

            // Update the LineRenderer with the new path.
            UpdatePathVisual(path);
            savedSelectedTile = selectedTile;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
        }
        else
        {
            Debug.Log("Can't Select Destination");
        }
    }

    private void UpdatePathVisual(List<TileController> path)
    {
        LineRenderer lineRenderer = GridManager.Instance.GetLineRenderer();
        if (lineRenderer != null)
        {
            Vector3[] pathPoints = path.Select(tile => GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate) + new Vector3(0, 0.7f, 0)).ToArray(); // Adjust Y if necessary to prevent z-fighting
            lineRenderer.positionCount = pathPoints.Length;
            lineRenderer.SetPositions(pathPoints);
            lineRenderer.startWidth = 0.25f;
            lineRenderer.endWidth = 0.25f;
        }
        else
        {
            Debug.LogError("LineRenderer is not initialized.");
        }
    }

    public void Deselect()
    {
        // If a saved destination esists, by clicking on that tile, this will get back to selection mode.
        if (savedSelectedTile != null)
        {
            savedSelectedTile.tileShaderController.AnimateFadeHeight(1f, 0.2f, Color.cyan);
            // Restores the Tile to the usual Move Selection Phase visuals.
            Debug.Log("Deselecting Destination");
            //savedSelectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
            GridManager.Instance.tileSelectionPermitted = true;
            GridManager.Instance.ClearPath();
            BattleAudioController battleAudioController = GameObject.FindGameObjectWithTag("BattleAudioController").GetComponent<BattleAudioController>();

            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
            if (battleAudioController != null)
            {
                battleAudioController.PlayCancelMoveSelectionSound();
            }
        }

        // If a saved destination doesn't exist, by clicking on that tile, it will call the RevertToSelectionUnitPlayerAction method.
        else if (GridManager.Instance.currentPlayerUnit != null & savedSelectedTile == null)
        {
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            }
            Debug.Log("Resetting Current Active Player Selection");
            RevertToSelectionUnitPlayerAction();
        }
        else if (savedSelectedTile == null)
        {
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            }
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
                return false; // There's at least one free tile, so the Unit is not surrounded.
            }
        }
        return true; // All neighbouring tiles are occupied, Unit is surrounded.
    }
    public void Execute()
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && activePlayerUnit.GetComponent<UnitStatusController>().unitCurrentStatus != UnitStatus.stun)
        {
            // Move the current Active Player Unit to the Destination Tile. 
            if (activePlayerUnit.MoveUnit(destinationTileXCoordinate, destinationTileYCoordinate, false))
            {
                GridManager.Instance.tileSelectionPermitted = true;

                activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementConfirmedSFX.Invoke();
                savedSelectedTile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
                GridManager.Instance.ClearPath();

                activePlayerUnit.GetComponentInChildren<Animator>().SetTrigger(FindAnimationTrigger(activePlayerUnit, savedSelectedTile));
                activePlayerUnit.ownedTile.detectedUnit = null;
                activePlayerUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
                activePlayerUnit.ownedTile = savedSelectedTile;
                activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;

                activePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitProfile(activePlayerUnit);

                activePlayerUnit.ownedTile.CheckFieldPrizes(activePlayerUnit.ownedTile, activePlayerUnit);
                GameObject.FindGameObjectWithTag("ReachableTilesVisualizer").GetComponent<ReachableTilesVisualizer>().ShowReachableTiles();
                TileController destinationTile = activePlayerUnit.ownedTile;
                OnUnitMovedToTile(destinationTile);
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
        // Loops through all the grids on the Grid Map, reactivates the select Player Unit Action Mode, deactivates the spell menu, reset the current Active Player Unit.

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            tile.currentPlayerAction = new SelectUnitPlayerAction();
            tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
        }
        GameObject[] playerUISpellButtons = GameObject.FindGameObjectsWithTag("PlayerUISpellButton");
        foreach (var playerUISpellButton in playerUISpellButtons)
        {
            Destroy(playerUISpellButton);
        }
        Destroy(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel);
        GridManager.Instance.currentPlayerUnit.tag = "Player";
        GridManager.Instance.currentPlayerUnit = null;

        GameObject movesContainer = GameObject.FindGameObjectWithTag("MovesContainer");
        movesContainer.transform.localScale = new Vector3(0, 0, 0);
        Destroy(GameObject.FindGameObjectWithTag("ActivePlayerCharacterSelectionIcon"));
        GridManager.Instance.ClearPath();
        BattleAudioController battleAudioController = GameObject.FindGameObjectWithTag("BattleAudioController").GetComponent<BattleAudioController>();
        if (battleAudioController != null)
        {
            battleAudioController.PlayCancelMoveSelectionSound();
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
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }
}