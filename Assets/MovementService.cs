using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementService : MonoBehaviour
{
    public int unitMovementLimit;
    public int currentXCoordinate;
    public int currentYCoordinate;


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
                transform.position = worldPosition + new Vector3(0, transform.localScale.y / 2, 0);

                // Update current grid coordinates
                currentXCoordinate = tile.tileXCoordinate;
                currentYCoordinate = tile.tileYCoordinate;
                //tile.GetComponent<TileController>().detectedUnit = this.gameObject;
                //030120240216 Removed because otherwise every tile of the movement path gets its DetectedUnit set as the moving Unit. 
                Debug.Log($"Moving to Tile at: ({tile.tileXCoordinate}, {tile.tileYCoordinate})");
            }
        }
        else
        {
            Debug.Log("No valid path found or path exceeds movement limit.");
        }
    }
}
