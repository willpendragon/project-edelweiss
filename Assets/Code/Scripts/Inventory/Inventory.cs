using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Game/Inventory", order = 10)]
public class Inventory : ScriptableObject
{
    public List<InventoryEntry> items = new List<InventoryEntry>();
    [SerializeField] private List<FoodInventoryEntry> bakedItems = new List<FoodInventoryEntry>();

    [System.Serializable]
    public struct FoodInventoryEntry
    {
        public ItemFood item;
        public int quantity;
    }

    public void Add(Ingredient ingredient, int amount = 1)
    {
        var entry = items.Find(e => e.ingredient == ingredient);
        if (entry.ingredient != null)
        {
            entry.quantity += amount;
            ReplaceEntry(entry);
        }
        else
        {
            items.Add(new InventoryEntry { ingredient = ingredient, quantity = amount });
        }
    }
    public bool HasIngredient(Ingredient ingredient, int requiredAmount = 1)
    {
        var entry = items.Find(e => e.ingredient == ingredient);
        return entry.ingredient != null && entry.quantity >= requiredAmount;
    }

    public void Remove(Ingredient ingredient, int amount = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ingredient == ingredient)
            {
                items[i] = new InventoryEntry
                {
                    ingredient = ingredient,
                    quantity = Mathf.Max(0, items[i].quantity - amount)
                };
                if (items[i].quantity == 0)
                    items.RemoveAt(i);
                break;
            }
        }
    }

    private void ReplaceEntry(InventoryEntry updatedEntry)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ingredient == updatedEntry.ingredient)
            {
                items[i] = updatedEntry;
                return;
            }
        }
    }

    public void ConsumeIngredients(Recipe recipe)
    {
        foreach (var entry in recipe.ingredients)
        {
            RemoveIngredient(entry.ingredient, entry.quantity);
        }
    }

    private void RemoveIngredient(Ingredient ingredient, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ingredient == ingredient)
            {
                int newQty = Mathf.Max(0, items[i].quantity - amount);
                if (newQty == 0)
                    items.RemoveAt(i);
                else
                    items[i] = new InventoryEntry { ingredient = ingredient, quantity = newQty };
                return;
            }
        }
    }

    public void AddBakedItem(ItemFood food, int amount = 1)
    {
        for (int i = 0; i < bakedItems.Count; i++)
        {
            if (bakedItems[i].item == food)
            {
                bakedItems[i] = new FoodInventoryEntry
                {
                    item = food,
                    quantity = bakedItems[i].quantity + amount
                };
                return;
            }
        }

        bakedItems.Add(new FoodInventoryEntry { item = food, quantity = amount });
    }

    public int GetBakedItemQuantity(ItemFood food)
    {
        foreach (var entry in bakedItems)
        {
            if (entry.item == food)
                return entry.quantity;
        }
        return 0;
    }

    public List<FoodInventoryEntry> GetAllBakedItems()
    {
        return bakedItems;
    }
}
