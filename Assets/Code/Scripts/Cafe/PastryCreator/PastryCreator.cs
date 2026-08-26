using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ProjectEdelweiss.Utils;
using System;

public class PastryCreator : MonoBehaviour
{
    [SerializeField] private List<Recipe> allRecipes;
    [SerializeField] private Transform recipeListParent;
    [SerializeField] private GameObject recipeUIPrefab;
    [SerializeField] InventoryDisplayHelper _inventoryDisplayHelper;
    [SerializeField] private PastryFeedbackController _pastryFeedbackController;
    [SerializeField] CafeMenuUIController _cafeMenuiUIController;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PersistentInventoryManager.CurrentInventory != null);

        var allIngredientPrototypes = PersistentInventoryManager.Instance.AllIngredientPrototypes;

        // Debug allIngredientPrototypes
        Debug.Log($"[PastryCreator] Ingredient prototypes loaded: {allIngredientPrototypes.Count}");
        foreach (var i in allIngredientPrototypes)
        {
            Debug.Log($"[PastryCreator] Prototype: {i.ingredientName}");
        }

        PersistentInventoryManager.ReloadInventory(allIngredientPrototypes);

        Debug.Log("[PastryCreator] Inventory reloaded, now refreshing UI...");
        Debug.Log($"[PastryCreator] Inventory now contains: {PersistentInventoryManager.CurrentInventory.items.Count} items");

        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        var inventory = PersistentInventoryManager.CurrentInventory;

        Debug.Log($"[PastryCreator] Refreshing UI items in inventory: {inventory.items.Count}");

        foreach (var recipe in allRecipes)
        {
            var canCraft = recipe.CanCraft(inventory);
            var uiElement = Instantiate(recipeUIPrefab, recipeListParent);
            var uiScript = uiElement.GetComponent<RecipeUI>();
            uiScript.Setup(recipe, canCraft);
            uiScript.SetCreator(this);
        }
    }

    public void TryCraft(Recipe recipe)
    {
        var inventory = PersistentInventoryManager.CurrentInventory;

        if (recipe.CanCraft(inventory))
        {
            inventory.ConsumeIngredients(recipe);
            inventory.AddBakedItem(recipe.resultItem);
            _pastryFeedbackController.CookingFeedback(recipe);
            Debug.Log("Crafted: " + recipe.resultItem.itemFoodName);
            RefreshUI(); // Refresh after crafting

            // Save the ingredients count
            GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
            gameStatsManager.SaveIngredientsAfterBaking();
            _inventoryDisplayHelper.RefreshInventoryDisplay();

            // Add the baked item to the Cafe Menu UI list of baked items.
            _cafeMenuiUIController.bakedItems.Add(new Inventory.FoodInventoryEntry
            {
                item = recipe.resultItem,
                quantity = 1
            }
            );
            // Save the list of baked items using SaveBakedItemsHelper.
            _cafeMenuiUIController.SaveBakedItemsHelper.SaveBakedItems();
        }
        else
        {
            Debug.Log("Not enough ingredients!");
        }
    }
}
