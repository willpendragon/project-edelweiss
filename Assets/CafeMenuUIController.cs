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
            itemFoodButton.onClick.AddListener(() => PurchaseFood(food.itemFoodPrice));
            itemFoodButton.GetComponentInChildren<TextMeshProUGUI>().text = food.itemFoodPrice.ToString();
        }
    }

    public void PurchaseFood(float foodPrice)
    {
        gameStatsManager.warFunds -= foodPrice;
        gameStatsManager.SaveSpentWarFunds(foodPrice);
        UpdateWarFundsCounter();
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
            characterProfile.GetComponentInChildren<Image>().sprite = partyMember.GetComponent<Unit>().unitTemplate.unitPortrait;
            characterProfile.GetComponentInChildren<TextMeshProUGUI>().text = partyMember.GetComponent<Unit>().unitTemplate.unitName;
        }
    }
}
