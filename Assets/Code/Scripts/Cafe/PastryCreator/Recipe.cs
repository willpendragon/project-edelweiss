using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Recipe", menuName = "Items/Recipe", order = 2)]
public class Recipe : ScriptableObject
{
    [System.Serializable]
    public class IngredientEntry
    {
        public Ingredient ingredient;
        public int quantity = 1;
    }

    public string recipeName;
    public string recipeDescription;

    public List<IngredientEntry> ingredients = new List<IngredientEntry>();
    public ItemFood resultItem;

    public bool IsUnlocked = false;
    public bool CanCraft(Inventory inventory)
    {
        bool allMet = true;

        foreach (var requirement in ingredients)
        {
            bool hasEnough = inventory.HasIngredient(requirement.ingredient, requirement.quantity);
            Debug.Log($"[CanCraft] Checking: {requirement.ingredient.name}, required: {requirement.quantity}, has: {hasEnough}");

            if (!hasEnough)
                allMet = false;
        }

        return allMet;
    }
}
