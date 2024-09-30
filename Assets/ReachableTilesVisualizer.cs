using System.Collections.Generic;
using UnityEngine;

public class ReachableTilesVisualizer : MonoBehaviour
{
    public List<TileController> reachableTiles = new List<TileController>(); // Track reachable tiles
    public Color highlightColor = Color.green; // Color for highlighting tiles
    public Color defaultTileColor = Color.blue; // Default color for tiles

    public const string activePlayerUnitTag = "ActivePlayerUnit";

    private Unit activePlayerUnit;

    void Update()
    {
        // Detect if the E key is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed, showing reachable tiles.");
            ShowReachableTiles();
        }
    }

    // Function to find and highlight reachable tiles
    public void ShowReachableTiles()
    {
        // Get the active player unit
        activePlayerUnit = GameObject.FindGameObjectWithTag(activePlayerUnitTag).GetComponent<Unit>();

        if (activePlayerUnit == null)
        {
            Debug.LogError("No active player unit found!");
            return;
        }

        // Clear any previous tile highlights
        ClearReachableTiles(0, 0.2f, Color.white);

        // Get all reachable tiles within movement range
        reachableTiles = GetReachableTiles(activePlayerUnit);

        // Highlight each reachable tile
        foreach (TileController tile in reachableTiles)
        {
            Debug.Log($"Highlighting tile at: {tile.tileXCoordinate}, {tile.tileYCoordinate}");
            // Change the color of the tile directly for testing
            tile.tileShaderController.AnimateFadeHeight(1f, 0.2f, Color.cyan);
        }

        Debug.Log($"Reachable tiles highlighted: {reachableTiles.Count}");
    }

    // Use BFS to find all reachable tiles within the player's movement range
    private List<TileController> GetReachableTiles(Unit unit)
    {
        List<TileController> reachableTiles = new List<TileController>();
        Queue<TileController> tilesToExplore = new Queue<TileController>();

        TileController startTile = unit.ownedTile;
        tilesToExplore.Enqueue(startTile);

        // Dictionary to track visited tiles and distance from the start tile
        Dictionary<TileController, int> visitedTiles = new Dictionary<TileController, int>();
        visitedTiles[startTile] = 0;

        while (tilesToExplore.Count > 0)
        {
            TileController currentTile = tilesToExplore.Dequeue();
            int currentDistance = visitedTiles[currentTile];

            // If the tile is within movement range, add it to reachable tiles
            if (currentDistance < unit.unitMovementLimit)  // We use < to avoid over-counting the last step
            {
                reachableTiles.Add(currentTile);

                // Get neighboring tiles (non-diagonal)
                List<TileController> neighbors = GridManager.Instance.gridMovementController.GetNeighbours(currentTile);

                foreach (TileController neighbor in neighbors)
                {
                    if (!visitedTiles.ContainsKey(neighbor) && neighbor.detectedUnit == null) // Check for no units on tile
                    {
                        tilesToExplore.Enqueue(neighbor);
                        visitedTiles[neighbor] = currentDistance + 1;
                    }
                }
            }
        }

        return reachableTiles;
    }

    // Clear the visual effect from previously highlighted tiles
    public void ClearReachableTiles(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        foreach (TileController tile in reachableTiles)
        {
            // Reset the tile color for testing
            tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
        }
        reachableTiles.Clear(); // Clear the list
        Debug.Log("Cleared previous tile highlights.");
    }
}

