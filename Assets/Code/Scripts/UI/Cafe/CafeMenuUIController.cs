using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Inventory;

public class CafeMenuUIController : MonoBehaviour
{
    public static CafeMenuUIController Instance { get; private set; }

    public GameObject confirmPurchasePopup;          // Popup for purchase confirmation
    public TextMeshProUGUI confirmationText;         // Text displaying the item name and price in the popup
    public Transform foodShelfContainer;             // Container where items will be displayed after purchase
    public GameObject foodShelfItemPrefab;           // Prefab for displaying each item on the Food Shelf

    [SerializeField] private ItemFood selectedItem;                   // Currently selected item for purchase
    private float selectedItemPrice;                 // Price of the selected item

    public List<FoodInventoryEntry> bakedItems;
    [SerializeField] SaveBakedItemsHelper _saveBakedItemsHelper;

    public TextMeshProUGUI warFundsCounter;
    public GameStatsManager gameStatsManager;

    public ItemFood currentPurchasedFood;

    [SerializeField] TextMeshProUGUI notificationTexts;
    [SerializeField] GameObject loveIconPrefab;
    [SerializeField] Transform loveIconPrefabTransform;
    [SerializeField] private FoodListUIController _foodListUIController;
    [SerializeField] private CharacterListUIController _characterListUIController;
    [SerializeField] private CafeSaveManager _cafeSaveManager;
    [SerializeField] private PastrySlotController _pastrySlotController;

    private FoodShelfItem selectedFoodItem;  // The currently selected food item

