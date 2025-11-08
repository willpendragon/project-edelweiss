using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListUIController : MonoBehaviour
{
    public List<GameObject> characterProfileSmallControllers = new List<GameObject>();
    public GameObject characterProfilesPrefab;
    public GameObject characterProfilesContainer;
    [SerializeField] GameObject _feedButton;
    public List<Button> feedPlayerCharactersButtons = new List<Button>();
    private TextMeshProUGUI[] characterTexts;

    public void GenerateCharacterList()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerPartyMembersInstances == null)
        {
            Debug.LogWarning("GameManager or playerPartyMembersInstances is null. Cannot generate character list.");
            return;
        }
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

                if (characterTexts.Length >= 3)
                {
                    characterTexts[0].text = partyMember.unitTemplate.unitName;
                    characterTexts[1].text = $"HP {partyMember.unitHealthPoints} / {partyMember.unitMaxHealthPoints}";
                    characterTexts[2].text = $"MP {partyMember.unitManaPoints} / {partyMember.unitMaxManaPoints}";
                }

                // Add the profile to the list of small controllers
                characterProfileSmallControllers.Add(characterProfile);

                // Create and configure the feed button for this character
                GameObject feedCharacterButton = Instantiate(_feedButton, characterProfile.transform);
                var button = feedCharacterButton.GetComponent<Button>();

                // Add the onClick listener for feeding the character
                Unit characterUnit = profileController.referenceUnit;  // Capture the reference to avoid closure issues
                button.onClick.AddListener(() => OnCharacterClicked(characterUnit));

                button.enabled = false;  // Disable initially; can be enabled when an item is selected
                feedPlayerCharactersButtons.Add(button);
            }
        }
    }
    public void OnCharacterClicked(Unit character)
    {
        //    if (selectedFoodItem == null)
        //    {
        //        notificationTexts.text = "Please select a food item first.";
        //        return;
        //    }

        //    // Feed the character with the selected food item
        //    bool itemUsed = FeedCharacter(ref selectedFoodItem.item, character);

        //    // If the item was used successfully, update the inventory
        //    if (itemUsed)
        //    {
        //        if (!selectedFoodItem.UseItem())
        //        {
        //            selectedFoodItem = null;  // Clear selected item if no more are left
        //        }
        //    }
    }
    public void UpdateCharacterStatsCounter(Unit fedUnit)
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
}
