using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using DG.Tweening;
using UnityEditor.Timeline.Actions;

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

    [Serialize]

    Dictionary<string, string> unitsLinkedToDeities = new Dictionary<string, string>();
    public void Start()
    {
        GameManager.Instance.ApplyDeityLinks();
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            GameObject newPlayerUnitProfileInstance = Instantiate(playerUnitProfileGO, playerPartyMembembersContainer);
            newPlayerUnitProfileInstance.GetComponent<AltarPlayerUnitProfileController>().PopulatePlayerUnitProfile(playerUnit);
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
            RemoveDeityBuffsOnUnit(selectedPlayerUnit, deity);
            saveData.unitsLinkedToDeities.Remove(selectedPlayerUnitId);
            selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = "No Link";

        }

        // Add the new link.
        PlayLinkAnimation(selectedPlayerUnit, deity);
        saveData.unitsLinkedToDeities.Add(selectedPlayerUnitId, deityId);

        selectedPlayerUnitProfileGO.GetComponent<AltarPlayerUnitProfileController>().linkedDeityName.text = deity.GetComponent<Unit>().unitTemplate.unitName;

        GameManager.Instance.ApplyDeityLinks();

        ApplyDeityBuffsOnUnit(selectedPlayerUnit, deity);

        SaveStateManager.SaveGame(saveData);


        Debug.Log("Deity successfully assigned to unit.");
    }

    public void PlayLinkAnimation(Unit selectedPlayerUnit, Deity deity)
    {
        Sprite selectedPlayerUnitPortrait = selectedPlayerUnit.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;
        Sprite linkedDeityUnitPortrait = deity.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;

        deityLinkCalloutController.PlayDeityLinkCalloutTransition(selectedPlayerUnitPortrait, linkedDeityUnitPortrait);
    }

    public void ApplyDeityBuffsOnUnit(Unit unit, Deity deity)
    {
        if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.MagicPower)
        {
            selectedPlayerUnit.unitMagicPower += deity.deityPrayerBuff.buffAmount;
        }
        else if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.AttackPower)
        {
            selectedPlayerUnit.unitAttackPower += deity.deityPrayerBuff.buffAmount;
        }
    }

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
    }

    //public void FadeEffect()
    //{
    //    float fadeDuration = 1.0f;

    //    // Fade to black
    //    if (fadePanel != null)
    //    {
    //        fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
    //        {
    //            fadePanel.DOFade(0f, fadeDuration);
    //        });
    //    }
    //}

}
