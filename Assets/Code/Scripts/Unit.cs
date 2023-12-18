using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int unitMovementLimit;
    //public GridMovementController gridMovementController;
    public int currentXCoordinate;
    public int currentYCoordinate;
    //public GridManager gridManager;
    public int startingXCoordinate;
    public int startingYCoordinate;

    public void OnEnable()
    {
        GridManager.OnSetUnitInitialPositionOnGrid += SetUnitInitialPositionOnGrid;
    }
    public void OnDisable()
    {
        GridManager.OnSetUnitInitialPositionOnGrid -= SetUnitInitialPositionOnGrid;
    }

    public void SetUnitInitialPositionOnGrid()
    {
        MoveUnit(startingXCoordinate, startingYCoordinate);
        Debug.Log("Moving at Start Position");
    }

    public void MoveUnit(int targetX, int targetY)
    {
        // Convert current world position to grid coordinates
        Vector2Int startGridPos = GridManager.Instance.GetGridCoordinatesFromWorldPosition(transform.position);

        // Find path using grid coordinates
        List<TileController> path = GridManager.Instance.GetComponentInChildren<GridMovementController>().FindPath(startGridPos.x, startGridPos.y, targetX, targetY);

        if (path != null && path.Count > 0 && path.Count <= unitMovementLimit)
        {
            foreach (var tile in path)
            {
                // Apply visual feedback for the path
                tile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;

                // Convert grid coordinates back to world position for actual movement
                Vector3 worldPosition = GridManager.Instance.GetWorldPositionFromGridCoordinates(tile.tileXCoordinate, tile.tileYCoordinate);
                transform.position = worldPosition;

                // Update current grid coordinates
                currentXCoordinate = tile.tileXCoordinate;
                currentYCoordinate = tile.tileYCoordinate;

                Debug.Log($"Moving to Tile at: ({tile.tileXCoordinate}, {tile.tileYCoordinate})");
            }
        }
        else
        {
            Debug.Log("No valid path found or path exceeds movement limit.");
        }
    }
}
