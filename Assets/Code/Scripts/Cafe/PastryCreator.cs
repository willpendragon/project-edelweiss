using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PastryCreator : MonoBehaviour
{
    [SerializeField] private List<Recipe> allRecipes;
    [SerializeField] private Transform recipeListParent;
    [SerializeField] private GameObject recipeUIPrefab;

    void Start()
    {
        RefreshUI();
    }
    public void RefreshUI()
    {
        // Clear existing UI
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        var inventory = PersistentInventoryManager.CurrentInventory;

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
            Debug.Log("Crafted: " + recipe.resultItem.itemFoodName);
        }
        else
        {
            Debug.Log("Not enough ingredients!");
        }
    }
}