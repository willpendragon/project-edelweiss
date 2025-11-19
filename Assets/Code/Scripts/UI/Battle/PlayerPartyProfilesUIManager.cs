using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerPartyProfilesUIManager : MonoBehaviour
{

    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _playerProfilePrefab;
    [SerializeField] private RectTransform _playerProfileContainer;
    [SerializeField] private Dictionary<Unit, PlayerPartyProfileHelper> unitsDictionary = new Dictionary<Unit, PlayerPartyProfileHelper>();

    void Start()
    {
        _gameManager = GameManager.Instance;
        RetrievePlayerUnits();
    }
    private void RetrievePlayerUnits()
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
        Unit matchingUnit = unitsDictionary.Keys
            .FirstOrDefault(u => u.unitTemplate.unitName.Equals(unitName, StringComparison.OrdinalIgnoreCase));

        if (matchingUnit == null)
        {
            Debug.Log($"The Unit {unitName} wasn't found in the dictionary.");
            return;
        }
        // Retrieve the corresponding Profile.
        PlayerPartyProfileHelper profilehelper = unitsDictionary[matchingUnit];
        // Refresh Profile UI.
        profilehelper.FillPlayerDetails(matchingUnit);
    }

    private void PrintDictionary() // Debug
    {
        foreach (var kvp in unitsDictionary)
        {
            Debug.Log($"Unit: {kvp.Key.unitTemplate.unitName} → Profile: {kvp.Value.name}");
        }
    }

}
