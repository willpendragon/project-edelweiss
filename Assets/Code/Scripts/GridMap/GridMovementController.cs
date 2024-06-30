using System.Collections.Generic;
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

        List<TileController> openSet = new List<TileController> { startTile };
        HashSet<TileController> closedSet = new HashSet<TileController>();

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
                if (neighbour == null || neighbour.currentSingleTileCondition == SingleTileCondition.occupied || closedSet.Contains(neighbour))
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
                    TileController neighbour = gridManager.GetTileControllerInstance(checkX, checkY);
                    if (neighbour != null)
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }
        }

        return neighbours;
    }

    public List<TileController> GetMultipleTiles(TileController tile, int numberOfTiles)
    {
        List<TileController> tilesInRange = new List<TileController>();

        int startX = Mathf.Max(0, tile.tileXCoordinate - numberOfTiles);
        int endX = Mathf.Min(gridManager.gridHorizontalSize - 1, tile.tileXCoordinate + numberOfTiles);
        int startY = Mathf.Max(0, tile.tileYCoordinate - numberOfTiles);
        int endY = Mathf.Min(gridManager.gridVerticalSize - 1, tile.tileYCoordinate + numberOfTiles);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                TileController neighbour = gridManager.GetTileControllerInstance(x, y);
                if (neighbour != null)
                {
                    tilesInRange.Add(neighbour);
                }
            }
        }

        return tilesInRange;
    }

    public int GetDistance(TileController tileA, TileController tileB)
    {
        int distX = Mathf.Abs(tileA.tileXCoordinate - tileB.tileXCoordinate);
        int distY = Mathf.Abs(tileA.tileYCoordinate - tileB.tileYCoordinate);

        return 10 * (distX + distY);
    }
}
