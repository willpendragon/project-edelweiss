using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public NodeController currentNode;
    public static GameManager Instance;
    public GameObject[] currentEnemySelection;
    public EnemySelection currentEnemySelectionComponent;
    public List<EnemyType> currentEnemySelectionIds = new List<EnemyType>();
    public List<Vector2> currentEnemySelectionCoords = new List<Vector2>();
    public List<Unit> playerPartyMembers;
    public List<Unit> playerPartyMembersInstances;
    public List<Deity> collectibleDeities;

    public DeityLinkController deityLinkController;

    private System.Random random;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InstantiateUnits();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event

            // Initialize the random number generator with a seed for consistency
            random = new System.Random(); // Use a specific seed or get it from game data
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance of GameManager exists
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is now correctly subscribed to the SceneManager.sceneLoaded event
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Attempt to access the DeityLinkController component safely
        if (deityLinkController != null)
        {
            //deityLinkController.LoadGame(); // Call LoadGame only if the component is found
        }
        else
        {
            Debug.LogWarning("DeityLinkController component not found on GameManager GameObject.");
        }
    }

    public void MarkCurrentNodeAsCompleted()
    {
        currentNode.nodeCompleted = true;
    }

    public void DefineEnemySelection(GameObject[] enemySelection)
    {
        //currentEnemySelection = enemySelection;
    }

    public void InstantiateUnits()
    {
        // Clear the existing instances list
        playerPartyMembersInstances.Clear();

        // Go through all the prefabs and instantiate them
        foreach (var unitPrefab in playerPartyMembers)
        {
            Unit newUnitInstance = Instantiate(unitPrefab, this.gameObject.transform);
            playerPartyMembersInstances.Add(newUnitInstance); // Add the new instance to the list
        }
        Debug.Log("Instantiated Player Units");
        ApplyDeityLinks();
    }

    public void ApplyDeityLinks()
    {
        Dictionary<string, string> unitsLinkedToDeities = SaveStateManager.saveData.unitsLinkedToDeities;
        foreach (var entry in unitsLinkedToDeities)
        {
            string unitID = entry.Key;
            string deityID = entry.Value;
        }

        foreach (var unitPrefab in playerPartyMembersInstances)
        {
            unitsLinkedToDeities.TryGetValue(unitPrefab.GetComponent<Unit>().Id, out string connectedDeity);
            unitPrefab.GetComponent<Unit>().LinkedDeityId = connectedDeity;
            unitPrefab.GetComponent<Unit>().linkedDeity = collectibleDeities.Find(deity => deity.Id == unitPrefab.LinkedDeityId);
        }
    }

    public List<Vector2Int> GetPlayerStartingCoordinates()
    {
        List<Vector2Int> startingCoordinates = new List<Vector2Int>();
        foreach (var playerUnit in playerPartyMembersInstances)
        {
            Unit unit = playerUnit.GetComponent<Unit>();
            startingCoordinates.Add(new Vector2Int(unit.startingXCoordinate, unit.startingYCoordinate));
        }
        return startingCoordinates;
    }

    public void GenerateLevelData(Level level)
    {
        if (GridManager.Instance != null)
        {
            // Generate a random number of enemies within the specified range
            int enemyPoolSize = RandomRange(level.minEnemyPoolSize, level.maxEnemyPoolSize + 1);

            // Generate the enemy pool based on the weights
            List<EnemyType> generatedEnemies = GenerateEnemyPool(level.enemyWeights, enemyPoolSize);

            // Get player starting coordinates from the GameManager
            List<Vector2Int> playerStartingCoordinates = GetPlayerStartingCoordinates();

            // Get existing tile coordinates from GridManager
            List<Vector2Int> existingTiles = GridManager.Instance.GetExistingTileCoordinates();

            // Generate random positions for the enemies on the grid without overlapping player starting positions and only on existing tiles
            List<Vector2> enemyPositions = GenerateEnemyPositions(enemyPoolSize, existingTiles, playerStartingCoordinates);

            // Update current enemy selection data
            currentEnemySelectionIds.Clear();
            currentEnemySelectionCoords.Clear();
            currentEnemySelectionIds.AddRange(generatedEnemies);
            currentEnemySelectionCoords.AddRange(enemyPositions);
        }
    }

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

    private float RandomRange(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }
}
