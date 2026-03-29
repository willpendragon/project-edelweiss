using System.Collections.Generic;
using UnityEngine;

public class GridMovementController : MonoBehaviour
{
    public GridManager gridManager;

    public int maxJumpHeight = 1; // Dislivello massimo (in numero di blocchi) che un'unità può salire/scendere

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

        // Ora usiamo gridPosition (che è un Vector3Int: x, y=elevazione, z=profondità)
        int x = tile.gridPosition.x;
        int y = tile.gridPosition.y; 
        int z = tile.gridPosition.z;

        // Le 4 direzioni cardinali piane
        Vector2Int[] planarDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Nord (Z+1)
            new Vector2Int(0, -1), // Sud (Z-1)
            new Vector2Int(1, 0),  // Est (X+1)
            new Vector2Int(-1, 0)  // Ovest (X-1)
        };

        foreach (var dir in planarDirections)
        {
            int checkX = x + dir.x;
            int checkZ = z + dir.y;

            // Dobbiamo cercare se esiste un tile adiacente nella colonna [checkX, checkZ]
            // che sia a un'altezza raggiungibile (da y - maxJumpHeight a y + maxJumpHeight)
            for (int h = -maxJumpHeight; h <= maxJumpHeight; h++)
            {
                int checkY = y + h;
                
                // Usiamo il nuovo GetTileControllerInstance a 3 dimensioni 
                // Assumendo che lo hai aggiornato in GridManager nel passaggio precedente
                TileController neighbor = GridManager.Instance.GetTileControllerInstance(checkX, checkY, checkZ);

                if (neighbor != null)
                {
                    // Controllo opzionale VOXEL PURO: Assicuriamoci che non ci sia un ostacolo (muro/blocco) SOPRA il tile vicino
                    // altrimenti il personaggio sbatterebbe la testa.
                    TileController tileAboveNeighbor = GridManager.Instance.GetTileControllerInstance(checkX, checkY + 1, checkZ);
                    
                    if (tileAboveNeighbor == null || tileAboveNeighbor.tileType == TileType.Basic /* adatta se i tuoi tile sono valicabili */) 
                    {
                        neighbours.Add(neighbor);
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

    public int GetDistance(TileController nodeA, TileController nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstZ = Mathf.Abs(nodeA.gridPosition.z - nodeB.gridPosition.z); // Era Y
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y); // Dislivello verticale
        
        // Puoi decidere se il dislivello costa punti movimento extra o è "gratis" 
        // finché rientra nel maxJumpHeight. Di base:
        int flatDistance = (dstX > dstZ) ? 
            14 * dstZ + 10 * (dstX - dstZ) : 
            14 * dstX + 10 * (dstZ - dstX);

        // Aggiungiamo un peso al dislivello se lo desideri
        int elevationPenalty = dstY * 10; 
        
        return flatDistance + elevationPenalty;
    }

    public List<TileController> GetTilesInDirection(int startX, int startY, Beacon.FacingDirection direction, int range)
    {
        List<TileController> tiles = new List<TileController>();

        // Determine the X and Y movement per step based on direction
        int dirX = 0;
        int dirY = 0;

        switch (direction)
        {
            case Beacon.FacingDirection.Up: dirY = 1; break;
            case Beacon.FacingDirection.Down: dirY = -1; break;
            case Beacon.FacingDirection.Left: dirX = -1; break;
            case Beacon.FacingDirection.Right: dirX = 1; break;
        }

        // Loop through the range, starting from 1 tile away from the beacon
        for (int i = 1; i <= range; i++)
        {
            int checkX = startX + (dirX * i);
            int checkY = startY + (dirY * i);

            // Check if the coordinates are within the grid boundaries
            if (checkX >= 0 && checkX < gridManager.gridHorizontalSize &&
                checkY >= 0 && checkY < gridManager.gridVerticalSize)
            {
                TileController tile = gridManager.GetTileControllerInstance(checkX, checkY);
                if (tile != null)
                {
                    tiles.Add(tile);
                }
            }
            else
            {
                // Stop if we hit the edge of the map
                break;
            }
        }

        return tiles;
    }
}
