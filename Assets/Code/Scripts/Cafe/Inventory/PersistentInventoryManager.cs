using System.Collections.Generic;
using UnityEngine;

public class PersistentInventoryManager : MonoBehaviour
{
    public static Inventory CurrentInventory { get; private set; }

    [SerializeField] private Inventory inventoryAsset; // drag SO here
    [SerializeField] private List<Ingredient> allIngredientPrototypes; // assign all known ingredients in Inspector

    void Awake()
    {
        if (CurrentInventory == null)
        {
            CurrentInventory = Instantiate(inventoryAsset);
            Debug.Log("[PersistentInventoryManager] Instantiated new inventory. ID: " + CurrentInventory.GetInstanceID());

            FromSaveData(SaveStateManager.saveData.savedInventory, CurrentInventory, allIngredientPrototypes);
            Debug.Log("[PersistentInventoryManager] Loaded saved inventory items: " + CurrentInventory.items.Count);

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

}
