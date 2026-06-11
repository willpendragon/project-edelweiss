using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;
using System;
public class DeityAltarController : MonoBehaviour
{

    [Header("Game Logic")]
    public Unit selectedPlayerUnit;
    [SerializeField] DeityLinkCalloutController deityLinkCalloutController;
    [SerializeField] SummoningBuffController summoningBuffController;
    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();

    [Header("UI")]
    [SerializeField] DeityListUIController _deityListUIController;
    [SerializeField] RectTransform playerPartyMembersContainer;
    [SerializeField] RectTransform capturedDeitiesContainer;
    [SerializeField] GameObject playerUnitImageGO;
    [SerializeField] GameObject deityImageGO;
    [SerializeField] RectTransform deityLinkMenuContainer;
    [SerializeField] RectTransform saveDeityLinkButtonContainer;
    [SerializeField] TextMeshProUGUI nameLabelPrefab;
    [SerializeField] GameObject playerUnitProfileGO;
    [SerializeField] GameObject deityProfileGO;
    private GameObject selectedPlayerUnitProfileGO;
    [SerializeField] Image fadePanel;
    [SerializeField] List<GameObject> playerUnitsProfiles = new List<GameObject>();

    [Header("Visuals")]
    [SerializeField] Transform deitySpot;

    public void Start()
    {
        GameManager.Instance.DeityLinkManager.ApplyDeityLinks();
        List<Unit> playerPartyMemberInstances = GameManager.Instance.playerPartyMembersInstances;

        // Creates a Player Profile in the Deity Altar for each party member.
        foreach (var playerUnit in playerPartyMemberInstances)
        {
            GameObject newPlayerUnitProfileInstance = Instantiate(playerUnitProfileGO, playerPartyMembersContainer);
            var playerProfileController = newPlayerUnitProfileInstance.GetComponent<AltarPlayerUnitProfileController>();

            playerProfileController.PopulatePlayerUnitProfile(playerUnit);
            playerProfileController.ownerUnit = playerUnit;
            if (playerUnit.linkedDeity != null)
            {
                playerProfileController.UpdatePlayerUnitLinkedDeityPortrait(playerUnit.linkedDeity);
                // Instiate Deity Profile in the List
                _deityListUIController.AddDeityProfile(playerUnit.linkedDeity);
            }
            playerUnitsProfiles.Add(newPlayerUnitProfileInstance);
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
            if (unit == null) continue; // Safety check.

            unitsLinkedToDeities.TryGetValue(unit.Id, out string connectedDeityId);
            unit.LinkedDeityId = connectedDeityId;

            // Safely find the linked Deity.
            var deity = GameManager.Instance.DeityLinkManager.collectibleDeities.Find(d => d.Id == unit.LinkedDeityId);
            if (deity == null) continue; // Skip if no deity found.

            GameObject newDeityUnitProfileInstance = Instantiate(deityProfileGO, capturedDeitiesContainer);
            newDeityUnitProfileInstance.GetComponent<AltarDeityUnitProfileController>().PopulateDeityUnitProfile(deity.GetComponent<Unit>(), deity);
        }
    }
    public void SetCurrentSelectedUnit(Unit unit, GameObject unitPanel)
    {
        // Reset the color of all unit profiles
        foreach (var profile in playerUnitsProfiles)
        {
            Image profileImage = profile.GetComponentInChildren<Image>();
            if (profileImage != null)
            {
                profileImage.color = Color.white; // Reset to default color
            }
        }

        // Set the selected unit and highlight its profile
        selectedPlayerUnitProfileGO = unitPanel;
        Image selectedImage = selectedPlayerUnitProfileGO.GetComponentInChildren<Image>();
        if (selectedImage != null)
        {
            selectedImage.color = Color.cyan; // Highlight the selected tab
        }
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

        // Find if the deity is already linked to another Unit.
        string oldLinkedUnitId = null;
        foreach (var entry in saveData.unitsLinkedToDeities)
        {
            if (entry.Value == deityId)
            {
                oldLinkedUnitId = entry.Key;
                break;
            }
        }

        // Remove Deity link buffs from previously linked Unit
        foreach (Unit playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            if (playerUnit.linkedDeity == deity && playerUnit != selectedPlayerUnit)
            {
                summoningBuffController.RemoveLinkedDeityPermanentBuff(playerUnit);
                UpdatePlayerUnitProfile(playerUnit);
                ResetPlayerUnitDeityPortrait(selectedPlayerUnit);

            }
        }

        // If the deity is already linked, remove the old link.
        if (oldLinkedUnitId != null)
        {
            saveData.unitsLinkedToDeities.Remove(oldLinkedUnitId);
        }

        // Remove existing Deity link for the selected unit (if it exists).
        if (saveData.unitsLinkedToDeities.ContainsKey(selectedPlayerUnitId))
        {
            saveData.unitsLinkedToDeities.Remove(selectedPlayerUnitId);
            selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = "No Link";
        }

        if (selectedPlayerUnit.LinkedDeityId != deity.Id)
        {
            // Add the new link.
            PlayLinkAnimation(selectedPlayerUnit, deity);
            saveData.unitsLinkedToDeities.Add(selectedPlayerUnitId, deityId);

            selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = deity.GetComponent<Unit>().unitTemplate.unitName;
            GameManager.Instance.DeityLinkManager.ApplyDeityLinks();
            summoningBuffController.ApplyLinkedDeityPermanentBuff(selectedPlayerUnit);
            UpdatePlayerUnitProfile(selectedPlayerUnit);
            var altarProfileController = selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>();
            altarProfileController.UpdatePlayerUnitLinkedDeityPortrait(deity);

            SaveStateManager.SaveGame(saveData);
            Debug.Log($"{deityId} successfully assigned to {selectedPlayerUnitId}.");

            GameObject[] playerUnitContainers = GameObject.FindGameObjectsWithTag("PlayerUnitContainer");
            foreach (var playerUnitContainer in playerUnitContainers)
            {
                Image buttonImage = playerUnitContainer.GetComponentInChildren<Image>();
                buttonImage.color = Color.white;
            }
        }
        else
        {
            // Add error feedback.
            Debug.Log("Unable to connect Unit to the Deity. This Unit is already connected to this Deity");
        }
    }

    private void ResetPlayerUnitDeityPortrait(Unit selectedPlayerUnit)
    {
        foreach (var playerUnitProfile in playerUnitsProfiles)
        {
            var altarProfileController = playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>();
            if (altarProfileController.playerId != selectedPlayerUnit.Id)
            {
                altarProfileController.ResetPlayerUnitLinkedDeityPortrait();
            }
        }
    }

    public void PlayLinkAnimation(Unit selectedPlayerUnit, Deity deity)
    {
        Sprite selectedPlayerUnitPortrait = selectedPlayerUnit.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;
        Sprite linkedDeityUnitPortrait = deity.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;
        deityLinkCalloutController.PlayDeityLinkCalloutTransition(selectedPlayerUnitPortrait, linkedDeityUnitPortrait);
    }

    public void UpdatePlayerUnitProfile(Unit unit)
    {
        foreach (var playerUnitProfile in playerUnitsProfiles)
        {
            var altarProfileController = playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>();
            if (altarProfileController.playerId == unit.Id)
            {
                altarProfileController.UpdatePlayerUnitProfile(unit);
            }
        }
    }
}