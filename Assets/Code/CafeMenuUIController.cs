using System.Collections;
using System.Collections.Generic;
using TMPro;
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
            itemFoodButton.GetComponentInChildren<TextMeshProUGUI>().text = food.itemFoodPrice.ToString();
        }
    }

    public void PurchaseFood(ItemFood purchasedFood, float foodPrice)
    {
        gameStatsManager.warFunds -= foodPrice;
        gameStatsManager.SaveSpentWarFunds(foodPrice);
        UpdateWarFundsCounter();
        currentPurchasedFood = purchasedFood;
        //Update Counter;
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
            characterProfile.GetComponent<CharacterProfileSmallController>().referenceUnit = partyMember;
            characterProfile.GetComponentInChildren<Image>().sprite = partyMember.GetComponent<Unit>().unitTemplate.unitPortrait;
            characterProfile.GetComponentInChildren<TextMeshProUGUI>().text = partyMember.GetComponent<Unit>().unitTemplate.unitName;
            // Create the button GameObject
            GameObject feedCharacterButtonGO = new GameObject("CharacterFeedButton");

            // Add RectTransform and set it up
            RectTransform rectTransform = feedCharacterButtonGO.AddComponent<RectTransform>();
            rectTransform.SetParent(characterProfile.transform, false); // Set parent with worldPositionStays = false to maintain proper UI scaling and positioning

            // Add an Image component to make the button visible
            Image buttonImage = feedCharacterButtonGO.AddComponent<Image>();
            // Set the button image sprite here if you have one, e.g.,
            // buttonImage.sprite = someSprite;
            // buttonImage.type = Image.Type.Sliced; // If you want to use a sliced image

            // Add the button component
            Button feedCharacterButton = feedCharacterButtonGO.AddComponent<Button>();

            GameObject textGO = new GameObject("ButtonText");
            RectTransform textRectTransform = textGO.AddComponent<RectTransform>();
            textRectTransform.SetParent(feedCharacterButtonGO.transform, false);

            // Set the TextMeshProUGUI component's RectTransform to fill the parent (the button in this case)
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero; // Reset size delta to zero for proper filling

            TextMeshProUGUI textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            // Configure your text properties here
            textMeshPro.text = "Feed Character"; // Example text
            textMeshPro.fontSize = 24; // Example font size
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.color = Color.black; // Example text color

            // Set up button colors if needed
            ColorBlock colors = feedCharacterButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1);
            colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1);
            feedCharacterButton.colors = colors;

            // Add an onClick listener
            feedCharacterButton.onClick.AddListener(() => FeedCharacter(currentPurchasedFood));
        }
    }

    public void FeedCharacter(ItemFood currentPurchasedFood)
    {
        if (currentPurchasedFood != null && currentPurchasedFood.itemFoodType == ItemFoodType.HPRecovery)
        {
            Unit fedUnit = characterProfilesContainer.GetComponentInChildren<CharacterProfileSmallController>().referenceUnit;
            if (fedUnit.unitHealthPoints <= fedUnit.unitMaxHealthPoints)
            {
                fedUnit.HealthPoints += currentPurchasedFood.recoveryAmount;
            }
        }
        else if (currentPurchasedFood != null && currentPurchasedFood.itemFoodType == ItemFoodType.manaRecovery)
        {
            Unit fedUnit = characterProfilesContainer.GetComponentInChildren<Unit>();
            if (fedUnit.unitManaPoints <= fedUnit.GetComponent<UnitTemplate>().unitManaPoints)
            {
                fedUnit.unitManaPoints += currentPurchasedFood.recoveryAmount;
            }
        }
        Debug.Log("Feeding Character");
    }
}
