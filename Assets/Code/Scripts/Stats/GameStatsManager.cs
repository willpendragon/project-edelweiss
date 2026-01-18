using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStatsManager : MonoBehaviour

{
    public int currentDay;
    public int enemiesKilled;
    public float warFunds;
    public int timesSingleTargetSpellWasUsed;
    public int captureCrystalsCount;
    public int unlockedPuzzleKeys;
    public Inventory inventory;
    [SerializeField] FaithController faithController;
    [SerializeField] TurnController _turnController;

    [SerializeField] private List<Ingredient> allIngredientPrototypes;

    private CharacterData characterData;

    public void Awake()
    {
        //LoadCalendarData();
        LoadWarFunds();
        LoadEnemiesKilled();
        LoadUsedSingleTargetSpells();

        //LoadCaptureCrystalsCount();
        LoadUnlockedKeys();
    }
    void Start()
    {
        LoadDeityTributesFromBakedItems();
        LoadCharacterData();
        // Load ingredients AFTER PersistentInventoryManager has been initialized.
        StartCoroutine(DelayedLoadIngredients());

        if (faithController != null)
        {
            faithController.DecreaseFaithPoints();
        }
    }

    IEnumerator DelayedLoadIngredients()
    {
        // Wait until the CurrentInventory is initialized
        yield return new WaitUntil(() => PersistentInventoryManager.CurrentInventory != null);

        LoadIngredients(allIngredientPrototypes);
    }

    public void SaveCharacterData()
    {
        //GameObject[] playerUnits = _turnController.playerUnitsOnBattlefield;
        List<Unit> playerUnits = GameManager.Instance.playerPartyMembersInstances;
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in playerUnits)
        {
            //Unit unitComponent = playerUnit.GetComponent<Unit>();
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == playerUnit.Id);

            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitHealthPoints = playerUnit.unitHealthPoints;
                existingCharacterData.unitSavedManaPoints = playerUnit.unitManaPoints;
                existingCharacterData.unitShieldPoints = playerUnit.unitShieldPoints;
                existingCharacterData.unitLifeCondition = playerUnit.currentUnitLifeCondition;
                existingCharacterData.unitAttackPower = playerUnit.unitAttackPower;
                existingCharacterData.unitMagicPower = playerUnit.unitMagicPower;
                existingCharacterData.unitFaithPoints = playerUnit.unitFaithPoints;
                existingCharacterData.unitOccupiedFoodSlots = playerUnit.unitOccupiedFoodSlots;

                SaveBuffsToData(playerUnit.gameObject, existingCharacterData);
            }
            else if (existingCharacterData == null)
            {
                // Add new character data
                CharacterData newCharacterData = new CharacterData()
                {
                    unitId = playerUnit.Id,
                    unitHealthPoints = playerUnit.unitHealthPoints,
                    unitSavedManaPoints = playerUnit.unitManaPoints,
                    unitShieldPoints = playerUnit.unitShieldPoints,
                    unitLifeCondition = playerUnit.currentUnitLifeCondition,
                    unitAttackPower = playerUnit.unitAttackPower,
                    unitMagicPower = playerUnit.unitMagicPower,
                    unitFaithPoints = playerUnit.unitFaithPoints,
                    unitOccupiedFoodSlots = playerUnit.unitOccupiedFoodSlots
                };

                SaveBuffsToData(playerUnit.gameObject, newCharacterData);
                characterSaveData.characterData.Add(newCharacterData);
            }
        }
        SaveStateManager.SaveGame(characterSaveData);
    }

    public void LoadCharacterData()
    {
        if (GameManager.Instance == null)
            return;
        List<Unit> playerUnits = GameManager.Instance.playerPartyMembersInstances;

        GameSaveData characterSaveData = SaveStateManager.saveData;
        foreach (var playerUnit in playerUnits)
        {
            Unit unitComponent = playerUnit.GetComponent<Unit>();
            CharacterData loadedCharacterData = characterSaveData.characterData.Find(character => character.unitId == unitComponent.Id);
            if (loadedCharacterData != null)
            {
                unitComponent.unitHealthPoints = loadedCharacterData.unitHealthPoints;
                unitComponent.unitManaPoints = loadedCharacterData.unitSavedManaPoints;
                unitComponent.unitShieldPoints = loadedCharacterData.unitShieldPoints;
                unitComponent.currentUnitLifeCondition = loadedCharacterData.unitLifeCondition;
                unitComponent.unitAttackPower = loadedCharacterData.unitAttackPower;
                unitComponent.unitMagicPower = loadedCharacterData.unitMagicPower;
                unitComponent.unitFaithPoints = loadedCharacterData.unitFaithPoints;
                Debug.Log("Restoring Player Units HP and Mana");
                // Loads the occupied food slots. The Saving happens in the Café only and when buffs expire.
                unitComponent.unitOccupiedFoodSlots = loadedCharacterData.unitOccupiedFoodSlots;

                LoadBuffsFromData(unitComponent.gameObject, loadedCharacterData);
            }
        }
        //}
    }
    public void LoadWarFunds()
    {
        GameSaveData resourceSaveData = SaveStateManager.saveData;
        if (resourceSaveData != null && resourceSaveData.resourceData != null)
        {
            warFunds = resourceSaveData.resourceData.warFunds;
            Debug.Log($"Loaded War Funds: {warFunds}");
        }
    }

    public void LoadCalendarData()
    {
        GameSaveData calendarSaveData = SaveStateManager.saveData;
        if (calendarSaveData != null && calendarSaveData.calendarData != null)
        {
            currentDay = calendarSaveData.calendarData.currentDay;
            Debug.Log($"Loaded Current Day: {currentDay}");
        }
    }

    private void SaveBuffsToData(GameObject unitObject, CharacterData data)
    {
        UnitBuffController buffController = unitObject.GetComponent<UnitBuffController>();
        if (buffController == null)
            return;

        data.activeBuffs.Clear();

        var runtimeBuffs = buffController.GetActiveBuffs();

        foreach (var kvp in runtimeBuffs)
        {
            foreach (var entry in kvp.Value)
            {
                data.activeBuffs.Add(new SavedBuffEntry
                {
                    type = entry.Type,
                    appliedValue = entry.AppliedValue,
                    remainingDuration = entry.RemainingDurationDays
                });
            }
        }
    }

    private void LoadBuffsFromData(GameObject unitObject, CharacterData data)
    {
        UnitBuffController buffController = unitObject.GetComponent<UnitBuffController>();
        if (buffController == null || data.activeBuffs == null) return;

        buffController.ClearAppliedBuffs();
        foreach (var savedBuff in data.activeBuffs)
        {
            // Pass 'true' for isLoading to prevent stats from being added twice
            buffController.CreateAppliedBuffEntry(savedBuff.appliedValue, savedBuff.remainingDuration, savedBuff.type, true);
        }
    }

    public void SaveCalendarData(int daysPassed)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.calendarData != null)
        {
            gameSaveData.calendarData.currentDay += daysPassed;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved Days Passed: {daysPassed}");
        }
    }
    public void SaveWarFunds(float newWarFunds)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.warFunds += newWarFunds;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved War Funds: {newWarFunds}");
        }
    }

    public void SaveSpentWarFunds(float spentWarFunds)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.warFunds -= spentWarFunds;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Spent War Funds: {spentWarFunds}");
        }
    }
    public void LoadUnlockedKeys()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            unlockedPuzzleKeys = gameSaveData.resourceData.puzzleLevelKeys;
            Debug.Log($"Loaded Keys: {unlockedPuzzleKeys}");
        }
    }
    public void SaveUnlockedKeys(int unlockedKeys)
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.puzzleLevelKeys += unlockedKeys;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved Keys: {unlockedKeys}");
        }
    }
    public void LoadEnemiesKilled()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            enemiesKilled = gameSaveData.enemiesKilled;
        }
    }

    public void LoadDeityTributesFromBakedItems()
    {
        GameSaveData save = SaveStateManager.saveData;

        if (save != null && save.bakedItems != null)
        {
            var deityEntries = save.bakedItems
                .FindAll(item => item.pastryName == "DeityTribute");

            captureCrystalsCount = deityEntries.Sum(item => item.quantity);

            Debug.Log($"Loaded Deity Tributes Battle Items Total: {captureCrystalsCount}");
        }
        else
        {
            captureCrystalsCount = 0;
            Debug.LogWarning("Could not load deity tributes.");
        }
    }

    public void ConsumeDeityTribute()
    {
        GameSaveData save = SaveStateManager.saveData;

        if (save?.bakedItems == null)
            return;

        // Search for any entry with pastryName == "DeityTribute"
        for (int i = 0; i < save.bakedItems.Count; i++)
        {
            BakedItemsData item = save.bakedItems[i];

            if (item.pastryName == "DeityTribute")
            {
                if (item.quantity > 1)
                {
                    item.quantity--;    // reduce this entry
                }
                else
                {
                    save.bakedItems.RemoveAt(i);  // remove entry entirely
                }

                SaveStateManager.SaveGame(save);

                Debug.Log("Consumed ONE DeityTribute from bakedItems.");
                return;
            }
        }

        Debug.LogWarning("Tried to consume a DeityTribute but none were found.");
    }


    public void SaveCaptureCrystalsCount()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData.resourceData != null)
        {
            gameSaveData.resourceData.captureCrystalsCount = captureCrystalsCount;
            SaveStateManager.SaveGame(gameSaveData);
            Debug.Log($"Saved Capture Crystals: {captureCrystalsCount}");
        }
    }
    public void SaveEnemiesKilled()
    {
        Debug.Log("Increasing Enemies Killed");

        // Prepare the Save Data
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.enemiesKilled = enemiesKilled;

        SaveStateManager.SaveGame(saveData);
    }
    public void LoadUsedSingleTargetSpells()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        if (gameSaveData != null)
        {
            timesSingleTargetSpellWasUsed = gameSaveData.timesSingleTargetSpellWasUsed;
        }
    }
    public void SaveUsedSingleTargetSpells()
    {
        Debug.Log("Increasing Used Single Target Spells statistics");

        // Prepare the save data
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.timesSingleTargetSpellWasUsed = timesSingleTargetSpellWasUsed;

        SaveStateManager.SaveGame(saveData);
    }
    public void SaveIngredients()
    {
        var currentInventoryData = PersistentInventoryManager.ToSaveData(PersistentInventoryManager.CurrentInventory);
        var savedInventory = SaveStateManager.saveData.savedInventory;

        // Merge or add new ingredients
        foreach (var newEntry in currentInventoryData)
        {
            var existingEntry = savedInventory.Find(e => e.ingredientName == newEntry.ingredientName);

            if (existingEntry != null)
            {
                existingEntry.quantity += newEntry.quantity;
            }
            else
            {
                savedInventory.Add(new IngredientSaveEntry
                {
                    ingredientName = newEntry.ingredientName,
                    quantity = newEntry.quantity
                });
            }
        }

        SaveStateManager.saveData.savedInventory = savedInventory;
        SaveStateManager.SaveGame(SaveStateManager.saveData);
    }

    public void SaveIngredientsAfterBaking()
    {
        var currentInventoryData = PersistentInventoryManager.ToSaveData(PersistentInventoryManager.CurrentInventory);

        // Create a fresh list that matches exactly what's in memory.
        List<IngredientSaveEntry> newSavedInventory = new List<IngredientSaveEntry>();

        foreach (var entry in currentInventoryData)
        {
            newSavedInventory.Add(new IngredientSaveEntry
            {
                ingredientName = entry.ingredientName,
                quantity = entry.quantity
            });
        }

        SaveStateManager.saveData.savedInventory = newSavedInventory;

        SaveStateManager.SaveGame(SaveStateManager.saveData);

        Debug.Log($"[SaveIngredients] Inventory saved ({newSavedInventory.Count} ingredients).");
    }

    public void LoadIngredients(List<Ingredient> allIngredientPrototypes)
    {
        PersistentInventoryManager.FromSaveData(
            SaveStateManager.saveData.savedInventory,
            PersistentInventoryManager.CurrentInventory,
            allIngredientPrototypes
        );
        Debug.Log("Loaded ingredients into runtime inventory.");
    }

}
