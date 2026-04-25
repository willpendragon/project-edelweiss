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

        if (allUnitMasterList == null || allUnitMasterList.Count == 0)
        {
            Debug.LogWarning("GameManager allUnitMasterList is empty! Assign all Unit prefabs in the Inspector.");
            return;
        }

        if (currentSave != null && (currentSave.activePartyUnitIds.Count > 0 || currentSave.availablePartyUnitIds.Count > 0))
        {
            playerPartyMembers.Clear();

            // Load Active slots
            foreach (string activeId in currentSave.activePartyUnitIds)
            {
                Unit foundPref = allUnitMasterList.Find(u => u.Id == activeId);
                if (foundPref != null) playerPartyMembers.Add(foundPref);
            }

            // Load Recruits
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
        
        // WE DELETED THE AUTO-ADD LOOP HERE.
        // If a character is in the master list but not in the save file, they remain hidden/locked!
    }

    public void InstantiateUnits()
    {
        // 1. Physically destroy any existing unit instances in the scene first, BUT save their current stats!
        foreach (Unit instance in playerPartyMembersInstances)
        {
            if (instance != null && instance.gameObject != null)
            {
                SaveUnitStats(instance); // Snapshot HP/Mana before destroying
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
            
            // 4. Inject the saved stats (HP, Mana, etc.) into the newly created prefab clone
            LoadUnitStats(newUnitInstance);
            
            playerPartyMembersInstances.Add(newUnitInstance); 
        }
        
        DeityLinkManager?.ApplyDeityLinks();
    }

    /// <summary>
    /// Snapshots the current runtime state of a living Unit into the Save Data.
    /// </summary>
    private void SaveUnitStats(Unit unit)
    {
        GameSaveData saveData = SaveStateManager.saveData;
        if (saveData == null || string.IsNullOrEmpty(unit.Id)) return;

        CharacterData charData = saveData.characterData.Find(c => c.unitId == unit.Id);
        if (charData == null)
        {
            charData = new CharacterData();
            charData.unitId = unit.Id;
            saveData.characterData.Add(charData);
        }

        // Keep snapshot of the dynamic combat stats
        charData.unitHealthPoints = unit.unitHealthPoints;
        charData.unitSavedManaPoints = unit.unitManaPoints;
        charData.unitShieldPoints = unit.unitShieldPoints;
        charData.unitLifeCondition = unit.currentUnitLifeCondition;
        charData.unitOccupiedFoodSlots = unit.unitOccupiedFoodSlots;
        
        // Keep snapshot of the progression stats (in case of cafe upgrades)
        charData.unitAttackPower = unit.unitAttackPower;
        charData.unitMagicPower = unit.unitMagicPower;
        charData.unitFaithPoints = unit.unitFaithPoints;

        // Optionally force a file write here, though usually handled globally later
    }

    /// <summary>
    /// Restores the snapshot of a unit from Save Data. If no snapshot exists, creates one from the base Template.
    /// </summary>
    private void LoadUnitStats(Unit unit)
    {
        // Force the unit to load its baseline stats from its template first.
        // This guarantees things like MaxHP or Experience curves are fully initialized.
        if (unit.unitTemplate != null)
        {
            unit.RetrieveTemplateValues();
        }

        GameSaveData saveData = SaveStateManager.saveData;
        if (saveData == null || string.IsNullOrEmpty(unit.Id)) return;

        CharacterData charData = saveData.characterData.Find(c => c.unitId == unit.Id);
        if (charData != null)
        {
            // Overwrite the fresh template values with the saved runtime values
            unit.unitHealthPoints = charData.unitHealthPoints;
            unit.unitManaPoints = charData.unitSavedManaPoints;
            unit.unitShieldPoints = charData.unitShieldPoints;
            unit.currentUnitLifeCondition = charData.unitLifeCondition;
            unit.unitOccupiedFoodSlots = charData.unitOccupiedFoodSlots;

            // If upgraded mid-run, apply the stronger base values too
            if (charData.unitAttackPower > 0) unit.unitAttackPower = charData.unitAttackPower;
            if (charData.unitMagicPower > 0) unit.unitMagicPower = charData.unitMagicPower;
            if (charData.unitFaithPoints > 0) unit.unitFaithPoints = charData.unitFaithPoints;

            // Force visual HP bar updates UI event checks
            unit.onHealthChanged?.Invoke(unit.unitHealthPoints);
        }
        else
        {
            // This is the absolute first time this unit was ever spawned in this Save File scenario.
            // Take the fresh Template baseline and register it in the Save memory!
            SaveUnitStats(unit);
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

    public Vector2Int GetDeityStartingCoordinates()
    {
        // Intercept from gridMapDictionary instead of gridTileControllers to avoid Star/Awake race conditions!
        if (GridManager.Instance != null && GridManager.Instance.gridMapDictionary != null)
        {
            foreach (var tile in GridManager.Instance.gridMapDictionary.Values)
            {
                if (tile.tileType == TileType.DeityTile)
                {
                    // Accessing gridPosition directly is safer since MapData paints via gridPosition
                    // X maps to horizontal, Z maps to the logical map 'Y' depth
                    return new Vector2Int(tile.gridPosition.x, tile.gridPosition.z);
                }
            }
        }
        
        // Fallback hard-coded default if DeityTile is absent
        return new Vector2Int(5, 5);
    }

    public void AddNodesUnlockManager(NodesUnlockManager nodesUnlockManager)
    {
        _nodesUnlockManager = nodesUnlockManager;
    }
}