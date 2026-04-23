using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Roster Configuration")]
    [Tooltip("The current actively available roster. Put your DEFAULT TRIO here in the inspector for new games.")]
    public List<Unit> playerPartyMembers;
    
    [Tooltip("Every possible Unit prefab in the game. Used to load saved units by ID.")]
    public List<Unit> allUnitMasterList = new List<Unit>();

    public List<Unit> playerPartyMembersInstances; // The living GameObjects

    [SerializeField] private EnemyPartyManager _enemyPartyManager;
    [SerializeField] private DeityLinkManager _deityLinkManager;
    [SerializeField] private BuffManager _buffManager;
    [SerializeField] private NodesUnlockManager _nodesUnlockManager;
    public MapData CurrentMap;

    public EnemyPartyManager EnemyPartyManager => _enemyPartyManager;
    public DeityLinkManager DeityLinkManager => _deityLinkManager;
    public BuffManager BuffManager => _buffManager;
    public NodesUnlockManager NodesUnlockManager => _nodesUnlockManager;
    
    public const int MaxActivePartySize = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 1. Load the roster from the save file (if it exists)
            LoadPartyFromSave();
            
            // 2. Instantiate the top 3 members
            InstantiateUnits();
            
            SceneManager.sceneLoaded += OnSceneLoaded; 
        }
        else if (Instance != this)
        {
            Destroy(gameObject); 
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    /// <summary>
    /// Reads the save file to inject the customized party into playerPartyMembers.
    /// If no save data exists, it leaves playerPartyMembers untouched (using the defaults from the Inspector).
    /// </summary>
    private void LoadPartyFromSave()
    {
        GameSaveData currentSave = SaveStateManager.saveData;

        // Ensure we have a master list to draw from
        if (allUnitMasterList == null || allUnitMasterList.Count == 0)
        {
            Debug.LogWarning("GameManager allUnitMasterList is empty! Assign all Unit prefabs in the Inspector.");
            return;
        }

        // If the save file has party data, overwrite the default roster
        if (currentSave != null && (currentSave.activePartyUnitIds.Count > 0 || currentSave.availablePartyUnitIds.Count > 0))
        {
            playerPartyMembers.Clear();

            // Load Active slots first (Ensures they are at indices 0, 1, 2)
            foreach (string activeId in currentSave.activePartyUnitIds)
            {
                Unit foundPref = allUnitMasterList.Find(u => u.Id == activeId);
                if (foundPref != null) playerPartyMembers.Add(foundPref);
            }

            // Load the rest of the available recruits
            foreach (string availId in currentSave.availablePartyUnitIds)
            {
                 Unit foundPref = allUnitMasterList.Find(u => u.Id == availId);
                 if (foundPref != null && !playerPartyMembers.Contains(foundPref))
                 {
                     playerPartyMembers.Add(foundPref);
                 }
            }
            
            Debug.Log("GameManager loaded custom party configuration from Save Data.");
        }

        // --- NEW SAFETY / AUTO-ADD LOGIC ---
        // Ensure any new units added to allUnitMasterList that aren't in the save file (or default trinity)
        // gracefully populate into the available roster! If they aren't locked, they appear automatically.
        foreach (Unit masterUnit in allUnitMasterList)
        {
            if (masterUnit != null && !playerPartyMembers.Contains(masterUnit))
            {
                // NOTE: If you later implement a global "Unlocked/Locked" state check, wrap this add inside an 'if (isUnlocked)'
                playerPartyMembers.Add(masterUnit);
                
                // Immediately push to save data to keep it perfectly synced for next boot
                if (currentSave != null)
                {
                    currentSave.availablePartyUnitIds.Add(masterUnit.Id);
                }
            }
        }
    }

    public void InstantiateUnits()
    {
        // 1. Physically destroy any existing unit instances in the scene first
        foreach (Unit instance in playerPartyMembersInstances)
        {
            if (instance != null && instance.gameObject != null)
            {
                Destroy(instance.gameObject);
            }
        }
        
        // 2. Clear the tracking list
        playerPartyMembersInstances.Clear();

        // 3. Create the new units
        int unitsToInstantiate = Mathf.Min(MaxActivePartySize, playerPartyMembers.Count);

        for (int i = 0; i < unitsToInstantiate; i++)
        {
            Unit newUnitInstance = Instantiate(playerPartyMembers[i], this.gameObject.transform);
            playerPartyMembersInstances.Add(newUnitInstance); 
        }
        
        DeityLinkManager?.ApplyDeityLinks();
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

    public Vector2Int GetDeityStartingCoordinates()
    {
        return new Vector2Int(5, 5);
    }

    public void AddNodesUnlockManager(NodesUnlockManager nodesUnlockManager)
    {
        _nodesUnlockManager = nodesUnlockManager;
    }
}