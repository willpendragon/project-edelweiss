using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static Inventory;

public class FoodListUIController : MonoBehaviour
{
    public GameObject foodItemsContainer;
    public List<FoodInventoryEntry> bakedItems;
    public GameObject itemFoodPrefab;

    public void GenerateFoodList()
    {
        foreach (Transform child in foodItemsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        bakedItems = PersistentInventoryManager.CurrentInventory.GetAllBakedItems();

        foreach (var entry in bakedItems)
        {
            if (entry.quantity <= 0)
                continue;

            GameObject itemFoodObject = Instantiate(itemFoodPrefab, foodItemsContainer.transform);
            itemFoodObject.GetComponent<Image>().sprite = entry.item.foodIcon;
            Button itemFoodButton = itemFoodObject.GetComponentInChildren<Button>();

            EventTrigger trigger = itemFoodObject.gameObject.GetComponent<EventTrigger>();
            // Add Require Component to Food Item GameObject.
            // Using Dragging instead of clicking
            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener((data) =>
            {
                CafeMenuUIController.Instance.OnItemBeginDrag(itemFoodObject, entry.item, entry.item.itemFoodPrice);
            });
            trigger.triggers.Add(beginDragEntry);
            // Release Drag event
            EventTrigger.Entry releaseDragEntry = new EventTrigger.Entry();
            releaseDragEntry.eventID = EventTriggerType.EndDrag;
            releaseDragEntry.callback.AddListener((data) =>
            {
                PointerEventData pointerData = (PointerEventData)data;
                CafeMenuUIController.Instance.OnItemDropped(itemFoodObject, entry.item, entry.item.itemFoodPrice, pointerData);
            });
            trigger.triggers.Add(releaseDragEntry);

            //itemFoodButton.onClick.AddListener(() => OnItemClicked(entry.item, entry.item.itemFoodPrice));

            TextMeshProUGUI[] texts = itemFoodButton.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 6)
            {
                texts[0].text = $"x{entry.quantity}";
                texts[1].text = entry.item.itemFoodPrice.ToString();
                texts[2].text = entry.item.itemFoodName;
                texts[3].text = FoodTypeLabel(entry.item);
                texts[4].text = entry.item.recoveryAmount.ToString();
                texts[5].text = entry.item.itemFoodDescription;
            }
        }
    }

    public string FoodTypeLabel(ItemFood food)
    {
        string foodTypeLabel;
        if (food.itemFoodType == ItemFoodType.HPRecovery)
        {
            foodTypeLabel = "HP Recovery";
            return foodTypeLabel;
        }
        else if (food.itemFoodType == ItemFoodType.ManaRecovery)
        {
            foodTypeLabel = "Mana Recovery";
            return foodTypeLabel;
        }
        else if (food.itemFoodType == ItemFoodType.FaithRecovery)
        {
            foodTypeLabel = "Faith Recovery";
            return foodTypeLabel;
        }
        else
        {
            return null;
        }
    }
}
