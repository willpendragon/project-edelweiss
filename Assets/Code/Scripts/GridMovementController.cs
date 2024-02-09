using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridMovementController : MonoBehaviour
{
    public GridManager gridManager;

    public List<TileController> FindPath(int startX, int startY, int targetX, int targetY)
    {
        TileController startTile = GridManager.Instance.GetTileControllerInstance(startX, startY);
        TileController targetTile = GridManager.Instance.GetTileControllerInstance(targetX, targetY);

        if (startTile == null || targetTile == null)
        {
            Debug.LogError("Start or Target tile is null.");
            return null;
        }

        if (Mathf.Abs(startTile.tileXCoordinate - targetTile.tileXCoordinate) == 1 && Mathf.Abs(startTile.tileYCoordinate - targetTile.tileYCoordinate) == 1)
        {
            return null; // Diagonal move not allowed
        }

        List<TileController> openSet = new List<TileController>();
        HashSet<TileController> closedSet = new HashSet<TileController>();
        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            TileController currentTile = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentTile.FCost || openSet[i].FCost == currentTile.FCost && openSet[i].hCost < currentTile.hCost)
                {
                    currentTile = openSet[i];
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile);
            }

            foreach (TileController neighbour in GetNeighbours(currentTile))
            {
                if (neighbour.currentSingleTileCondition == SingleTileCondition.occupied || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    neighbour.parent = currentTile;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    private List<TileController> RetracePath(TileController startTile, TileController endTile)
    {
        List<TileController> path = new List<TileController>();
        TileController currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();

        return path;
    }

    public List<TileController> GetNeighbours(TileController tile)
    {
        List<TileController> neighbours = new List<TileController>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the current tile and any diagonal tiles
                if (x == 0 && y == 0 || x != 0 && y != 0)
                    continue;
                int checkX = tile.tileXCoordinate + x;
                int checkY = tile.tileYCoordinate + y;

                // Check if the neighbor is within the grid bounds
                if (checkX >= 0 && checkX < gridManager.gridHorizontalSize && checkY >= 0 && checkY < gridManager.gridVerticalSize)
                {
                    neighbours.Add(gridManager.GetTileControllerInstance(checkX, checkY));
                }
            }
        }

        return neighbours;
    }

    //Use this method to select the AOE Range. In the future, I will need to change the values dynamically retrieving info about the AOE Spell from the AOE Spell Scriptable Object.
    //Use the same system also for the Deities spawning.
    public List<TileController> GetMultipleTiles(TileController tile)
    {
        List<TileController> neighbours = new List<TileController>();

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                int checkX = tile.tileXCoordinate + x;
                int checkY = tile.tileYCoordinate + y;

                // Check if the neighbor is within the grid bounds
                if (checkX >= 0 && checkX < gridManager.gridHorizontalSize && checkY >= 0 && checkY < gridManager.gridVerticalSize)
                {
                    neighbours.Add(gridManager.GetTileControllerInstance(checkX, checkY));
                }
            }
        }

        return neighbours;
    }

    int GetDistance(TileController tileA, TileController tileB)
    {
        int distX = Mathf.Abs(tileA.tileXCoordinate - tileB.tileXCoordinate);
        int distY = Mathf.Abs(tileA.tileYCoordinate - tileB.tileYCoordinate);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}
