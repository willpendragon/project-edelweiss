using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryDisplayHelper : MonoBehaviour

{
    [SerializeField] private List<string> _inventoryEntries;
    [SerializeField] private TextMeshProUGUI _inventoryText;

    void Start()
    {
        RefreshInventoryDisplay();
    }
    public void RefreshInventoryDisplay()
    {
        _inventoryEntries.Clear();
        foreach (var entry in PersistentInventoryManager.CurrentInventory.items)
        {
            string inventoryEntry = $"{entry.ingredient.name} x{entry.quantity}";
            _inventoryEntries.Add(inventoryEntry);
            string inventoryList = string.Join(",", _inventoryEntries);
            _inventoryText.text = inventoryList;
        }
        if (_inventoryEntries.Count == 0)
        {
            _inventoryText.text = "No Ingredients available";
        }
    }
}