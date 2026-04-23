using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Added for SceneManager
using System.Linq; 

public class RecruitManager : MonoBehaviour
{
    public static RecruitManager Instance { get; private set; }

    public const int MaxActivePartySize = 3;

    [Header("Recruit Status Lists")]
    public List<Unit> lockedUnits = new List<Unit>();

    // This array acts as the exact 3 slots. Null means the slot is empty.
    public Unit[] selectedActiveParty = new Unit[MaxActivePartySize];

    public List<Unit> AvailablePartyMembers => GameManager.Instance != null ? GameManager.Instance.playerPartyMembers : new List<Unit>();

    // We keep a unified list of ALL known units to fetch prefabs based on saved IDs
    [Header("All Possible Units")]
    [Tooltip("Place every possible Unit prefab here so we can find them by ID on load")]
    public List<Unit> allUnitMasterList = new List<Unit>();

    [Header("UI References")]
    public Transform activePartyGrid;
    public Transform availableUnitsGrid;
    public GameObject unitPortraitPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadPartyFromSave();
        RefreshUI();
    }

    private void Update()
    {
        // Debug command: Press F12 to jump straight to the overworld and test party composition
        if (Input.GetKeyDown(KeyCode.F12))
        {
            DebugLoadOverworld();
        }
    }

    /// <summary>
    /// Debug method to immediately load the overworld map scene.
    /// You can also link this method to a UI "Continue" button.
    /// </summary>
    public void DebugLoadOverworld()
    {
        Debug.Log("Loading overworld_map to test party persistence...");
        SceneManager.LoadScene("overworld_map");
    }

    /// <summary>
    /// Loads the active party and recruits from the current GameSaveData via SaveStateManager.
    /// If no save exists yet, defaults to the GameManager's default unit setup.
    /// </summary>
    private void LoadPartyFromSave()
    {
        GameSaveData currentSave = SaveStateManager.saveData;

        // Verify the master list is populated
        if (allUnitMasterList == null || allUnitMasterList.Count == 0)
        {
            Debug.LogWarning("allUnitMasterList is empty! Please assign all Unit Prefabs in the RecruitManager inspector.");
        }

        // If we have saved data and it has actual party configuration saved
        if (currentSave != null && (currentSave.activePartyUnitIds.Count > 0 || currentSave.availablePartyUnitIds.Count > 0))
        {
            GameManager.Instance.playerPartyMembers.Clear();

            // Load Active slots
            for (int i = 0; i < currentSave.activePartyUnitIds.Count; i++)
            {
                string idToFind = currentSave.activePartyUnitIds[i];
                Unit foundPref = allUnitMasterList.FirstOrDefault(u => u.Id == idToFind);
                if (foundPref != null)
                {
                    selectedActiveParty[i] = foundPref;
                    GameManager.Instance.playerPartyMembers.Add(foundPref);
                }
                else
                {
                    Debug.LogError($"Could not find Unit with ID {idToFind} in allUnitMasterList!");
                }
            }

            // Load Available Pool (The rest of the roster)
            foreach (string availId in currentSave.availablePartyUnitIds)
            {
                 Unit foundPref = allUnitMasterList.FirstOrDefault(u => u.Id == availId);
                 if (foundPref != null && !GameManager.Instance.playerPartyMembers.Contains(foundPref))
                 {
                     GameManager.Instance.playerPartyMembers.Add(foundPref);
                 }
            }
        }
        else
        {
            // First time playing / No save file / Nothing configured -> Default load
            int limit = Mathf.Min(MaxActivePartySize, AvailablePartyMembers.Count);
            for (int i = 0; i < limit; i++)
            {
                selectedActiveParty[i] = AvailablePartyMembers[i];
            }
        }
    }

    /// <summary>
    /// Writes the current roster back to GameSaveData and forces a file save.
    /// </summary>
    private void SavePartyToSaveFile()
    {
        GameSaveData currentSave = SaveStateManager.saveData;
        if (currentSave == null) return;

        // Save Active Members
        currentSave.activePartyUnitIds.Clear();
        foreach (Unit active in selectedActiveParty)
        {
            if (active != null)
                currentSave.activePartyUnitIds.Add(active.Id);
        }

        // Save Available Members
        currentSave.availablePartyUnitIds.Clear();
        foreach (Unit available in AvailablePartyMembers)
        {
            // If they aren't deployed in an active slot, add them back to the available ID lists
            if (!System.Array.Exists(selectedActiveParty, u => u != null && u.unitTemplate == available.unitTemplate))
            {
                 currentSave.availablePartyUnitIds.Add(available.Id);
            }
        }

        // Trigger your global static save logic
        SaveStateManager.SaveGame(currentSave);
    }

    public void RefreshUI()
    {
        PopulateActiveGrid();
        PopulateAvailableGrid();
    }

    private void PopulateActiveGrid()
    {
        foreach (Transform child in activePartyGrid) Destroy(child.gameObject);

        for (int i = 0; i < MaxActivePartySize; i++)
        {
            GameObject slotInstance = Instantiate(unitPortraitPrefab, activePartyGrid);
            RecruitSlotUI slotUI = slotInstance.GetComponent<RecruitSlotUI>() ?? slotInstance.AddComponent<RecruitSlotUI>();
            slotUI.isActiveSlot = true;
            slotUI.slotIndex = i;
            slotUI.assignedUnit = selectedActiveParty[i];

            Image portraitImage = slotInstance.GetComponentInChildren<Image>();
            if (portraitImage != null && selectedActiveParty[i] != null && selectedActiveParty[i].unitTemplate != null)
            {
                portraitImage.sprite = selectedActiveParty[i].unitTemplate.unitPortrait;
                portraitImage.color = Color.white;
            }
            else if (portraitImage != null)
            {
                // Slot is empty
                portraitImage.sprite = null; 
                portraitImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Gray out empty slots
            }
        }
    }

    private void PopulateAvailableGrid()
    {
        foreach (Transform child in availableUnitsGrid) Destroy(child.gameObject);

        foreach (Unit unit in AvailablePartyMembers)
        {
            if (unit == null) continue;

            GameObject slotInstance = Instantiate(unitPortraitPrefab, availableUnitsGrid);
            RecruitSlotUI slotUI = slotInstance.GetComponent<RecruitSlotUI>() ?? slotInstance.AddComponent<RecruitSlotUI>();
            slotUI.isActiveSlot = false;
            slotUI.assignedUnit = unit;

            Image portraitImage = slotInstance.GetComponentInChildren<Image>();
            if (portraitImage != null && unit.unitTemplate != null)
            {
                portraitImage.sprite = unit.unitTemplate.unitPortrait;

                // Highlight red if they are currently an active slotted character
                bool isCurrentlyActive = System.Array.Exists(selectedActiveParty, 
                    activeUnit => activeUnit != null && activeUnit.unitTemplate == unit.unitTemplate);

                portraitImage.color = isCurrentlyActive ? Color.red : Color.white;
            }
        }
    }

    public void RemoveFromActiveParty(int slotIndex)
    {
        selectedActiveParty[slotIndex] = null;
        SyncWithGameManager();
        SavePartyToSaveFile();
        RefreshUI();
    }

    public void AddToActiveParty(Unit unitToAdd)
    {
        // Don't add if already in an active slot
        if (System.Array.Exists(selectedActiveParty, u => u != null && u.unitTemplate == unitToAdd.unitTemplate))
            return;

        // Assign to first empty (null) slot
        for (int i = 0; i < MaxActivePartySize; i++)
        {
            if (selectedActiveParty[i] == null)
            {
                selectedActiveParty[i] = unitToAdd;
                SyncWithGameManager();
                SavePartyToSaveFile(); // Persist changes
                RefreshUI();
                return;
            }
        }
        
        Debug.Log("Active party is full!");
    }

    /// <summary>
    /// Re-orders GameManager.playerPartyMembers so the assigned units are grouped at the front.
    /// </summary>
    private void SyncWithGameManager()
    {
        List<Unit> newOrder = new List<Unit>();

        // Add Active
        foreach (Unit active in selectedActiveParty)
        {
            if (active != null) newOrder.Add(active);
        }

        // Add the rest
        foreach ( Unit rosterUnit in GameManager.Instance.playerPartyMembers)
        {
            if (!newOrder.Exists(u => u.unitTemplate == rosterUnit.unitTemplate))
            {
                newOrder.Add(rosterUnit);
            }
        }

        // FIX: Modify the existing list instead of replacing the reference to prevent Unity UI Toolkit Editor errors 
        GameManager.Instance.playerPartyMembers.Clear();
        GameManager.Instance.playerPartyMembers.AddRange(newOrder);
    }
}
