using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DeityAltarController : MonoBehaviour
{
    public RectTransform playerPartyMembembersContainer;
    public RectTransform capturedDeitiesContainer;

    public Unit selectedPlayerUnit;
    public GameObject playerUnitImageGO;
    public GameObject deityImageGO;
    public GameObject deityLinkButtonPrefab;
    public RectTransform deityLinkMenuContainer;
    public RectTransform saveDeityLinkButtonContainer;
    public TextMeshProUGUI nameLabelPrefab;

    [SerializeField] DeityLinkCalloutController deityLinkCalloutController;
    [SerializeField] SummoningBuffController summoningBuffController;

    [SerializeField] GameObject playerUnitProfileGO;
    [SerializeField] GameObject deityProfileGO;

    private GameObject selectedPlayerUnitProfileGO;

    [SerializeField] Image fadePanel;
    [SerializeField] Transform deitySpot;

    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();

    List<GameObject> playerUnitsProfiles = new List<GameObject>();
    public void Start()
    {
        GameManager.Instance.ApplyDeityLinks();
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            GameObject newPlayerUnitProfileInstance = Instantiate(playerUnitProfileGO, playerPartyMembembersContainer);
            newPlayerUnitProfileInstance.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(playerUnit);
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
            var deity = GameManager.Instance.collectibleDeities.Find(d => d.Id == unit.LinkedDeityId);
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
            GameManager.Instance.ApplyDeityLinks();
            summoningBuffController.ApplyLinkedDeityPermanentBuff(selectedPlayerUnit);
            UpdatePlayerUnitProfile(selectedPlayerUnit);

            SaveStateManager.SaveGame(saveData);
            Debug.Log("Deity successfully assigned to Unit.");

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
            if (playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().playerId == unit.Id)
            {
                playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().UpdatePlayerUnitProfile(unit);
            }
        }
    }
}
