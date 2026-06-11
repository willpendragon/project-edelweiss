using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPartyManager : MonoBehaviour
{
    public List<EnemyType> currentEnemySelectionIds = new List<EnemyType>();
    public List<Vector2> currentEnemySelectionCoords = new List<Vector2>();

    private System.Random random;

    private void Start()
    {
        random = new System.Random(); 
    }

    public void GenerateEnemyPartyData(EnemyPartyData enemyParty)
    {
        MapData mapData = GameManager.Instance.CurrentMap;

        if (mapData == null || mapData.tilePositions == null)
            return;

        int enemyPoolSize = RandomRange(enemyParty.minEnemyPoolSize, enemyParty.maxEnemyPoolSize + 1);
        List<EnemyType> generatedEnemies = GenerateEnemyPool(enemyParty.enemyWeights, enemyPoolSize);

        // 1. Gather all mathematically painted basic floor tiles available from MapData
        var legallyEmptyTiles = mapData.tilePositions
            .Where(tile => tile.tileType == TileType.Basic)
            .Select(tile => new Vector2Int(tile.position.x, tile.position.z))
            .Distinct()
            .ToList();

        // 2. Identify every single coordinate that has something physically occupying it globally
        List<Vector2Int> excludedPositions = new List<Vector2Int>();
        
        // Add MapData Painted entities
        if (mapData.enemySpawnPositions != null)
            excludedPositions.AddRange(mapData.enemySpawnPositions.Select(e => new Vector2Int(e.position.x, e.position.z)));

        // --- NEW: Exclude explicitly painted Player Units ---
        if (mapData.playerSpawnPositions != null)
            excludedPositions.AddRange(mapData.playerSpawnPositions.Select(e => new Vector2Int(e.position.x, e.position.z)));
        // ---------------------------------------------------
        
        if (mapData.decorationPositions != null)
            excludedPositions.AddRange(mapData.decorationPositions.Select(d => new Vector2Int(d.position.x, d.position.z)));
            
        if (mapData.interactablePositions != null)
            excludedPositions.AddRange(mapData.interactablePositions.Select(i => new Vector2Int(i.position.x, i.position.z)));

        // Add Systemic starting coordinates (Player + Deity)
        excludedPositions.AddRange(GameManager.Instance.GetPlayerStartingCoordinates());
        excludedPositions.Add(GameManager.Instance.GetDeityStartingCoordinates());

        // 3. Remove the excluded tiles from the legally empty tiles 
        List<Vector2Int> finalValidTiles = legallyEmptyTiles
            .Where(t => !excludedPositions.Contains(t))
            .ToList();

        // 4. Generate random positions strictly from the perfectly safe whitelist
        List<Vector2> enemyPositions = GenerateEnemyPositions(enemyPoolSize, finalValidTiles);

        currentEnemySelectionIds.Clear();
        currentEnemySelectionCoords.Clear();
        currentEnemySelectionIds.AddRange(generatedEnemies);
        currentEnemySelectionCoords.AddRange(enemyPositions);
    }

    private List<EnemyType> GenerateEnemyPool(List<EnemyWeight> weights, int poolSize)
    {
        List<EnemyType> pool = new List<EnemyType>();
        int totalWeight = 0;

        foreach (var weight in weights) totalWeight += weight.weight;

        for (int i = 0; i < poolSize; i++)
        {
            int randomValue = RandomRange(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var weight in weights)
            {
                cumulativeWeight += weight.weight;
                if (randomValue < cumulativeWeight)
                {
                    pool.Add(weight.enemyType);
                    break;
                }
            }
        }
        return pool;
    }

    private List<Vector2> GenerateEnemyPositions(int count, List<Vector2Int> validTiles)
    {
        List<Vector2> finalPositions = new List<Vector2>();

        // Fisher-Yates Shuffle
        for (int i = 0; i < validTiles.Count; i++)
        {
            Vector2Int temp = validTiles[i];
            int randomIndex = Random.Range(i, validTiles.Count);
            validTiles[i] = validTiles[randomIndex];
            validTiles[randomIndex] = temp;
        }

        int spawnCount = Mathf.Min(count, validTiles.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            finalPositions.Add(new Vector2(validTiles[i].x, validTiles[i].y));
        }

        if (finalPositions.Count < count)
        {
            Debug.LogWarning($"EnemyPartyManager: Map is too crowded! Only found {finalPositions.Count} safe spots for {count} enemies.");
        }

        return finalPositions;
    }

    private int RandomRange(int min, int max)
    {
        if (random == null) random = new System.Random();
        return random.Next(min, max);
    }
}
