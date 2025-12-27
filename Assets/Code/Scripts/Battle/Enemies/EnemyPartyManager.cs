using System.Collections.Generic;
using UnityEngine;

public class EnemyPartyManager : MonoBehaviour
{
    public List<EnemyType> currentEnemySelectionIds = new List<EnemyType>();
    public List<Vector2> currentEnemySelectionCoords = new List<Vector2>();

    private System.Random random;

    private void Start()
    {
        // Initialize the random number generator with a seed for consistency
        random = new System.Random(); // Use a specific seed or get it from game data

    }
    // This is a level enemy generator and should have its own dedicated class.
    // Beware: the Enemy start positions generate programmatically BEFORE the battle.
    // Forcibly hooking the Deity logic here.
    public void GenerateEnemyPartyData(EnemyPartyData enemyParty)
    {
        if (GridManager.Instance == null)
            return;
        {
            // Generate a random number of enemies within the specified range
            int enemyPoolSize = RandomRange(enemyParty.minEnemyPoolSize, enemyParty.maxEnemyPoolSize + 1);

            // Generate the enemy pool based on the weights
            List<EnemyType> generatedEnemies = GenerateEnemyPool(enemyParty.enemyWeights, enemyPoolSize);

            // Get player starting coordinates from the GameManager
            List<Vector2Int> playerStartingCoordinates = GameManager.Instance.GetPlayerStartingCoordinates();

            // Get existing tile coordinates from GridManager
            List<Vector2Int> existingTiles = GridManager.Instance.GetExistingTileCoordinates();

            // Exclude tiles occupied by Obstacles/Hazards.

            // Retrieve Deity starting position, add it to the ExcludedCoordinatesList.
            List<Vector2Int> occupiedCoordinates = new List<Vector2Int>(playerStartingCoordinates);
            occupiedCoordinates.Add(GameManager.Instance.GetDeityStartingCoordinates());

            // Generate random positions for the enemies on the grid without overlapping player/Deity starting positions and only on existing tiles
            List<Vector2> enemyPositions = GenerateEnemyPositions(enemyPoolSize, existingTiles, occupiedCoordinates);

            // Update current enemy selection data
            currentEnemySelectionIds.Clear();
            currentEnemySelectionCoords.Clear();
            currentEnemySelectionIds.AddRange(generatedEnemies);
            currentEnemySelectionCoords.AddRange(enemyPositions);
        }
    }

    // This could be moved in its own class
    private List<EnemyType> GenerateEnemyPool(List<EnemyWeight> weights, int poolSize)
    {
        List<EnemyType> pool = new List<EnemyType>();
        int totalWeight = 0;

        foreach (var weight in weights)
        {
            totalWeight += weight.weight;
        }

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

    private List<Vector2> GenerateEnemyPositions(int count, List<Vector2Int> existingTiles, List<Vector2Int> excludedPositions)
    {
        List<Vector2> positions = new List<Vector2>();
        HashSet<Vector2> usedPositions = new HashSet<Vector2>(excludedPositions.ConvertAll(p => (Vector2)p));

        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            int attempt = 0;

            do
            {
                Vector2Int randomTile = existingTiles[RandomRange(0, existingTiles.Count)];
                position = new Vector2(randomTile.x, randomTile.y);
                attempt++;
                if (attempt > 100) // Prevent an infinite loop
                {
                    Debug.LogError("Could not find a suitable position for the enemy.");
                    break;
                }
            }
            while (usedPositions.Contains(position));

            if (!usedPositions.Contains(position))
            {
                positions.Add(position);
                usedPositions.Add(position);
            }
        }

        return positions;
    }

    private int RandomRange(int min, int max)
    {
        return random.Next(min, max);
    }
}
