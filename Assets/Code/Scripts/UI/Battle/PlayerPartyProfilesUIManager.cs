using DG.Tweening;
using ProjectEdelweiss.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPartyProfilesUIManager : MonoBehaviour
{

    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _playerProfilePrefab;
    [SerializeField] private RectTransform _playerProfileContainer;
    [SerializeField] private Dictionary<Unit, PlayerPartyProfileHelper> unitsDictionary = new Dictionary<Unit, PlayerPartyProfileHelper>();

    void Start()
    {
        _gameManager = GameManager.Instance;
        CreatePlayerUnitsEntries();
        DOVirtual.DelayedCall(0.1f, () => UpdateValuesAtStart());
        //UpdateValuesAtStart();
    }
    private void UpdateValuesAtStart()
    {
        foreach (var unit in _gameManager.playerPartyMembersInstances)
        {
            UpdateProfile(unit.unitTemplate.unitName);
        }
    }

    private void CreatePlayerUnitsEntries()
    {
        foreach (var unit in _gameManager.playerPartyMembersInstances)
        {
            // Instantiate the Player Profile Object.
            GameObject newPlayerProfilePrefab = Instantiate(_playerProfilePrefab, _playerProfileContainer);
            // Retrieve the Helper.
            PlayerPartyProfileHelper playerPartyProfileHelper = newPlayerProfilePrefab.GetComponent<PlayerPartyProfileHelper>();
            // Fill the Player Profile Object Details
            playerPartyProfileHelper.FillPlayerDetails(unit.GetComponent<Unit>());
            unitsDictionary.Add(unit, playerPartyProfileHelper);
            // Create a dictionary entry with the Player Unit's Names and the Profile Objects
        }
        PrintDictionary();
    }

    public void UpdateProfile(string unitName)
    {
        // Search in the Dictionary.
        // Sort by unitName.
        Unit matchingUnit = LookForUnit(unitName);
        if (matchingUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            return;
        }
        // Retrieve the corresponding Profile.
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        // Refresh Profile UI with updated gameplay stats.
        profileHelper.FillPlayerDetails(matchingUnit);
        profileHelper.UpdateRemainingMovesDisplay(matchingUnit);
    }

    public void SetDeadUnitProfile(string unitName)
    {
        Unit matchingUnit = LookForUnit(unitName);
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        profileHelper.DeactivateProfile();
    }

    private Unit LookForUnit(string unitName)
    {
        Unit matchingUnit = unitsDictionary.Keys
            .FirstOrDefault(u => u.unitTemplate.unitName.Equals(unitName, StringComparison.OrdinalIgnoreCase));

        if (matchingUnit == null)
        {
            Debug.Log($"The Unit {unitName} wasn't found in the dictionary.");
            return null;
        }
        return matchingUnit;
    }

    public void UpdateRemainingMoves(string unitName)
    {
        // Search in the Dictionary.
        // Sort by unitName.
        Unit matchingUnit = LookForUnit(unitName);
        // Retrieve the corresponding Profile.
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        // Refresh Profile UI with new remaining Moves Count.
        profileHelper.UpdateRemainingMovesDisplay(matchingUnit);
    }

    public void UpdateHPWrapper(string unitName)
    {
        // Search in the Dictionary.
        // Sort by unitName.
        Unit matchingUnit = LookForUnit(unitName);
        // Retrieve the corresponding Profile.
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        // Refresh Profile UI with new remaining Moves Count.
        profileHelper.UpdateHP(matchingUnit);

    }

    private void PrintDictionary() // Debug
    {
        foreach (var kvp in unitsDictionary)
        {
            Debug.Log($"Unit: {kvp.Key.unitTemplate.unitName} → Profile: {kvp.Value.name}");
        }
    }

    public void RefreshPartyMovesCounter()
    {
        foreach (var unit in _gameManager.playerPartyMembersInstances)
        {
            if (unit != null)
                UpdateRemainingMoves(unit.unitTemplate.unitName);
        }
    }

    public void CollectUpgradeFeedback()
    {
        // Retrieve ActivePlayerUnit
        var activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();
        Unit matchingUnit = LookForUnit(activePlayerUnit.unitTemplate.unitName);
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        // Add feedback to the matching profile container after collecting an upgrade
        profileHelper.CollectUpgradeFeedback();
    }

    public PlayerPartyProfileHelper RetrieveProfile(string unitName)
    {
        var activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit).GetComponent<Unit>();
        Unit matchingUnit = LookForUnit(activePlayerUnit.unitTemplate.unitName);
        PlayerPartyProfileHelper profileHelper = unitsDictionary[matchingUnit];
        return profileHelper;
    }
}
