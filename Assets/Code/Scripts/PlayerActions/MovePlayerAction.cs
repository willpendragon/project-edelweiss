using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public delegate void UnitMovedToTile(TileController tileController);
    public static event UnitMovedToTile OnUnitMovedToTile;

    public void Execute(TileController targetTile)
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.CheckTileAvailability(targetTile.tileXCoordinate, targetTile.tileYCoordinate) == false)
            return;
        if (activePlayerUnit.unitOpportunityPoints == 0)
            return;
        if (UnitHasNegativeStatus(activePlayerUnit) == true)
            return;
        activePlayerUnit.MoveUnit(targetTile.tileXCoordinate, targetTile.tileYCoordinate, false);

        GridManager.Instance.tileSelectionPermitted = true;
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMovementConfirmedSFX.Invoke();
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(activePlayerUnit.currentXCoordinate, activePlayerUnit.currentYCoordinate, targetTile.tileXCoordinate, targetTile.tileYCoordinate);
        GridManager.Instance.ClearPath();

        FreeTile(activePlayerUnit);
        ClaimTile(activePlayerUnit, targetTile);
        HandleCameraSorting();
        SpendOpportunityPoints(activePlayerUnit);
        UpdateActivePlayerUnitProfile(activePlayerUnit);
        activePlayerUnit.ownedTile.CheckFieldPrizes(activePlayerUnit.ownedTile, activePlayerUnit);
        UpdatePathVisual(path);
        //savedSelectedTile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
        //activePlayerUnit.GetComponentInChildren<Animator>().SetTrigger(FindAnimationTrigger(activePlayerUnit, savedSelectedTile));
        //GameObject.FindGameObjectWithTag("ReachableTilesVisualizer").GetComponent<ReachableTilesVisualizer>().ShowReachableTiles();
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
    }
    private void ClaimTile(Unit activePlayerUnit, TileController tile)
    {
        activePlayerUnit.ownedTile = tile;
        activePlayerUnit.ownedTile.detectedUnit = activePlayerUnit.gameObject;
    }
    private void HandleCameraSorting()
    {
        GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
    }
    private void SpendOpportunityPoints(Unit activePlayerUnit)
    {
        activePlayerUnit.unitOpportunityPoints--;
    }
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }
    private void UpdatePathVisual(List<TileController> path)
    {
        LineRenderer lineRenderer = GridManager.Instance.GetLineRenderer();
        if (lineRenderer != null)
        {
            Vector3[] pathPoints = path.Select(tile => GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate) + new Vector3(0, 0.7f, 0)).ToArray(); // May be necessary to adjust Y to avoid z-fighting.
            lineRenderer.positionCount = pathPoints.Length;
            lineRenderer.SetPositions(pathPoints);
            lineRenderer.startWidth = 0.25f;
            lineRenderer.endWidth = 0.25f;
        }
        else
        {
            Debug.LogError("LineRenderer not initialized.");
        }
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