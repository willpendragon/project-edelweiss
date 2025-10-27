using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Edelweiss.Core;

public class MovePlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public delegate void UnitMovedToTile(TileController tileController);
    public static event UnitMovedToTile OnUnitMovedToTile;

    public delegate void UnitNegativeStatus(string notification);
    public static event UnitNegativeStatus OnUnitNegativeStatus;

    public void Execute(TileController targetTile)
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.CheckTileAvailability(targetTile.tileXCoordinate, targetTile.tileYCoordinate) == false)
            return;
        if (activePlayerUnit.unitOpportunityPoints <= 0)
            return;
        if (UnitHasNegativeStatus(activePlayerUnit) == true)
        {
            OnUnitNegativeStatus($"{activePlayerUnit.unitTemplate.unitName} is unable to move");
            return;
        }
        activePlayerUnit.MoveUnit(targetTile.tileXCoordinate, targetTile.tileYCoordinate, false);

        GridManager.Instance.tileSelectionPermitted = true;
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementConfirmedSFX.Invoke();
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(activePlayerUnit.currentXCoordinate, activePlayerUnit.currentYCoordinate, targetTile.tileXCoordinate, targetTile.tileYCoordinate);
        GridManager.Instance.ClearPath();

        FreeTile(activePlayerUnit);
        ClaimTile(activePlayerUnit, targetTile);
        SpendOpportunityPoints(activePlayerUnit);
        UpdateActivePlayerUnitProfile(activePlayerUnit);
        activePlayerUnit.ownedTile.CheckFieldPrizes(activePlayerUnit.ownedTile, activePlayerUnit);
        var reachableTilesVisualizer = FindAnyObjectByType<ReachableTilesVisualizer>();
        reachableTilesVisualizer.ClearReachableTiles();

        OnUnitMovedToTile(targetTile);
    }
    public void Deselect()
    {
    }
    private bool UnitHasNegativeStatus(Unit activePlayerUnit)
    {
        if (activePlayerUnit.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.basic)
            return false;
        else
            return true;
    }
    private void FreeTile(Unit activePlayerUnit)
    {
        activePlayerUnit.ownedTile.detectedUnit = null;
        activePlayerUnit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
        // Reset tile color
        activePlayerUnit.ownedTile.tileShaderController.SetTileToMoveRangeColor();
        activePlayerUnit.ownedTile.tileShaderController.SetTileGlowIntensity(0f);
        // Destroy the Active Player Unit tile indicator
        var unitSelection = FindAnyObjectByType<UnitSelectionController>();
        Destroy(unitSelection.selectedTileInstance);
    }
    private void ClaimTile(Unit activePlayerUnit, TileController tile)
    {
        activePlayerUnit.ownedTile = tile;
        activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;
    }
    private void SpendOpportunityPoints(Unit activePlayerUnit)
    {
        activePlayerUnit.unitOpportunityPoints--;
    }
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }

    public string FindAnimationTrigger(Unit activePlayerUnit, TileController destinationTile)
    {
        if (activePlayerUnit.ownedTile.transform.localPosition.x > destinationTile.transform.localPosition.x)
        {

            return "leftAnimationTrigger";
        }
        else
        {
            return null;
        }
    }
}