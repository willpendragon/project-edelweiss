using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CafeMenuUIController : MonoBehaviour
{
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



    void Start()
    {
        gameStatsManager = GameObject.FindWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        warFundsCounter.text = gameStatsManager.warFunds.ToString();
        GenerateFoodList();
        GenerateCharacterList();
    }

    void GenerateFoodList()
    {
        foreach (var food in itemFoodList)
        {
            GameObject foodItem = Instantiate(itemFoodPrefab, foodItemsContainer.transform);
            foodItem.GetComponent<Image>().sprite = food.foodIcon;
            Button itemFoodButton = foodItem.GetComponentInChildren<Button>();
            itemFoodButton.onClick.AddListener(() => PurchaseFood(food, food.itemFoodPrice));

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
        else
        {
            foodTypeLabel = "Mana Recovery";
            return foodTypeLabel;
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
        foreach (var partyMember in GameManager.Instance.playerPartyMembersInstances)
        {
            GameObject characterProfile = Instantiate(characterProfilesPrefab, characterProfilesContainer.transform);
            CharacterProfileSmallController profileController = characterProfile.GetComponent<CharacterProfileSmallController>();
            profileController.referenceUnit = partyMember;

            characterProfile.GetComponentInChildren<Image>().sprite = partyMember.GetComponent<Unit>().unitTemplate.unitPortrait;

            characterTexts = characterProfile.GetComponentsInChildren<TextMeshProUGUI>();

            if (characterTexts.Length >= 5)
            {
                characterTexts[0].text = partyMember.name;
                characterTexts[1].text = "HP";
                characterTexts[2].text = partyMember.unitHealthPoints.ToString();
                characterTexts[3].text = "MP";
                characterTexts[4].text = partyMember.unitManaPoints.ToString();
            }

            characterProfile.GetComponentInChildren<TextMeshProUGUI>().text = partyMember.GetComponent<Unit>().unitTemplate.unitName;
            characterProfileSmallControllers.Add(characterProfile);


            // Create the button GameObject
            GameObject feedCharacterButtonGO = new GameObject("CharacterFeedButton");

            RectTransform rectTransform = feedCharacterButtonGO.AddComponent<RectTransform>();
            rectTransform.SetParent(characterProfile.transform, false); // Set parent with worldPositionStays = false to maintain proper UI scaling and positioning

            Image buttonImage = feedCharacterButtonGO.AddComponent<Image>();

            // Add the button component
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

            // Add an onClick listener
            feedCharacterButton.onClick.AddListener(() => FeedCharacter(ref currentPurchasedFood, profileController.referenceUnit));
            feedCharacterButton.enabled = false;
            feedPlayerCharactersButtons.Add(feedCharacterButton);
        }
    }

    public void FeedCharacter(ref ItemFood currentPurchasedFood, Unit fedUnit)
    {
        if (currentPurchasedFood != null && currentPurchasedFood.itemFoodType == ItemFoodType.HPRecovery)
        {
            if (fedUnit.unitHealthPoints < fedUnit.unitMaxHealthPoints)
            {
                fedUnit.unitHealthPoints += currentPurchasedFood.recoveryAmount;
                if (fedUnit.unitHealthPoints > fedUnit.unitMaxHealthPoints)
                {
                    fedUnit.unitHealthPoints = fedUnit.unitMaxHealthPoints;
                }
                UpdateCharacterStatsCounter(fedUnit);
                notificationTexts.text = fedUnit.unitTemplate.unitName + " recovered " + currentPurchasedFood.recoveryAmount.ToString() + " HP!";

                GameObject loveIconPrefabInstance = Instantiate(loveIconPrefab, loveIconPrefabTransform);

                StartCoroutine(ClearNotificationText(loveIconPrefabInstance));

            }
            else if (fedUnit.unitHealthPoints == fedUnit.unitMaxHealthPoints)
            {
                notificationTexts.text = fedUnit.unitTemplate.unitName + " is already at full HP!";
                StartCoroutine("ClearNotificationText");
            }
        }
        else if (currentPurchasedFood != null && currentPurchasedFood.itemFoodType == ItemFoodType.manaRecovery)
        {
            if (fedUnit.unitManaPoints < fedUnit.unitMaxManaPoints)
            {
                fedUnit.unitManaPoints += currentPurchasedFood.recoveryAmount;
                if (fedUnit.unitManaPoints > fedUnit.unitMaxManaPoints)
                {
                    fedUnit.unitManaPoints = fedUnit.unitMaxManaPoints;
                }
                UpdateCharacterStatsCounter(fedUnit);
                notificationTexts.text = fedUnit.unitTemplate.unitName + " recovered " + currentPurchasedFood.recoveryAmount.ToString() + " MP!";

                GameObject loveIconPrefabInstance = Instantiate(loveIconPrefab, loveIconPrefabTransform);

                StartCoroutine(ClearNotificationText(loveIconPrefabInstance));

            }
            else if (fedUnit.unitManaPoints == fedUnit.unitMaxManaPoints)
            {
                notificationTexts.text = fedUnit.unitTemplate.unitName + " is already at full MP!";
                StartCoroutine("ClearNotificationText");
            }
        }
        currentPurchasedFood = null;
        DisableFeedingCharactersButtons();
    }

    void EnableFeedingCharactersButtons()
    {
        foreach (var button in feedPlayerCharactersButtons)
        {
            button.enabled = true;
            button.GetComponent<Image>().color = Color.yellow;
        }
    }

    void DisableFeedingCharactersButtons()
    {
        foreach (var button in feedPlayerCharactersButtons)
        {
            button.enabled = false;
        }
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
                    characterTexts[4].text = fedUnit.unitManaPoints.ToString();
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
}