    public FoodListUIController FoodListUIController => _foodListUIController;
    public PastrySlotController PastrySlotController => _pastrySlotController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of CafeMenuUIController detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        gameStatsManager = GameObject.FindWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
        _foodListUIController.GenerateFoodList();
        _characterListUIController.GenerateCharacterList();
    }

    public void OnItemBeginDrag(GameObject itemFoodObject, ItemFood item, float price)
    {
        // Attach the Food Object Image to the Pointer
        // ...
        Debug.Log($"Dragging {itemFoodObject}, {item}");
        if (price > gameStatsManager.warFunds)
        {
            // Stop interaction, display "Not Enough War Funds" message.
            notificationTexts.text = "Not enough War Funds!";
            return;
        }

        // Store the selected item and its price
        selectedItem = item;
        selectedItemPrice = price;

        // Update the confirmation popup text and display the popup
        //confirmationText.text = $"Buy {item.itemFoodName} for {price} War Funds?";
        //confirmPurchasePopup.SetActive(true);
    }
    public void OnItemDropped(GameObject itemFoodObject, ItemFood itemFood, float itemFoodPrice, PointerEventData pointerData)
    {
        if (selectedItem == null)
            return;
        // Check if the Item is being dropped on a Character (?)
        // ...
        Ray ray = Camera.main.ScreenPointToRay(pointerData.position);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, LayerMask.GetMask("NoPixelation"))) // Should be a dedicated layer for Units.
        {
            CafeUnitHelper foundUnit = raycastHit.collider.gameObject.GetComponent<CafeUnitHelper>();
            if (foundUnit != null)
            {
                var unit = foundUnit.unit;
                Debug.Log($"Released Drag on {unit.unitTemplate.unitName}");
                FeedCharacter(ref itemFood, unit);
            }
        }
    }
    public void ConfirmPurchase()
    {
        if (gameStatsManager.warFunds >= selectedItemPrice)
        {
            // Deduct funds and update display
            gameStatsManager.warFunds -= selectedItemPrice;
            UpdateWarFundsCounter();

            // Add the item to the Food Shelf
            //AddItemToFoodShelf(selectedItem);

            // Show purchase notification
            notificationTexts.text = $"{selectedItem.itemFoodName} purchased!";

            // Remove baked item and update the food list
            RemoveBakedItem(selectedItem);
            _saveBakedItemsHelper.SaveBakedItems();

            // Save the War Funds amount after spending.
            gameStatsManager.SaveSpentWarFunds(selectedItemPrice);
            _foodListUIController.GenerateFoodList();
        }
        else
        {
            notificationTexts.text = "Not enough War Funds!";
        }

        // Reset selected item and hide the popup
        selectedItem = null;
        selectedItemPrice = 0;
        confirmPurchasePopup.SetActive(false);  // Hide the popup
    }

    public void RemoveBakedItem(ItemFood item, int amount = 1)
    {
        for (int i = 0; i < bakedItems.Count; i++)
        {
            if (bakedItems[i].item == item)
            {
                int newQty = bakedItems[i].quantity - amount;
                if (newQty <= 0)
                {
                    bakedItems.RemoveAt(i);
                }
                else
                {
                    bakedItems[i] = new FoodInventoryEntry
                    {
                        item = item,
                        quantity = newQty
                    };
                }
                return;
            }
        }
    }
    public void CancelPurchase()
    {
        // Reset selected item and hide the popup
        selectedItem = null;
        selectedItemPrice = 0;
        confirmPurchasePopup.SetActive(false);
    }

    public void PurchaseFood(ItemFood purchasedFood, float foodPrice)
    {
        if (foodPrice <= gameStatsManager.warFunds)
        {
            gameStatsManager.warFunds -= foodPrice;
            gameStatsManager.SaveSpentWarFunds(foodPrice);
            UpdateWarFundsCounter();
            currentPurchasedFood = purchasedFood;
            //EnableFeedingCharactersButtons();
        }
        else
        {
            notificationTexts.text = "There are not enough War Funds to purchase this Food Item";
        }
    }
    public void UpdateWarFundsCounter()
    {
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
    }

    public void SelectFoodItemForFeeding(FoodShelfItem foodItem)
    {
        selectedFoodItem = foodItem;
        notificationTexts.text = $"Selected {foodItem.item.itemFoodName} for feeding. Choose a character.";
    }

    public bool FeedCharacter(ref ItemFood foodItem, Unit fedUnit)
    {
        // Destroy existing Pastry Slots panels.

        // Instantiate Pastry Slots Panel for the corresponding Unit.
        //GameObject newPastrySlotsPanel = Instantiate(_pastrySlotsObject, _pastrySlotsCanvas.transform);

        // Fill the Pastry Slots Panel with the eaten pastries history for that character.

        if (fedUnit.unitFoodSlots == fedUnit.unitTemplate.unitMaxFoodSlots)
        {
            notificationTexts.text = $"{fedUnit.unitTemplate.unitName} is not hungry!";
            return false;
        }

        bool itemUsed = false;

        if (foodItem.itemFoodType == ItemFoodType.HPRecovery)
        {
            if (fedUnit.unitHealthPoints < fedUnit.unitMaxHealthPoints)
            {
                fedUnit.unitHealthPoints += foodItem.recoveryAmount;
                if (fedUnit.unitHealthPoints > fedUnit.unitMaxHealthPoints)
                {
                    fedUnit.unitHealthPoints = fedUnit.unitMaxHealthPoints;
                }
                itemUsed = true;
            }
        }
        else if (foodItem.itemFoodType == ItemFoodType.ManaRecovery)
        {
            if (fedUnit.unitManaPoints < fedUnit.unitMaxManaPoints)
            {
                fedUnit.unitManaPoints += foodItem.recoveryAmount;
                if (fedUnit.unitManaPoints > fedUnit.unitMaxManaPoints)
                {
                    fedUnit.unitManaPoints = fedUnit.unitMaxManaPoints;
                }
                itemUsed = true;
            }
        }

        else if (foodItem.itemFoodType == ItemFoodType.FaithRecovery)
        {
            if (fedUnit.unitFaithPoints >= 0)
            {
                fedUnit.unitFaithPoints += (int)foodItem.recoveryAmount;
                itemUsed = true;
            }
        }
        if (itemUsed)
        {
            notificationTexts.text = $"{fedUnit.unitTemplate.unitName} recovered {foodItem.recoveryAmount} {(foodItem.itemFoodType == ItemFoodType.HPRecovery ? "HP" : "MP")}!";
            _characterListUIController.UpdateCharacterStatsCounter(fedUnit);
            _cafeSaveManager.SaveRestoredCharacterStats();
            GameObject loveIconPrefabInstance = Instantiate(loveIconPrefab, loveIconPrefabTransform);
            Destroy(loveIconPrefabInstance, 1);
            // Fill one food slot.
            fedUnit.unitFoodSlots += 1;
            // Spend War Funds and Update Counter
            gameStatsManager.warFunds -= selectedItemPrice;
            UpdateWarFundsCounter();

            // Add the food to the list of eaten Pastry for the corresponding character.
            _pastrySlotController.TrackEatenFood(fedUnit, foodItem);
            // Update the Pastry Slots for the corresponding Character.
            //newPastrySlotsPanel.GetComponent<PastrySlotsPanelHelper>().UpdatePastrySlots();
            // Save the list of eaten Pastry.
        }
        else
        {
            notificationTexts.text = $"{fedUnit.unitTemplate.unitName} is already at full {(foodItem.itemFoodType == ItemFoodType.HPRecovery ? "HP" : "MP")}!";
        }

        return itemUsed; // Return whether the item was used successfully
    }

    IEnumerator ClearNotificationText(GameObject currentEmoticon)
    {
        float clearNotificationWaitingTime = 1.5f;
        yield return new WaitForSeconds(clearNotificationWaitingTime);
        notificationTexts.text = "";
    }
    //private void AddItemToFoodShelf(ItemFood item)
    //{
    //    // Check if the item is already on the Food Shelf
    //    foreach (Transform child in foodShelfContainer)
    //    {
    //        FoodShelfItem shelfItem = child.GetComponent<FoodShelfItem>();
    //        if (shelfItem != null && shelfItem.item == item)
    //        {
    //            // If the item already exists, increase the quantity
    //            shelfItem.IncreaseQuantity();
    //            return;
    //        }
    //    }
    //    // If item is not already on the shelf, create a new shelf item
    //    GameObject foodShelfItem = Instantiate(foodShelfItemPrefab, foodShelfContainer);
    //    FoodShelfItem shelfItemComponent = foodShelfItem.GetComponent<FoodShelfItem>();

    //    // Set item details using ScriptableObject data
    //    shelfItemComponent.SetItem(item);
    //}
}