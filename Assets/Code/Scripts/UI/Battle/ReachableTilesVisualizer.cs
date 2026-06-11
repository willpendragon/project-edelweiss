using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;

public class ReachableTilesVisualizer : MonoBehaviour
{
    public List<TileController> reachableTiles = new List<TileController>(); // Track reachable tiles
    private Unit _activePlayerUnit;

    // Function to find and highlight reachable tiles
    public void ShowReachableTiles()
    {
        // Get the active player unit
        _activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();

        if (_activePlayerUnit == null)
        {
            Debug.LogError("No active player unit found!");
            return;
        }

        // Clear any previous tile highlights
        ClearReachableTiles();

        // Get all reachable tiles within movement range
        reachableTiles = GetReachableTiles(_activePlayerUnit);

        // Highlight each reachable tile
        foreach (TileController tile in reachableTiles)
        {
            Debug.Log($"Highlighting tile at: {tile.tileXCoordinate}, {tile.tileYCoordinate}");
            // Change the color of the tile directly for testing
            tile.tileShaderController.SetTileToMoveRangeColor();
            tile.tileShaderController.SetTileGlowIntensity(1f);
        }
        Debug.Log($"Reachable tiles highlighted: {reachableTiles.Count}");

        // Sort units to prevent visual overlap issues
        GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
    }

    // Use BFS to find all reachable tiles within the player's movement range
    private List<TileController> GetReachableTiles(Unit unit)
    {
        List<TileController> reachableTiles = new List<TileController>();
        Queue<TileController> tilesToExplore = new Queue<TileController>();

        TileController startTile = unit.ownedTile;
        tilesToExplore.Enqueue(startTile);

        Dictionary<TileController, int> visitedTiles = new Dictionary<TileController, int>();
        visitedTiles[startTile] = 0;

        while (tilesToExplore.Count > 0)
        {
            TileController currentTile = tilesToExplore.Dequeue();
            int currentDistance = visitedTiles[currentTile];


            if (currentDistance <= unit.unitMovementLimit)
            {

                if (currentTile.currentSingleTileCondition == SingleTileCondition.free || currentTile == startTile)
                {
                    if (!reachableTiles.Contains(currentTile))
                        reachableTiles.Add(currentTile);
                }

                if (currentDistance < unit.unitMovementLimit)
                {
                    List<TileController> neighbors = GridManager.Instance.gridMovementController.GetNeighbours(currentTile);

                    foreach (TileController neighbor in neighbors)
                    {

                        if (!visitedTiles.ContainsKey(neighbor) &&
                            neighbor.currentSingleTileCondition == SingleTileCondition.free &&
                            neighbor.detectedUnit == null)
                        {
                            tilesToExplore.Enqueue(neighbor);
                            visitedTiles[neighbor] = currentDistance + 1;
                        }
                    }
                }
            }
        }

        return reachableTiles;
    }

    private List<TileController> GetTargetableTiles(Unit unit, int range)
    {
        List<TileController> targetableTiles = new List<TileController>();
        Queue<TileController> tilesToExplore = new Queue<TileController>();
        Dictionary<TileController, int> visitedTiles = new Dictionary<TileController, int>();

        TileController startTile = unit.ownedTile;
        tilesToExplore.Enqueue(startTile);
        visitedTiles[startTile] = 0;

        while (tilesToExplore.Count > 0)
        {
            TileController currentTile = tilesToExplore.Dequeue();
            int currentDistance = visitedTiles[currentTile];

            if (currentDistance > 0 && currentDistance <= range && currentTile.detectedUnit != null && currentTile.detectedUnit.CompareTag("Enemy"))
            {
                targetableTiles.Add(currentTile);
            }

            if (currentDistance < range)
            {
                List<TileController> neighbors = GridManager.Instance.gridMovementController.GetNeighbours(currentTile);

                foreach (TileController neighbor in neighbors)
                {
                    if (!visitedTiles.ContainsKey(neighbor))
                    {
                        tilesToExplore.Enqueue(neighbor);
                        visitedTiles[neighbor] = currentDistance + 1;
                    }
                }
            }
        }

        return targetableTiles;
    }

    public void ShowTargetableTiles(Unit unit, int range, Color highlightColor)
    {
        ClearReachableTiles();

        reachableTiles = GetTargetableTiles(unit, range);

        foreach (TileController tile in reachableTiles)
        {
            tile.tileShaderController.SetTileGlowIntensity(1f);
        }

        Debug.Log($"Targetable tiles highlighted: {reachableTiles.Count}");
    }


    // Clear the visual effect from previously highlighted tiles
    public void ClearReachableTiles()
    {
        GetReachableTiles(_activePlayerUnit);
        foreach (TileController tile in reachableTiles)
        {
            // Reset the tile color
            tile.tileShaderController.SetTileGlowIntensity(0f);
        }
        reachableTiles.Clear(); // Clear the list
        Debug.Log("Cleared previous tile highlights.");
    }
}

