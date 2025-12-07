using System.Collections.Generic;
using UnityEngine;

public class SaveBakedItemsHelper : MonoBehaviour
{
    public void SaveBakedItems()
    {
        var bakedItems = PersistentInventoryManager.CurrentInventory.bakedItems;
        List<BakedItemsData> bakedItemsData = new List<BakedItemsData>();

        foreach (var item in bakedItems)
        {
            bakedItemsData.Add(new BakedItemsData
            {
                pastryName = item.item.name,
                quantity = item.quantity
            });
        }

        SaveStateManager.saveData.bakedItems = bakedItemsData;
        SaveStateManager.SaveGame(SaveStateManager.saveData);
    }
}
