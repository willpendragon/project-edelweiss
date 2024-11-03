using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodShelfItem : MonoBehaviour
{
    public ItemFood item;                 // Reference to the ScriptableObject for food
    public TextMeshProUGUI itemNameText;  // UI text component for item name
    public TextMeshProUGUI quantityText;  // UI text component for quantity
    public Image itemIcon;                // UI image component for item icon

    private int quantity = 1;             // Default quantity when an item is first added

    public void SetItem(ItemFood newItem)
    {
        item = newItem;
        itemNameText.text = item.itemFoodName;
        itemIcon.sprite = item.foodIcon;
        UpdateQuantityText();
    }

    public void IncreaseQuantity()
    {
        quantity++;
        UpdateQuantityText();
    }

    private void UpdateQuantityText()
    {
        quantityText.text = $"x{quantity}";
    }

    // Method called when this item is clicked
    public void OnItemClicked()
    {
        CafeMenuUIController.Instance.SelectFoodItemForFeeding(this);
    }

    // Method to decrease quantity when used and return true if quantity is above zero
    public bool UseItem()
    {
        quantity--;
        UpdateQuantityText();

        if (quantity <= 0)
        {
            Destroy(gameObject);  // Remove from UI if quantity reaches zero
            return false;
        }
        return true;
    }
}
