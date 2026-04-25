using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimildeBehavior", menuName = "DeityBehavior/Similde")]
public class DeitySimildeBehavior : DeityBehavior
{
    private string deityName = "Similde";

    public override void ExecuteBehavior(Deity deity)
    {
        Debug.Log("Deity is acting");
        // Scan the Grid
        var tileList = GridManager.Instance.gridTileControllers;
        // Roll two random numbers
        int randomTileIndex = Random.Range(1, tileList.Length);
        TileController randomTile = tileList[randomTileIndex];
        List<TileController> randomTiles = GridManager.Instance.gridMovementController.GetNeighbours(randomTile);
        List<TileController> finalList = new List<TileController>(randomTiles);
        finalList.Add(randomTile);

        // Load prefab once before the loop
        GameObject effectPrefab = Resources.Load<GameObject>("SimildePossessedTile");
        int enchantedCount = 0; // Track how many valid tiles actually got enchanted

        foreach (var tile in finalList)
        {
            // Skip decorations, obstacles, chests, and solid things.
            // Only enchant Basic tiles (or add others if you consider them valid targets).
            if (tile.tileType == TileType.Environment || tile.tileType == TileType.Obstacle)
            {
                Debug.Log($"Skipped {tile.gameObject.name} because it is type {tile.tileType}");
                continue;
            }

            if (effectPrefab != null)
            {
                GameObject effectInstance = Instantiate(effectPrefab, tile.transform);
                
                // Keep the Y-offset of 0.52 to ensure it sits safely above the floor
                effectInstance.transform.localPosition = new Vector3(0f, 0.52f, 0f); 
                
                // Rotate exactly 90 degrees on the X-axis
                effectInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                
                // Scale uniform to exactly 0.5
                effectInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            Debug.Log($"Randomly picked {tile.gameObject.name} at {tile.tileXCoordinate}, {tile.tileYCoordinate}");
            tile.tileElement = TileElement.Ice;
            enchantedCount++;
        }
        
        if (enchantedCount > 0)
        {
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName} enchanted {enchantedCount} tiles");
        }
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {

    }
}
