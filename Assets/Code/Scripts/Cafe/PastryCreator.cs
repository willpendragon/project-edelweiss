using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PastryCreator : MonoBehaviour
{
    [SerializeField] private List<Recipe> allRecipes;
    [SerializeField] private Inventory ingredientInventory;
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

        foreach (var recipe in allRecipes)
        {
            var canCraft = recipe.CanCraft(ingredientInventory);
            var uiElement = Instantiate(recipeUIPrefab, recipeListParent);
            var uiScript = uiElement.GetComponent<RecipeUI>();
            uiScript.Setup(recipe, canCraft);
            uiScript.SetCreator(this);
        }
    }

    public void TryCraft(Recipe recipe)
    {
        if (recipe.CanCraft(ingredientInventory))
        {
            ingredientInventory.ConsumeIngredients(recipe);
            Debug.Log($"Crafted {recipe.resultItem.itemFoodName}");
            RefreshUI();
        }
        else
        {
            Debug.Log("Not enough ingredients!");
        }
    }
}
