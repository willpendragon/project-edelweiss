using TMPro;
using UnityEngine;

public class InventoryDisplayHelper : MonoBehaviour
{
    [SerializeField] private Transform _inventoryContentParent;
    [SerializeField] private TextMeshProUGUI _inventoryText;
    [SerializeField] private GameObject _ingredientPrefab; 

    void Start()
    {
        RefreshInventoryDisplay();
    }

    public void RefreshInventoryDisplay()
    {
        foreach (Transform child in _inventoryContentParent)
            Destroy(child.gameObject);

        var items = PersistentInventoryManager.CurrentInventory.items;

        if (_inventoryText != null)
            _inventoryText.gameObject.SetActive(items.Count == 0);

        if (items.Count == 0)
        {
            if (_inventoryText != null)
                _inventoryText.text = "No Ingredients available";
            return;
        }

        foreach (var entry in items)
        {
            CreateInventoryRow(entry);
        }
    }

    private void CreateInventoryRow(InventoryEntry entry)
    {
        GameObject row = Instantiate(_ingredientPrefab, _inventoryContentParent);
        row.name = $"Row_{entry.ingredient.ingredientName}";

        if (row.TryGetComponent<IngredientRowContainerHelper>(out var rowHelper))
        {
            string labelText = $"{entry.ingredient.ingredientName} x{entry.quantity}";
            rowHelper.UpdateIngredientDetails(entry.ingredient.ingredientIcon, labelText);
        }
    }
}