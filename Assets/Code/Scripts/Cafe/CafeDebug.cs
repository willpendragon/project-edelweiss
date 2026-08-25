using System.Collections.Generic;
using UnityEngine;

public class DebugFillResources : MonoBehaviour
{

    [ContextMenu("Debug: Fill War Funds")]
    
        void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            FillEverything();
        }       
    }

    
    public void FillWarFunds()
    {
        GameStatsManager gameStatsManager = FindObjectOfType<GameStatsManager>();
        if (gameStatsManager == null)
        {
            Debug.LogError("GameStatsManager not found in scene!");
            return;
        }

        float fillAmount = 10000f; // Set to whatever amount you want
        gameStatsManager.warFunds += fillAmount;
        gameStatsManager.SaveWarFunds(fillAmount);
        Debug.Log($"[DEBUG] War Funds filled! Current: {gameStatsManager.warFunds}");
    }

    [ContextMenu("Debug: Fill All Ingredients")]
    public void FillAllIngredients()
    {
        if (PersistentInventoryManager.CurrentInventory == null)
        {
            Debug.LogError("PersistentInventoryManager.CurrentInventory is null!");
            return;
        }

        if (PersistentInventoryManager.Instance == null)
        {
            Debug.LogError("PersistentInventoryManager.Instance not found!");
            return;
        }

        int quantityPerIngredient = 10; // Set to whatever quantity you want

        foreach (var ingredient in PersistentInventoryManager.Instance.AllIngredientPrototypes)
        {
            PersistentInventoryManager.CurrentInventory.Add(ingredient, quantityPerIngredient);
            Debug.Log($"[DEBUG] Added {ingredient.ingredientName} x{quantityPerIngredient}");
        }

        // Sync to save file
        GameStatsManager gameStatsManager = FindObjectOfType<GameStatsManager>();
        if (gameStatsManager != null)
        {
            SaveIngredientsDebug();
        }

        Debug.Log("[DEBUG] All ingredients filled!");
    }

    [ContextMenu("Debug: Fill Everything")]
    public void FillEverything()
    {
        FillWarFunds();
        FillAllIngredients();
        Debug.Log("[DEBUG] War Funds and Ingredients filled!");
    }

        public void SaveIngredientsDebug()
    {
        GameSaveData gameSaveData = SaveStateManager.saveData;
        gameSaveData.savedInventory = PersistentInventoryManager.ToSaveData(PersistentInventoryManager.CurrentInventory);
        SaveStateManager.SaveGame(gameSaveData);
        Debug.Log($"[DEBUG] Saved ingredients to file");
    }
}
