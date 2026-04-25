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
            
        // 1. Filter out nulls, occupied tiles, ALREADY frozen tiles, and explicitly tagged Decorations
        var allTiles = GridManager.Instance.gridTileControllers;
        var validTiles = allTiles.Where(t => 
            t != null && 
            t.tileType == TileType.Basic && // Strictly only normal floor tiles
            t.currentSingleTileCondition == SingleTileCondition.free && 
            t.detectedUnit == null &&
            t.tileElement != TileElement.Ice && // CRITICAL: Do not enchant tiles that are already iced!
            !IsDecoration(t) // Ignore explicit DecorationEnvironment tags
        ).ToList();

        if (validTiles.Count == 0)
        {
            Debug.LogWarning("Similde couldn't find any valid free/unfrozen tiles to enchant.");
            return;
        }

        // 2. Pick a random valid center tile
        int randomTileIndex = localRandom.Next(validTiles.Count);
        TileController randomTile = validTiles[randomTileIndex];
        
        // 3. Get neighbors and safely handle nulls/grid holes
        List<TileController> rawNeighbors = GridManager.Instance.gridMovementController.GetNeighbours(randomTile);
        List<TileController> finalList = new List<TileController>();
        
        if (rawNeighbors != null)
        {
            finalList.AddRange(rawNeighbors);
        }
        finalList.Add(randomTile);

        // 4. Sanitize the blast radius using the exact same aggressive filtering
        var sanitizedList = finalList.Where(t => 
            t != null && 
            t.tileType == TileType.Basic && 
            t.currentSingleTileCondition == SingleTileCondition.free && 
            t.detectedUnit == null &&
            t.tileElement != TileElement.Ice &&
            !IsDecoration(t)
        ).ToList();

        // 5. Apply the VFX
        GameObject effectPrefab = Resources.Load<GameObject>("SimildePossessedTile");
        int enchantedCount = 0;

        foreach (var tile in sanitizedList)
        {
            if (effectPrefab != null)
            {
                GameObject effectInstance = Instantiate(effectPrefab, tile.transform);
                
                effectInstance.transform.localPosition = new Vector3(0f, 0.52f, 0f); 
                effectInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
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
