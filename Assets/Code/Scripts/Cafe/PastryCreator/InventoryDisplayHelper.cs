using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplayHelper : MonoBehaviour

{
    [SerializeField] private Transform _inventoryContentParent; // Parent holding one generated row per ingredient
    [SerializeField] private TextMeshProUGUI _inventoryText; // Shown only when inventory is empty
    [SerializeField] private Vector2 _iconSize = new Vector2(32f, 32f);
    [SerializeField] private float _rowSpacing = 6f;

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
        GameObject row = new GameObject($"Row_{entry.ingredient.ingredientName}", typeof(RectTransform));
        row.transform.SetParent(_inventoryContentParent, false);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = _rowSpacing;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement rowLayoutElement = row.AddComponent<LayoutElement>();
        rowLayoutElement.preferredHeight = _iconSize.y;
        rowLayoutElement.minHeight = _iconSize.y;

        GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(row.transform, false);
        Image icon = iconGO.AddComponent<Image>();
        icon.sprite = entry.ingredient.ingredientIcon;
        icon.preserveAspect = true;
        iconGO.GetComponent<RectTransform>().sizeDelta = _iconSize;

        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(row.transform, false);
        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = $"{entry.ingredient.ingredientName} x{entry.quantity}";
        label.fontSize = _inventoryText != null ? _inventoryText.fontSize : 24f;
        label.color = _inventoryText != null ? _inventoryText.color : Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        labelGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, _iconSize.y);
    }
}