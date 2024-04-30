using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerProfileController;
using UnityEngine.UI;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.IO;
using TMPro;

public class DeityAltarController : MonoBehaviour
{
    //public GameObject playerPartyMemberProfileGO;
    public RectTransform playerPartyMembembersContainer;
    public RectTransform capturedDeitiesContainer;

    public Unit selectedPlayerUnit;
    public GameObject playerUnitImageGO;
    public GameObject deityImageGO;
    public GameObject deityLinkButtonPrefab;
    public RectTransform deityLinkMenuContainer;
    public RectTransform saveDeityLinkButtonContainer;
    public TextMeshProUGUI nameLabelPrefab;

    [SerializeField] Transform deitySpot;

    [Serialize]

    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public void Start()
    {
        GameManager.Instance.ApplyDeityLinks();
        //CreateLinkButton();
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            Sprite playerUnitPortrait = playerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            GameObject newPlayerUnitImage = Instantiate(playerUnitImageGO, playerPartyMembembersContainer);
            newPlayerUnitImage.GetComponent<Image>().sprite = playerUnitPortrait;
            newPlayerUnitImage.tag = "Player";
            newPlayerUnitImage.GetComponent<UnitImageController>().unitReference = playerUnit;
            TextMeshProUGUI playerName = Instantiate(nameLabelPrefab, playerPartyMembembersContainer).GetComponent<TextMeshProUGUI>();
            playerName.text = playerUnit.unitTemplate.unitName;
        }


        Dictionary<string, string> unitsLinkedToDeities = SaveStateManager.saveData.unitsLinkedToDeities;
        foreach (var entry in unitsLinkedToDeities)
        {
            string unitID = entry.Key;
            string deityID = entry.Value;
        }

        foreach (var unitPrefab in GameManager.Instance.playerPartyMembersInstances)
        {
            Unit unit = unitPrefab.GetComponent<Unit>();
            if (unit == null) continue; // Safety check

            unitsLinkedToDeities.TryGetValue(unit.Id, out string connectedDeityId);
            unit.LinkedDeityId = connectedDeityId;

            // Safely find the linked deity
            var deity = GameManager.Instance.collectibleDeities.Find(d => d.Id == unit.LinkedDeityId);
            if (deity == null) continue; // Skip if no deity found

            GameObject deityModel = Instantiate(deity.gameObject, deitySpot.transform.position, Quaternion.identity);

            Sprite deityPortrait = deity.deityPortrait;

            GameObject newDeityUnitImage = Instantiate(deityImageGO, capturedDeitiesContainer);
            newDeityUnitImage.tag = "Deity";
            newDeityUnitImage.GetComponent<Image>().sprite = deityPortrait;
            newDeityUnitImage.GetComponent<UnitImageController>().deityReference = deity;

            TextMeshProUGUI deityName = Instantiate(nameLabelPrefab, capturedDeitiesContainer).GetComponent<TextMeshProUGUI>();
            deityName.text = deity.name;
        }
    }

    //public void CreateLinkButton()
    //{
    //    GameObject deityLinkButtonInstance = Instantiate(deityLinkButtonPrefab, saveDeityLinkButtonContainer);
    //    Button currentdDeityLinkButton = deityLinkButtonInstance.GetComponent<Button>();
    //    currentdDeityLinkButton.onClick.AddListener(() => GameManager.Instance.ApplyDeityLinks());
    //}

    public void SetCurrentSelectedUnit(Unit unit)
    {
        selectedPlayerUnit = unit;
    }

    public void AssignDeityToUnit(Deity deity)
    {
        string selectedPlayerUnitId = selectedPlayerUnit.Id;
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.unitsLinkedToDeities.Add(selectedPlayerUnitId, deity.Id);
        SaveStateManager.SaveGame(saveData);
        GameManager.Instance.ApplyDeityLinks();
    }
    //public void CreateDictionaryEntry(Deity deity)
    //{
    //    string selectedPlayerUnitId = selectedPlayerUnit.Id;
    //    unitsLinkedToDeities[selectedPlayerUnitId] = deity.Id;
    //    SaveDictionaryToFile(unitsLinkedToDeities);
    //}
    //public void SaveDictionaryToFile(Dictionary<string, string> unitsLinkedToDeities)
    //{
    //    string saveState = JsonConvert.SerializeObject(unitsLinkedToDeities, Formatting.Indented);
    //    File.WriteAllText(Application.persistentDataPath + "/savegame.json", saveState);

    //    Debug.Log("Saving Dictionary");
    //}
}
