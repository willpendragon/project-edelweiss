using DG.Tweening;
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

    // Drag and Drop UI
    private RectTransform _draggedFoodRT;
    private GameObject _ghostFoodInstance;
    private GameObject _ghostItemFoodPrefab;
    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Vector2 _dragOffset;

    [SerializeField] private Canvas _cafeMenuCanvas;

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
        // Reload the Baked items. Allow to update the list in cases where the food was consumed outside the café.
        // (Namely in battle, offered as a tribute).
        PersistentInventoryManager.ReloadBakedItems();
        _foodListUIController.GenerateFoodList();
        _characterListUIController.GenerateCharacterList();
    }

    public void OnItemBeginDrag(GameObject itemFoodObject, ItemFood item, float price)
    {
        Debug.Log($"Dragging {itemFoodObject}, {item}");
        if (price > gameStatsManager.warFunds)
        {
            // Stop interaction, display "Not Enough War Funds" message.

            notificationTexts.text = "Not enough War Funds!";
            // Make notification disappear after a while (should generalize this)
            // Make the item shake.
            itemFoodObject.transform.DOShakePosition(0.35f, 20f, 20, 90f);
            return;
        }

        _draggedFoodRT = itemFoodObject.GetComponent<RectTransform>();
        // Attach the Food Object Image to the Pointer
        _originalParent = _draggedFoodRT.parent;
        _originalPosition = _draggedFoodRT.anchoredPosition;

        //// Instantiate a Ghost on the previously position of the selected food on the UI.
        //_ghostFoodInstance = Instantiate(_ghostItemFoodPrefab, _originalParent);
        //_ghostFoodInstance.transform.SetAsSiblingIndex(_draggedFoodRT.GetSiblingIndex());
        //_ghostFoodInstance.GetComponent<CanvasGroup>().DOFade(0.4f, 0.15f);

        _draggedFoodRT.SetParent(_cafeMenuCanvas.transform);
        _draggedFoodRT.DOScale(1.1f, 0.15f);

        // Store the selected item and its price
        selectedItem = item;
        selectedItemPrice = price;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedFoodRT == null)
            return;

        RectTransform canvasRT = _cafeMenuCanvas.transform as RectTransform;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            eventData.position,
            null,                  // IMPORTANT for Screen Space Overlay
            out pos
        );

        _draggedFoodRT.anchoredPosition = pos;
        _draggedFoodRT.anchorMin = new Vector2(0.5f, 0.5f);
        _draggedFoodRT.anchorMax = new Vector2(0.5f, 0.5f);
        _draggedFoodRT.pivot = new Vector2(0.5f, 0.5f);
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
            if (foundUnit == null)
            {
                _draggedFoodRT.DOAnchorPos(_originalPosition, 0.25f).SetEase(Ease.OutQuad);
                _draggedFoodRT.DOScale(1f, 0.2f).OnComplete(() =>
                {
                    _draggedFoodRT.SetParent(_originalParent);
                });

                selectedItem = null;
                // If there's nothing, just make the item return to its original place, and destroy the "ghost".

                return;
            }
            var unit = foundUnit.unit;
            Debug.Log($"Released Drag on {unit.unitTemplate.unitName}");

            if (FeedCharacter(ref itemFood, unit))
            {
                // Make food disappear on the position of the character
                Vector3 worldPos = unit.transform.position;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                Vector2 uiPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _cafeMenuCanvas.transform as RectTransform, screenPos, null, out uiPos);

                _draggedFoodRT.DOAnchorPos(uiPos, 0.25f).OnComplete(() =>
                {
                    _draggedFoodRT.DOScale(0f, 0.18f).OnComplete(() =>
                    {
                        Destroy(itemFoodObject);
                    });
                });

                // Spend War Funds
                ConfirmPurchase();

                // Update Persistent Inventory.
                PersistentInventoryManager.Instance.RemoveBakedItem(itemFood);

                // Save the change:
                _saveBakedItemsHelper.SaveBakedItems();

                // Refresh UI:
                _foodListUIController.GenerateFoodList();

                selectedItem = null;
                return;
                // Destroy the food ghost as well (?).
            }
            else
            {
                CancelEating(_originalPosition);
                Debug.Log("Handle cases where the character unit doesn't eat");
            }
        }
        else
        {
            Debug.Log("Handle dropping the food outside of a character");
        }
    }

    private void CancelEating(Vector3 originalPosition)
    {
        if (_draggedFoodRT == null)
            return;

        _draggedFoodRT.DOShakeAnchorPos(
            duration: 0.25f,
            strength: 20f,
            vibrato: 20,
            randomness: 90f,
            snapping: false,
            fadeOut: true
        )
        .OnComplete(() =>
        {
            // Return to original UI position smoothly
            _draggedFoodRT.DOAnchorPos(originalPosition, 0.25f)
                .SetEase(Ease.OutQuad);

            // Reset scale
            _draggedFoodRT.DOScale(1f, 0.2f).OnComplete(() =>
            {
                // Restore hierarchy
                _draggedFoodRT.SetParent(_originalParent);

                // Clear selection
                selectedItem = null;
            });
        });
    }

    public void ConfirmPurchase()
    {
        // Warning: this logic should tie in the new drag and drop system!
        // Add new name for this method.
        if (gameStatsManager.warFunds >= selectedItemPrice)
        {
            // Deduct funds and update display
            gameStatsManager.warFunds -= selectedItemPrice;
            UpdateWarFundsCounter();

            // Show purchase notification
            notificationTexts.text = $"{selectedItem.itemFoodName} purchased!";
        }
    }
    // UI-only logic
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
    //public void CancelPurchase()
    //{
    //    // Reset selected item and hide the popup
    //    selectedItem = null;
    //    selectedItemPrice = 0;
    //    confirmPurchasePopup.SetActive(false);
    //}

    //public void PurchaseFood(ItemFood purchasedFood, float foodPrice)
    //{
    //    if (foodPrice <= gameStatsManager.warFunds)
    //    {
    //        gameStatsManager.warFunds -= foodPrice;
    //        gameStatsManager.SaveSpentWarFunds(foodPrice);
    //        UpdateWarFundsCounter();
    //        currentPurchasedFood = purchasedFood;
    //    }
    //    else
    //    {
    //        notificationTexts.text = "There are not enough War Funds to purchase this Food Item";
    //    }
    //}
    public void UpdateWarFundsCounter()
    {
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
    }

    public void SelectFoodItemForFeeding(FoodShelfItem foodItem)
    {
        //selectedFoodItem = foodItem;
        //notificationTexts.text = $"Selected {foodItem.item.itemFoodName} for feeding. Choose a character.";
    }

    public bool FeedCharacter(ref ItemFood foodItem, Unit fedUnit)
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
            gameStatsManager.SaveSpentWarFunds(selectedItemPrice);
            _pastrySlotUIController.CreatePastrySlotsPanel(fedUnit, foodItem);
            bool characterWasFed = true;
            return characterWasFed;
        }
        else
        {
            return false;
        }

    }

    public void HandleNotifications(string message)
    {
        notificationTexts.text = message;
    }
}