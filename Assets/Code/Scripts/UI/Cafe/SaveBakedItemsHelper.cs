using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBakedItemsHelper : MonoBehaviour
{
    [SerializeField] CafeMenuUIController _cafeMenuUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SaveBakedItems();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            LoadBakedItems();
        }
    }
    public void SaveBakedItems()
    {
        var bakedItems = _cafeMenuUI.bakedItems;
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

    public void LoadBakedItems()
    {
        SaveStateManager.LoadGame();
    }
}