using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListUIController : MonoBehaviour
{
    public List<GameObject> characterProfileObjs = new List<GameObject>();
    public List<CharacterProfileSmallController> characterProfileSmallControllers = new List<CharacterProfileSmallController>();

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
                    profileController.UpdateUIStats();
                    //characterTexts[1].text = $"HP {partyMember.unitHealthPoints} / {partyMember.unitMaxHealthPoints}";
                    //characterTexts[2].text = $"MP {partyMember.unitManaPoints} / {partyMember.unitMaxManaPoints}";
                }

                // Add the instantiated profile object to the list of small profiles.
                characterProfileObjs.Add(characterProfile);
                // Add the corresponding controller to the list of controllers.
                CharacterProfileSmallController characterProfileController = characterProfile.GetComponent<CharacterProfileSmallController>();
                characterProfileSmallControllers.Add(characterProfileController);
            }
        }
    }
    public void OnCharacterClicked(Unit character)
    {
    }
    public void UpdateCharacterStatsCounter(Unit fedUnit)
    {
        foreach (CharacterProfileSmallController smallProfileController in characterProfileSmallControllers)
        {
            if (fedUnit == smallProfileController.referenceUnit)
            {
                smallProfileController.UpdateUIStats();
                //characterTexts = smallProfileController.GetComponentsInChildren<TextMeshProUGUI>();

                //if (characterTexts.Length >= 5)
                //{
                //    characterTexts[2].text = fedUnit.unitHealthPoints.ToString();
                //    characterTexts[6].text = fedUnit.unitManaPoints.ToString();
                //}
            }
        }
    }
}
