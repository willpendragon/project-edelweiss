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
        selectedPlayerUnitProfileGO = unitPanel;
        selectedPlayerUnitProfileGO.GetComponentInChildren<Image>().color = Color.cyan;
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

        // If the deity is already linked, remove the old link.
        if (oldLinkedUnitId != null)
        {
            saveData.unitsLinkedToDeities.Remove(oldLinkedUnitId);
        }

        // Remove existing link for the selected unit (if it exists).
        if (saveData.unitsLinkedToDeities.ContainsKey(selectedPlayerUnitId))
        {
            //RemoveDeityBuffsOnUnit(selectedPlayerUnit, deity);
            saveData.unitsLinkedToDeities.Remove(selectedPlayerUnitId);
            selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = "No Link";
            //RemoveDeityBuffsOnOtherUnits(deity);
        }

        // Add the new link.
        PlayLinkAnimation(selectedPlayerUnit, deity);
        saveData.unitsLinkedToDeities.Add(selectedPlayerUnitId, deityId);

        selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = deity.GetComponent<Unit>().unitTemplate.unitName;

        GameManager.Instance.ApplyDeityLinks();

        //ApplyDeityBuffsOnUnit(selectedPlayerUnit, deity);

        SaveStateManager.SaveGame(saveData);

        Debug.Log("Deity successfully assigned to unit.");

        GameObject[] playerUnitContainers = GameObject.FindGameObjectsWithTag("PlayerUnitContainer");
        foreach (var playerUnitContainer in playerUnitContainers)
        {
            Image buttonImage = playerUnitContainer.GetComponentInChildren<Image>();
            buttonImage.color = Color.white;
        }
    }
    public void PlayLinkAnimation(Unit selectedPlayerUnit, Deity deity)
    {
        Sprite selectedPlayerUnitPortrait = selectedPlayerUnit.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;
        Sprite linkedDeityUnitPortrait = deity.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;

        deityLinkCalloutController.PlayDeityLinkCalloutTransition(selectedPlayerUnitPortrait, linkedDeityUnitPortrait);
    }

    //public void ApplyDeityBuffsOnUnit(Unit unit, Deity deity)
    //{
    //    if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.MagicPower)
    //    {
    //        selectedPlayerUnit.unitMagicPower += deity.deityPrayerBuff.buffAmount;
    //    }
    //    else if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.AttackPower)
    //    {
    //        selectedPlayerUnit.unitAttackPower += deity.deityPrayerBuff.buffAmount;
    //    }

    //    foreach (var playerUnitProfile in playerUnitsProfiles)
    //    {
    //        if (unit.Id == playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().playerId)
    //        {
    //            playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(unit);
    //        }
    //    }
    //}

    public void RemoveDeityBuffsOnUnit(Unit unit, Deity deity)
    {
        if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.MagicPower)
        {
            selectedPlayerUnit.unitMagicPower -= deity.deityPrayerBuff.buffAmount;
        }
        else if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.AttackPower)
        {
            selectedPlayerUnit.unitAttackPower -= deity.deityPrayerBuff.buffAmount;
        }

        foreach (var playerUnitProfile in playerUnitsProfiles)
        {
            if (unit.Id == playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().playerId)
            {
                playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(unit);
            }
        }
        RemoveDeityBuffsOnOtherUnits(deity);
    }

    public void RemoveDeityBuffsOnOtherUnits(Deity deity)
    {
        List<Unit> playerUnitsInstances = GameManager.Instance.playerPartyMembersInstances;
        foreach (Unit playerUnit in playerUnitsInstances)
        {
            if (playerUnit.LinkedDeityId != deity.Id)
            {
                if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.MagicPower)
                {
                    selectedPlayerUnit.unitMagicPower -= deity.deityPrayerBuff.buffAmount;
                    UpdatePlayerUnitProfile(playerUnit);

                }
                else if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.AttackPower)
                {
                    selectedPlayerUnit.unitAttackPower -= deity.deityPrayerBuff.buffAmount;
                    UpdatePlayerUnitProfile(playerUnit);
                }
            }
        }
    }

    public void UpdatePlayerUnitProfile(Unit unit)
    {
        foreach (var playerUnitProfile in playerUnitsProfiles)
        {
            if (playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().playerId == unit.Id)
            {
                playerUnitProfile.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(unit);
            }
        }
    }
}
