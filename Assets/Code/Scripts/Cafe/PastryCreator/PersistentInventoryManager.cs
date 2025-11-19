using System.Collections.Generic;
using UnityEngine;
using static Inventory;

public class PersistentInventoryManager : MonoBehaviour
{
    public static Inventory CurrentInventory { get; private set; }

    [SerializeField] private Inventory inventoryAsset; // Drag Inventory SO.
    [SerializeField] private List<Ingredient> allIngredientPrototypes; // Assign all known ingredients in the Inspector.
    [SerializeField] private List<ItemFood> allBakedItemPrototypes; // Assign all known baked items in the Inspector.


    void Awake()
    {
        if (CurrentInventory == null)
        {
            CurrentInventory = Instantiate(inventoryAsset);
            Debug.Log("[PersistentInventoryManager] Instantiated new inventory. ID: " + CurrentInventory.GetInstanceID());

            FromSaveData(SaveStateManager.saveData.savedInventory, CurrentInventory, allIngredientPrototypes);
            Debug.Log("[PersistentInventoryManager] Loaded saved inventory items: " + CurrentInventory.items.Count);

            FromSavedBakedItems(SaveStateManager.saveData.bakedItems, CurrentInventory, allBakedItemPrototypes);


            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static List<IngredientSaveEntry> ToSaveData(Inventory inventory)
    {
        var data = new List<IngredientSaveEntry>();
        foreach (var item in inventory.items)
        {
            data.Add(new IngredientSaveEntry
            {
                ingredientName = item.ingredient.name,
                quantity = item.quantity
            });
        }
        return data;
    }

    public static void FromSaveData(List<IngredientSaveEntry> savedItems, Inventory inventory, List<Ingredient> allIngredients)
    {
        inventory.items.Clear();

        foreach (var saved in savedItems)
        {
            Ingredient match = allIngredients.Find(i => i.name == saved.ingredientName);
            if (match != null)
            {
                inventory.Add(match, saved.quantity);
                Debug.Log($"[Inventory Load] {match.name} x{saved.quantity}");
            }
            else
            {
                Debug.LogWarning($"[Inventory Load] Ingredient not found: {saved.ingredientName}");
            }
        }
    }

    public static void ReloadInventory(List<Ingredient> allIngredients)
    {
        FromSaveData(SaveStateManager.saveData.savedInventory, CurrentInventory, allIngredients);
        Debug.Log("[Inventory] Reloaded from save file after scene change.");
    }

    public static void FromSavedBakedItems(List<BakedItemsData> savedBakedItems, Inventory inventory, List<ItemFood> allBakedItemPrototypes)
    {
        foreach (var saved in savedBakedItems)
        {
            if (string.IsNullOrEmpty(saved.pastryName) || saved.quantity <= 0)
                continue;

            ItemFood match = allBakedItemPrototypes.Find(p => p.name == saved.pastryName);
            if (match != null)
            {
                inventory.AddBakedItem(match, saved.quantity);
                Debug.Log($"[Load Baked Items] Loaded {match.name} x{saved.quantity}");
            }
            else
            {
                Debug.LogWarning($"[Load Baked Items] Missing prototype for: {saved.pastryName}");
            }
        }
    }


}
