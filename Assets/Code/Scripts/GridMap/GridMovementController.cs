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
            Debug.LogError($"Start or Target tile is null. Start: ({startX}, {startY}), Target: ({targetX}, {targetY})");
            return null;
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

            // Check for L-shaped moves
            if (Mathf.Abs(currentTile.tileXCoordinate - targetTile.tileXCoordinate) == 1 && Mathf.Abs(currentTile.tileYCoordinate - targetTile.tileYCoordinate) == 1)
            {
                // Find the intermediate tile
                int midX1 = currentTile.tileXCoordinate;
                int midY1 = targetTile.tileYCoordinate;
                int midX2 = targetTile.tileXCoordinate;
                int midY2 = currentTile.tileYCoordinate;

                TileController midTile1 = GridManager.Instance.GetTileControllerInstance(midX1, midY1);
                TileController midTile2 = GridManager.Instance.GetTileControllerInstance(midX2, midY2);

                if (midTile1 != null && midTile1.currentSingleTileCondition != SingleTileCondition.occupied && !closedSet.Contains(midTile1))
                {
                    int newMovementCostToMidTile1 = currentTile.gCost + GetDistance(currentTile, midTile1);
                    if (newMovementCostToMidTile1 < midTile1.gCost || !openSet.Contains(midTile1))
                    {
                        midTile1.gCost = newMovementCostToMidTile1;
                        midTile1.hCost = GetDistance(midTile1, targetTile);
                        midTile1.parent = currentTile;

                        if (!openSet.Contains(midTile1))
                            openSet.Add(midTile1);
                    }
                }

                if (midTile2 != null && midTile2.currentSingleTileCondition != SingleTileCondition.occupied && !closedSet.Contains(midTile2))
                {
                    int newMovementCostToMidTile2 = currentTile.gCost + GetDistance(currentTile, midTile2);
                    if (newMovementCostToMidTile2 < midTile2.gCost || !openSet.Contains(midTile2))
                    {
                        midTile2.gCost = newMovementCostToMidTile2;
                        midTile2.hCost = GetDistance(midTile2, targetTile);
                        midTile2.parent = currentTile;

                        if (!openSet.Contains(midTile2))
                            openSet.Add(midTile2);
                    }
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
            if (currentTile == null)
            {
                Debug.LogError("Parent link not set correctly, path retrace failed.");
                return null;
            }

            path.Add(currentTile);
            currentTile = currentTile.parent;
        }

        // Add the start tile to the path
        path.Add(startTile);
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

        return /*10 * */(distX + distY);
    }
}
