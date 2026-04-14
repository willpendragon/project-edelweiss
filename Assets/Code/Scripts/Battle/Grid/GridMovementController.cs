using System.Collections.Generic;
using UnityEngine;

public class GridMovementController : MonoBehaviour
{
    public GridManager gridManager;

    public int maxJumpHeight = 1; // Dislivello massimo (in numero di blocchi) che un'unità può salire/scendere

    public int GetDistance(TileController nodeA, TileController nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstZ = Mathf.Abs(nodeA.gridPosition.z - nodeB.gridPosition.z); // Era Y
        
        int flatDistance = (dstX > dstZ) ? 
            14 * dstZ + 10 * (dstX - dstZ) : 
            14 * dstX + 10 * (dstZ - dstX);

        // A* shouldn't penalize hill climbing more than taking a long flat path! 
        // We reduce the elevation penalty to a tiny tie-breaker so it just favors flat routes when EQUAL
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y); 
        int elevationPenalty = dstY * 1; 
        
        return flatDistance + elevationPenalty;
    }
    
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
                    continue; // Skip occupied tiles cleanly!
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
        
        int x = tile.gridPosition.x;
        int y = tile.gridPosition.y; 
        int z = tile.gridPosition.z;

        Vector2Int[] planarDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),  new Vector2Int(0, -1), 
            new Vector2Int(1, 0),  new Vector2Int(-1, 0)
        };

        foreach (var dir in planarDirections)
        {
            int checkX = x + dir.x;
            int checkZ = z + dir.y;

            // Esplora dal basso verso l'alto per l'altezza raggiungibile
            for (int h = -maxJumpHeight; h <= maxJumpHeight; h++)
            {
                int checkY = y + h;
                
                TileController neighbor = GridManager.Instance.GetTileControllerInstance(checkX, checkY, checkZ);

                if (neighbor != null)
                {
                    // IL PUNTO CRITALE VOXEL:
                    // Per poter camminare "SOPRA" questo neighbor, lo spazio a Y+1 deve essere LIBERO!
                    TileController tileAboveNeighbor = GridManager.Instance.GetTileControllerInstance(checkX, checkY + 1, checkZ);
                    
                    // Se c'è spazio vuoto (!tileAboveNeighbor) oppure è un tile invisibile pass-through (se lo codifichi)
                    if (tileAboveNeighbor == null) 
                    {
                        neighbours.Add(neighbor);
                        break; // Trovata la vetta calpestabile per questa colonna (X, Z), fermiamo l'analisi verticale
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

    public List<TileController> GetTilesInDirection(int startX, int startZ, Beacon.FacingDirection direction, int range)
    {
        List<TileController> tiles = new List<TileController>();

        // Determine the X and Z movement per step based on direction
        int dirX = 0;
        int dirZ = 0; // Using Z for depth in the voxel map

        switch (direction)
        {
            case Beacon.FacingDirection.Up: dirZ = 1; break;
            case Beacon.FacingDirection.Down: dirZ = -1; break;
            case Beacon.FacingDirection.Left: dirX = -1; break;
            case Beacon.FacingDirection.Right: dirX = 1; break;
        }

        TileController beaconTile = GridManager.Instance.GetTileControllerInstance(startX, startZ);

        // Loop through the range, starting from 1 tile away from the beacon
        for (int i = 1; i <= range; i++)
        {
            int checkX = startX + (dirX * i);
            int checkZ = startZ + (dirZ * i);

            // Fetch the highest tile at that X/Z column
            TileController tile = GridManager.Instance.GetTileControllerInstance(checkX, checkZ);
            
            if (tile != null)
            {
                tiles.Add(tile);

                // Stop the beam if it hits a solid Wall/Environment, an Obstacle, or a cliff face taller than the Beacon!
                if (tile.tileType == TileType.Obstacle || 
                    tile.tileType == TileType.Environment || 
                    (beaconTile != null && tile.gridPosition.y > beaconTile.gridPosition.y))
                {
                    break;
                }
            }
            else
            {
                // Stop if we hit the edge of the map (no tiles found)
                break;
            }
        }

        return tiles;
    }
}
