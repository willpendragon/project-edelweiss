using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ConversationData;

public class CafeSaveManager : MonoBehaviour

{
    [Header("References")]
    [SerializeField] private PastrySlotUIController pastryUI;

    [Header("Database")]
    [Tooltip("Drag all ItemFood SOs here to allow the save system to find them by name.")]
    [SerializeField] private List<ItemFood> allFoodRegistry;


    //public void SaveRestoredCharacterStats()
    //{
    //    //Saves the stats after feeding.
    //    GameSaveData characterSaveData = SaveStateManager.saveData;

    //    foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
    //    {
    //        CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == playerUnit.Id);
    //        if (existingCharacterData != null)
    //        {
    //            // Update existing character data
    //            existingCharacterData.unitHealthPoints = playerUnit.unitHealthPoints;
    //            existingCharacterData.unitSavedManaPoints = playerUnit.unitManaPoints;
    //            existingCharacterData.unitShieldPoints = playerUnit.unitShieldPoints;

    //            existingCharacterData.unitLifeCondition = playerUnit.currentUnitLifeCondition;

    //            existingCharacterData.unitAttackPower = playerUnit.unitAttackPower;
    //            existingCharacterData.unitMagicPower = playerUnit.unitMagicPower;

    //            // Update other stats as necessary
    //            existingCharacterData.unitOccupiedFoodSlots = playerUnit.unitOccupiedFoodSlots;
    //            Debug.Log("Character Stats Saved");
    //        }
    //    }
    //    SaveStateManager.SaveGame(characterSaveData);
    //}

    public void Start()
    {
        LoadEatenPastry();
    }

    public void SaveEatenPastry()
    {
        GameSaveData saveData = SaveStateManager.saveData;

        // Clear the old history to overwrite with fresh data
        saveData.eatenPastriesHistory.Clear();

        // Convert Dictionary from UI Controller into our Serializable Class
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            var history = pastryUI.GetHistory(playerUnit);
            if (history.Count > 0)
            {
                List<string> names = history.Select(f => f.itemFoodName).ToList();
                saveData.eatenPastriesHistory.Add(new EatenPastryData(playerUnit.Id, names));
            }
        }

        SaveStateManager.SaveGame(saveData);
        Debug.Log("Pastry History Serialized and Saved.");
    }

    public void LoadEatenPastry()
    {
        // 1. Get the current save file
        GameSaveData saveData = SaveStateManager.saveData;
        if (saveData.eatenPastriesHistory == null) return;

        // 2. Loop through history and restore the UI
        foreach (var entry in saveData.eatenPastriesHistory)
        {
            // Find the living Unit instance by ID
            Unit unit = GameManager.Instance.playerPartyMembersInstances
                        .Find(u => u.Id == entry.unitId);

            if (unit != null)
            {
                foreach (string foodName in entry.foodNames)
                {
                    // Find the matching ScriptableObject in your Registry List
                    ItemFood foodAsset = allFoodRegistry.FirstOrDefault(f => f.itemFoodName == foodName);

                    if (foodAsset != null)
                    {
                        // Restore the item to the UI Controller
                        pastryUI.TrackEatenFood(unit, foodAsset);
                    }
                    else
                    {
                        Debug.LogWarning($"[CafeSaveManager] Saved food '{foodName}' not found in AllFoodRegistry.");
                    }
                }
            }
        }
        Debug.Log("Pastry History Loaded from Save.");
    }

    public void SaveSpecificUnitHistory(Unit unit)
    {
        // Get the current save file.
        GameSaveData saveData = SaveStateManager.saveData;

        // Get the current history from the UI.
        var currentHistory = pastryUI.GetHistory(unit);
        List<string> foodNames = currentHistory.Select(f => f.itemFoodName).ToList();

        // Find if this unit already has a history entry in the save file.
        var existingEntry = saveData.eatenPastriesHistory.Find(e => e.unitId == unit.Id);

        if (existingEntry != null)
        {
            // Update existing entry.
            existingEntry.foodNames = foodNames;
        }
        else
        {
            // Create new entry
            saveData.eatenPastriesHistory.Add(new EatenPastryData(unit.Id, foodNames));
        }

        // Update Character Stats (HP, Mana, Upgrade, Filled Slots).
        SaveRestoredCharacterStats(unit);

        // Write to Disk
        SaveStateManager.SaveGame(saveData);
        Debug.Log($"Saved bite for {unit.unitTemplate.unitName}");
    }

    // Helper to save stats for a Unit.
    public void SaveRestoredCharacterStats(Unit specificUnit)
    {
        GameSaveData characterSaveData = SaveStateManager.saveData;

        CharacterData charData = characterSaveData.characterData.Find(c => c.unitId == specificUnit.Id);
        if (charData != null)
        {
            charData.unitHealthPoints = specificUnit.unitHealthPoints;
            charData.unitSavedManaPoints = specificUnit.unitManaPoints;
            charData.unitShieldPoints = specificUnit.unitShieldPoints;
            charData.unitLifeCondition = specificUnit.currentUnitLifeCondition;
            charData.unitOccupiedFoodSlots = specificUnit.unitOccupiedFoodSlots;
            // Upgrade stats.
            charData.unitAttackPower = specificUnit.unitAttackPower;
            charData.unitShieldPoints = specificUnit.unitShieldPoints;
        }
    }
}