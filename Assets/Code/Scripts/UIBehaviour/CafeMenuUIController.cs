using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CafeMenuUIController : MonoBehaviour
{
    public static CafeMenuUIController Instance { get; private set; }

    public GameObject confirmPurchasePopup;          // Popup for purchase confirmation
    public TextMeshProUGUI confirmationText;         // Text displaying the item name and price in the popup
    public Transform foodShelfContainer;             // Container where items will be displayed after purchase
    public GameObject foodShelfItemPrefab;           // Prefab for displaying each item on the Food Shelf

    private ItemFood selectedItem;                   // Currently selected item for purchase
    private float selectedItemPrice;                 // Price of the selected item

    public GameObject foodItemsContainer;
    public GameObject characterProfilesContainer;

    public List<ItemFood> itemFoodList;
    public GameObject itemFoodPrefab;
    public GameObject characterProfilesPrefab;

    public TextMeshProUGUI warFundsCounter;
    public GameStatsManager gameStatsManager;

    public ItemFood currentPurchasedFood;

    public List<GameObject> characterProfileSmallControllers = new List<GameObject>();
    public List<Button> feedPlayerCharactersButtons = new List<Button>();

    private TextMeshProUGUI[] characterTexts;

    [SerializeField] TextMeshProUGUI notificationTexts;
    [SerializeField] GameObject loveIconPrefab;
    [SerializeField] Transform loveIconPrefabTransform;

    private FoodShelfItem selectedFoodItem;  // The currently selected food item

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
        GenerateFoodList();
        GenerateCharacterList();
    }

    public void OnItemClicked(ItemFood item, float price)
    {
        // Store the selected item and its price
        selectedItem = item;
        selectedItemPrice = price;

        // Update the confirmation popup text and display the popup
        confirmationText.text = $"Buy {item.itemFoodName} for {price} War Funds?";
        confirmPurchasePopup.SetActive(true);
    }

    public void ConfirmPurchase()
    {
        if (gameStatsManager.warFunds >= selectedItemPrice)
        {
            // Deduct funds and update display
            gameStatsManager.warFunds -= selectedItemPrice;
            UpdateWarFundsCounter();

            // Add the item to the Food Shelf
            AddItemToFoodShelf(selectedItem);

            // Show purchase notification
            notificationTexts.text = $"{selectedItem.itemFoodName} purchased!";
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

    public void CancelPurchase()
    {
        // Reset selected item and hide the popup
        selectedItem = null;
        selectedItemPrice = 0;
        confirmPurchasePopup.SetActive(false);
    }
    void GenerateFoodList()
    {
        foreach (var food in itemFoodList)
        {
            GameObject foodItem = Instantiate(itemFoodPrefab, foodItemsContainer.transform);
            foodItem.GetComponent<Image>().sprite = food.foodIcon;
            Button itemFoodButton = foodItem.GetComponentInChildren<Button>();

            // Update to open the confirmation popup instead of purchasing directly
            itemFoodButton.onClick.AddListener(() => OnItemClicked(food, food.itemFoodPrice));

            // Get all TextMeshPro components in children
            TextMeshProUGUI[] texts = itemFoodButton.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 6)
            {
                texts[1].text = food.itemFoodPrice.ToString();
                texts[2].text = food.itemFoodName;
                texts[3].text = FoodTypeLabel(food);
                texts[4].text = food.recoveryAmount.ToString();
                texts[5].text = food.itemFoodDescription;
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
    public void PurchaseFood(ItemFood purchasedFood, float foodPrice)
    {
        if (foodPrice <= gameStatsManager.warFunds)
        {
            gameStatsManager.warFunds -= foodPrice;
            gameStatsManager.SaveSpentWarFunds(foodPrice);
            UpdateWarFundsCounter();
            currentPurchasedFood = purchasedFood;
            EnableFeedingCharactersButtons();
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
    public void GenerateCharacterList()
    {
        // Clear the list to ensure it doesn’t contain outdated references
        characterProfileSmallControllers.Clear();

        foreach (var partyMember in GameManager.Instance.playerPartyMembersInstances)
        {
            if (partyMember.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                GameObject characterProfile = Instantiate(characterProfilesPrefab, characterProfilesContainer.transform);

                // Set up the CharacterProfileSmallController reference and other details
                CharacterProfileSmallController profileController = characterProfile.GetComponent<CharacterProfileSmallController>();
                profileController.referenceUnit = partyMember;

                // Assign the character portrait and stats to the UI
                characterProfile.GetComponentInChildren<Image>().sprite = partyMember.GetComponent<Unit>().unitTemplate.unitPortrait;

                TextMeshProUGUI[] characterTexts = characterProfile.GetComponentsInChildren<TextMeshProUGUI>();

                if (characterTexts.Length >= 5)
                {
                    characterTexts[0].text = partyMember.unitTemplate.unitName;
                    characterTexts[1].text = "HP";
                    characterTexts[2].text = partyMember.unitHealthPoints.ToString();
                    characterTexts[3].text = "/";
                    characterTexts[4].text = partyMember.unitMaxHealthPoints.ToString();
                    characterTexts[5].text = "MP";
                    characterTexts[6].text = partyMember.unitManaPoints.ToString();
                    characterTexts[7].text = "/";
                    characterTexts[8].text = partyMember.unitMaxManaPoints.ToString();
                }

                // Add the profile to the list of small controllers
                characterProfileSmallControllers.Add(characterProfile);

                // Create and configure the feed button for this character
                GameObject feedCharacterButtonGO = new GameObject("CharacterFeedButton");
                RectTransform rectTransform = feedCharacterButtonGO.AddComponent<RectTransform>();
                rectTransform.SetParent(characterProfile.transform, false);
                rectTransform.sizeDelta = new Vector2(128, 32);

                Image buttonImage = feedCharacterButtonGO.AddComponent<Image>();
                Button feedCharacterButton = feedCharacterButtonGO.AddComponent<Button>();

                GameObject textGO = new GameObject("ButtonText");
                RectTransform textRectTransform = textGO.AddComponent<RectTransform>();
                textRectTransform.SetParent(feedCharacterButtonGO.transform, false);
                textRectTransform.anchorMin = Vector2.zero;
                textRectTransform.anchorMax = Vector2.one;
                textRectTransform.sizeDelta = Vector2.zero;

                TextMeshProUGUI textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
                textMeshPro.text = "Feed";
                textMeshPro.fontSize = 24;
                textMeshPro.alignment = TextAlignmentOptions.Center;
                textMeshPro.color = Color.black;

                ColorBlock colors = feedCharacterButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1);
                colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1);
                feedCharacterButton.colors = colors;

                // Add the onClick listener for feeding the character
                Unit characterUnit = profileController.referenceUnit;  // Capture the reference to avoid closure issues
                feedCharacterButton.onClick.AddListener(() => OnCharacterClicked(characterUnit));

                feedCharacterButton.enabled = false;  // Disable initially; can be enabled when an item is selected
                feedPlayerCharactersButtons.Add(feedCharacterButton);
            }
        }
    }
    public void OnCharacterClicked(Unit character)
    {
        if (selectedFoodItem == null)
        {
            notificationTexts.text = "Please select a food item first.";
            return;
        }

        // Feed the character with the selected food item
        bool itemUsed = FeedCharacter(ref selectedFoodItem.item, character);

        // If the item was used successfully, update the inventory
        if (itemUsed)
        {
            if (!selectedFoodItem.UseItem())
            {
                selectedFoodItem = null;  // Clear selected item if no more are left
            }
        }
    }
    public void SelectFoodItemForFeeding(FoodShelfItem foodItem)
    {
        selectedFoodItem = foodItem;
        notificationTexts.text = $"Selected {foodItem.item.itemFoodName} for feeding. Choose a character.";

        // Enable all feed buttons
        EnableFeedingCharactersButtons();
    }
    void EnableFeedingCharactersButtons()
    {
        foreach (var button in feedPlayerCharactersButtons)
        {
            button.enabled = true;
        }
    }
    void DisableFeedingCharactersButtons()
    {
        foreach (var button in feedPlayerCharactersButtons)
        {
            button.enabled = false;
        }
    }
    public bool FeedCharacter(ref ItemFood foodItem, Unit fedUnit)
    {
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
            UpdateCharacterStatsCounter(fedUnit);
            SaveRestoredCharacterStats();
            GameObject loveIconPrefabInstance = Instantiate(loveIconPrefab, loveIconPrefabTransform);
            Destroy(loveIconPrefabInstance, 1);
        }
        else
        {
            notificationTexts.text = $"{fedUnit.unitTemplate.unitName} is already at full {(foodItem.itemFoodType == ItemFoodType.HPRecovery ? "HP" : "MP")}!";
        }

        return itemUsed; // Return whether the item was used successfully
    }
    void UpdateCharacterStatsCounter(Unit fedUnit)
    {
        foreach (var smallProfileController in characterProfileSmallControllers)
        {
            if (fedUnit == smallProfileController.GetComponent<CharacterProfileSmallController>().referenceUnit)
            {
                characterTexts = smallProfileController.GetComponentsInChildren<TextMeshProUGUI>();

                if (characterTexts.Length >= 5)
                {
                    characterTexts[2].text = fedUnit.unitHealthPoints.ToString();
                    characterTexts[6].text = fedUnit.unitManaPoints.ToString();
                }
            }

        }
    }
    IEnumerator ClearNotificationText(GameObject currentEmoticon)
    {
        float clearNotificationWaitingTime = 1.5f;
        yield return new WaitForSeconds(clearNotificationWaitingTime);
        notificationTexts.text = "";
    }
    public void SaveRestoredCharacterStats()
    {
        //Saves the stats after feeding.
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == playerUnit.Id);
            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitHealthPoints = playerUnit.unitHealthPoints;
                existingCharacterData.unitSavedManaPoints = playerUnit.unitManaPoints;
                existingCharacterData.unitShieldPoints = playerUnit.unitShieldPoints;

                existingCharacterData.unitLifeCondition = playerUnit.currentUnitLifeCondition;

                existingCharacterData.unitAttackPower = playerUnit.unitAttackPower;
                existingCharacterData.unitMagicPower = playerUnit.unitMagicPower;

                // Update other stats as necessary

                Debug.Log("Character Stats Saved");
            }
        }
        SaveStateManager.SaveGame(characterSaveData);
    }
    private void AddItemToFoodShelf(ItemFood item)
    {
        // Check if the item is already on the Food Shelf
        foreach (Transform child in foodShelfContainer)
        {
            FoodShelfItem shelfItem = child.GetComponent<FoodShelfItem>();
            if (shelfItem != null && shelfItem.item == item)
            {
                // If the item already exists, increase the quantity
                shelfItem.IncreaseQuantity();
                return;
            }
        }
        // If item is not already on the shelf, create a new shelf item
        GameObject foodShelfItem = Instantiate(foodShelfItemPrefab, foodShelfContainer);
        FoodShelfItem shelfItemComponent = foodShelfItem.GetComponent<FoodShelfItem>();

        // Set item details using ScriptableObject data
        shelfItemComponent.SetItem(item);
    }
}