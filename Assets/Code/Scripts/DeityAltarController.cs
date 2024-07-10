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
    [SerializeField] GameObject playerUnitProfileGO;
    [SerializeField] GameObject deityProfileGO;

    [SerializeField] Transform deitySpot;

    [Serialize]

    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public void Start()
    {
        GameManager.Instance.ApplyDeityLinks();
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            GameObject newPlayerUnitProfileInstance = Instantiate(playerUnitProfileGO, playerPartyMembembersContainer);
            newPlayerUnitProfileInstance.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(playerUnit);

            //Sprite playerUnitPortrait = playerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            //GameObject newPlayerUnitImage = Instantiate(playerUnitImageGO, playerPartyMembembersContainer);
            //newPlayerUnitImage.GetComponent<Image>().sprite = playerUnitPortrait;
            //newPlayerUnitImage.tag = "Player";
            //newPlayerUnitImage.GetComponent<UnitImageController>().unitReference = playerUnit;
            //TextMeshProUGUI playerName = Instantiate(nameLabelPrefab, playerPartyMembembersContainer).GetComponent<TextMeshProUGUI>();
            //playerName.text = playerUnit.unitTemplate.unitName;
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

            GameObject newDeityUnitProfileInstance = Instantiate(deityProfileGO, capturedDeitiesContainer);
            newDeityUnitProfileInstance.GetComponent<AltarDeityUnitProfileController>().PopulateDeityUnitProfile(deity.GetComponent<Unit>(), deity);


            //GameObject deityModel = Instantiate(deity.gameObject, deitySpot.transform.position, Quaternion.identity);
            //deityModel.transform.localScale = new Vector3(2, 2, 2);

            //Sprite deityPortrait = deity.deityPortrait;

            //GameObject newDeityUnitImage = Instantiate(deityImageGO, capturedDeitiesContainer);
            //newDeityUnitImage.tag = "Deity";
            //newDeityUnitImage.GetComponent<Image>().sprite = deityPortrait;
            //newDeityUnitImage.GetComponent<UnitImageController>().deityReference = deity;

            //TextMeshProUGUI deityName = Instantiate(nameLabelPrefab, capturedDeitiesContainer).GetComponent<TextMeshProUGUI>();
            //deityName.text = deity.name;
        }
    }

    public void SetCurrentSelectedUnit(Unit unit)
    {
        selectedPlayerUnit = unit;
    }

    public void AssignDeityToUnit(Deity deity)
    {
        if (selectedPlayerUnit == null)
        {
            Debug.LogError("No player unit selected.");
            return;
        }

        if (deity == null)
        {
            Debug.LogError("No deity selected.");
            return;
        }

        GameSaveData saveData = SaveStateManager.saveData;

        string selectedPlayerUnitId = selectedPlayerUnit.Id;
        string deityId = deity.Id;

        // Find if the deity is already linked to another unit
        string oldLinkedUnitId = null;
        foreach (var entry in saveData.unitsLinkedToDeities)
        {
            if (entry.Value == deityId)
            {
                oldLinkedUnitId = entry.Key;
                break;
            }
        }

        // If the deity is already linked, remove the old link
        if (oldLinkedUnitId != null)
        {
            saveData.unitsLinkedToDeities.Remove(oldLinkedUnitId);
        }

        // Remove existing link for the selected unit if it exists
        if (saveData.unitsLinkedToDeities.ContainsKey(selectedPlayerUnitId))
        {
            saveData.unitsLinkedToDeities.Remove(selectedPlayerUnitId);
        }

        // Add the new link
        saveData.unitsLinkedToDeities.Add(selectedPlayerUnitId, deityId);
        SaveStateManager.SaveGame(saveData);
        GameManager.Instance.ApplyDeityLinks();

        Debug.Log("Deity successfully assigned to unit.");
    }

}
