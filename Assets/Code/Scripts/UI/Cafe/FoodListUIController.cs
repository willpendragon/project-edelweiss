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
            // itemFoodObject.GetComponentInChildren<Image>().sprite = entry.item.foodIcon;
            itemFoodObject.GetComponent<ItemFoodIconHelper>().UpdatePastryIcon(entry.item.foodIcon);
            // I'm keeping this button reference just because I need to keep the flow intact (however the button is not enabled).
            Button itemFoodButton = itemFoodObject.GetComponentInChildren<Button>();

            EventTrigger trigger = itemFoodObject.gameObject.GetComponentInChildren<EventTrigger>();
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

            // Drag event
            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener((data) =>
            {
                PointerEventData pointerData = (PointerEventData)data;
                CafeMenuUIController.Instance.OnDrag(pointerData);
            });
            trigger.triggers.Add(dragEntry);


            //itemFoodButton.onClick.AddListener(() => OnItemClicked(entry.item, entry.item.itemFoodPrice));

            var itemFoodIconHelper = itemFoodObject.GetComponent<ItemFoodIconHelper>();
            itemFoodIconHelper.PopulateItemFoodDetails(
                entry.item.itemFoodName,
                $"{FoodTypeLabel(entry.item)} +{entry.item.recoveryAmount}",
                $"{entry.item.itemFoodPrice} <space=30><sprite=91>",
                $"x{entry.quantity} Available",
                entry.item.itemFoodDescription
            );

            // TextMeshProUGUI[] texts = itemFoodButton.GetComponentsInChildren<TextMeshProUGUI>();
            // if (texts.Length >= 5)
            // {
            //     texts[0].text = $"x{entry.quantity} Crafted Items Available";
            //     texts[1].text = $"{entry.item.itemFoodPrice} [COINS ICON]";
            //     texts[2].text = entry.item.itemFoodName;
            //     texts[3].text = $"{FoodTypeLabel(entry.item)} +{entry.item.recoveryAmount}";
            //     texts[4].text = entry.item.itemFoodDescription;
            // }
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
            foodTypeLabel = "MP Recovery";
            return foodTypeLabel;
        }
        else if (food.itemFoodType == ItemFoodType.FaithRecovery)
        {
            foodTypeLabel = "FP Recovery";
            return foodTypeLabel;
        }
        else
        {
            return null;
        }
    }
}
