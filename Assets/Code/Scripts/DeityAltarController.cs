using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerProfileController;
using UnityEngine.UI;
using RPGCharacterAnims.Actions;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.IO;

public class DeityAltarController : MonoBehaviour
{
    //public GameObject playerPartyMemberProfileGO;
    public RectTransform playerPartyContainer;
    public Unit selectedPlayerUnit;
    public RectTransform capturedDeitiesContainer;
    public GameObject playerUnitImageGO;
    public GameObject deityImageGO;
    public GameObject deityLinkButtonPrefab;
    public RectTransform deityLinkMenuContainer;
    public RectTransform saveDeityLinkButtonContainer;

    [Serialize]

    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public void Start()
    {
        CreateLinkButton();
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            Sprite playerUnitPortrait = playerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            GameObject newPlayerUnitImage = Instantiate(playerUnitImageGO, playerPartyContainer);
            newPlayerUnitImage.GetComponent<Image>().sprite = playerUnitPortrait;
            newPlayerUnitImage.tag = "Player";
            newPlayerUnitImage.GetComponent<UnitImageController>().unitReference = playerUnit;
        }
        foreach (var capturedDeity in GameManager.Instance.collectibleDeities)
        {
            Sprite deityPortrait = capturedDeity.deityPortrait;
            GameObject newDeityUnitImage = Instantiate(deityImageGO, capturedDeitiesContainer);
            newDeityUnitImage.tag = "Deity";
            newDeityUnitImage.GetComponent<Image>().sprite = deityPortrait;
            newDeityUnitImage.GetComponent<UnitImageController>().deityReference = capturedDeity;
        }

    }

    public void CreateLinkButton()
    {
        GameObject deityLinkButtonInstance = Instantiate(deityLinkButtonPrefab, saveDeityLinkButtonContainer);
        Button currentdDeityLinkButton = deityLinkButtonInstance.GetComponent<Button>();
        currentdDeityLinkButton.onClick.AddListener(() => GameManager.Instance.ApplyDeityLinks());
    }

    public void SetCurrentSelectedUnit(Unit unit)
    {
        selectedPlayerUnit = unit;
    }

    public void AssignDeityToUnit(Deity deity)
    {
        selectedPlayerUnit.LinkedDeityId = deity.Id; // Link the Deity ID to the Unit
        Debug.Log("Linked Player to Deity"); // Any additional logic for effects on the Unit's stats or state
        CreateDictionaryEntry(deity);
    }
    public void CreateDictionaryEntry(Deity deity)
    {
        string selectedPlayerUnitId = selectedPlayerUnit.Id;
        unitsLinkedToDeities[selectedPlayerUnitId] = deity.Id;
        SaveDictionaryToFile(unitsLinkedToDeities);
    }

    public void SaveDictionaryToFile(Dictionary<string, string> unitsLinkedToDeities)
    {
        string saveState = JsonConvert.SerializeObject(unitsLinkedToDeities, Formatting.Indented);
        File.WriteAllText(Application.persistentDataPath + "/savegame.json", saveState);

        Debug.Log("Saving Dictionary");
    }
}
