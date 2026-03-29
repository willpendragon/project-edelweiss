using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Unit> playerPartyMembers;
    public List<Unit> playerPartyMembersInstances;

    [SerializeField] private EnemyPartyManager _enemyPartyManager;
    [SerializeField] private DeityLinkManager _deityLinkManager;
    [SerializeField] private BuffManager _buffManager;
    [SerializeField] private NodesUnlockManager _nodesUnlockManager;
    public MapData CurrentMap;

    public EnemyPartyManager EnemyPartyManager => _enemyPartyManager;
    public DeityLinkManager DeityLinkManager => _deityLinkManager;

    public BuffManager BuffManager => _buffManager;
    public NodesUnlockManager NodesUnlockManager => _nodesUnlockManager;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InstantiateUnits();
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
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
    }

    // This can be separated in a class that manages the Units
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
        DeityLinkManager.ApplyDeityLinks();
    }

    // Move to its own class.
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

    public Vector2Int GetDeityStartingCoordinates()
    {
        // Band-aid fix, beware. This will prevent ALWAYS prevent Enemies from spawning on 5,5 tile.
        Vector2Int deityStartingPosition = new Vector2Int(5, 5);
        return deityStartingPosition;
    }

    public void AddNodesUnlockManager(NodesUnlockManager nodesUnlockManager)
    {
        _nodesUnlockManager = nodesUnlockManager;
    }
}