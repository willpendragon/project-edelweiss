using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SimildeBehavior", menuName = "DeityBehavior/Similde")]
public class DeitySimildeBehavior : DeityBehavior
{
    private string deityName = "Similde";
    private System.Random localRandom;

    public override void ExecuteBehavior(Deity deity)
    {
        Debug.Log("Deity is acting");
        
        if (localRandom == null)
            localRandom = new System.Random();
            
        // Helper: Quick way to verify if a tile is corrupted by a decoration object
        bool IsDecoration(TileController t)
        {
            // Check if the tile's own GameObject holds the tag
            if (t.gameObject.CompareTag("DecorationEnvironment")) return true;
            
            // Check if there is an object sitting on the tile that holds the tag
            if (t.detectedUnit != null && t.detectedUnit.CompareTag("DecorationEnvironment")) return true;

            return false;
        }
            
        var allTiles = GridManager.Instance.gridTileControllers;

        // Check if all tiles are already iced
        bool allIced = allTiles.All(t => t.tileElement == TileElement.Ice);

        if (allIced)
        {
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName}'s ice covers the entire field.");
            return;
        }

        // 1. Filter out nulls, occupied tiles, ALREADY frozen tiles, and explicitly tagged Decorations
        var validTiles = allTiles.Where(t => 
            t != null && 
            t.tileType == TileType.Basic && // Strictly only normal floor tiles
            t.currentSingleTileCondition == SingleTileCondition.free && 
            t.detectedUnit == null &&
            t.tileElement != TileElement.Ice && // CRITICAL: Do not enchant tiles that are already iced!
            !IsDecoration(t) // Ignore explicit DecorationEnvironment tags
        ).ToList();

        // 2. Define the amount of tiles to enchant (using Laurinus' range of 20-30)
        int minEnchantedRange = 5;
        int maxEnchantedRange = 10;
        int enchantCountGoal = UnityEngine.Random.Range(minEnchantedRange, maxEnchantedRange);

        if (validTiles.Count < enchantCountGoal)
        {
            enchantCountGoal = validTiles.Count; // Adjust to max available
        }

        if (enchantCountGoal == 0)
        {
            Debug.LogWarning("Similde couldn't find any valid free/unfrozen tiles to enchant.");
            return;
        }

        // 3. Randomize and extract the selection (same logic as Laurinus)
        var selectedTiles = validTiles
            .OrderBy(t => System.Guid.NewGuid()) // Randomize
            .Take(enchantCountGoal)
            .ToArray();

        // 4. Apply the VFX
        GameObject effectPrefab = Resources.Load<GameObject>("SimildePossessedTile");
        int enchantedCount = 0;

        foreach (var tile in selectedTiles)
        {
            if (effectPrefab != null)
            {
                GameObject effectInstance = Instantiate(effectPrefab, tile.transform);
            
                // Keep it anchored to the tile center with a 0.52 Y offset
                effectInstance.transform.localPosition = new Vector3(0f, 0.52f, 0f); 
                
                // Rotate 90 degrees on the X-axis to lie flat on the 3D ground
                effectInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                
                // Scale uniform to 2.5 fitting the new 3D tile size
                effectInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            Debug.Log($"Similde enchanted {tile.gameObject.name} at {tile.tileXCoordinate}, {tile.tileYCoordinate}");
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
