using System;
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

    [SerializeField] private ItemFood selectedItem;  // Currently selected item for purchase
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

    [SerializeField] private PastrySlotUIController _pastrySlotUIController;
    [SerializeField] private FeedingController _feedingController;

    [SerializeField] private CafeSaveManager _cafeSaveManager;


    private FoodShelfItem selectedFoodItem;  // The currently selected food item

    public FoodListUIController FoodListUIController => _foodListUIController;
    public PastrySlotUIController PastrySlotController => _pastrySlotUIController;

    public SaveBakedItemsHelper SaveBakedItemsHelper => _saveBakedItemsHelper;

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

    public void FeedCharacter(ref ItemFood foodItem, Unit fedUnit)
    {
        if (_feedingController.HandleFeeding(foodItem, fedUnit))
        {
            // This sequence triggers only when the character was actually fed.
            _pastrySlotUIController.DestroyExistingSlotsPanel();
            _characterListUIController.UpdateCharacterStatsCounter(fedUnit);
            _cafeSaveManager.SaveRestoredCharacterStats();
            // Move "love" feedback in another class
            GameObject loveIconPrefabInstance = Instantiate(loveIconPrefab, loveIconPrefabTransform);
            Destroy(loveIconPrefabInstance, 1);
            // Fill one food slot. Should use an helper class on the Unit.
            fedUnit.unitFoodSlots += 1;
            // Spend War Funds and Update Counter. Should use a dedicated class for spending.
            gameStatsManager.warFunds -= selectedItemPrice;
            UpdateWarFundsCounter();

            _pastrySlotUIController.CreatePastrySlotsPanel(fedUnit, foodItem);
        }
    }

    public void HandleNotifications(string message)
    {
        notificationTexts.text = message;
    }
}