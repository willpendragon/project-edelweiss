using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BuffDisplayUIManager : MonoBehaviour
{
    [SerializeField] private List<Unit> _partyMembers;
    private Dictionary<string, GameObject> _partyMembersIconsDictionary = new Dictionary<string, GameObject>();
    [SerializeField] private GameObject _iconTemplate;
    [SerializeField] private RectTransform _unitsContainer;

    void Start()
    {
        // Cache Party Members
        _partyMembers = GameManager.Instance.playerPartyMembersInstances;
        CreateUnitsOnUI();
        DisplayBuffOnCharacters();
        DisplayDeityOnCharacters();
    }
    private void CreateUnitsOnUI()
    {
        //_partyMembersIconsDictionary.Clear();

        foreach (Unit unit in _partyMembers)
        {
            if (unit != null)
            {
                string unitName = unit.unitTemplate.unitName;
                // Instantiate Visual Representation
                SpriteRenderer unitIcon = unit.gameObject.GetComponentInChildren<SpriteRenderer>();
                GameObject unitUIIcon = SpawnUnitIcon(unitIcon);
                // Create Entry in the Dictionary
                _partyMembersIconsDictionary.Add(unitName, unitUIIcon);
            }
        }
    }

    public GameObject SpawnUnitIcon(SpriteRenderer spriteIcon)
    {
        GameObject unitRepresentation = Instantiate(_iconTemplate, _unitsContainer);
        var iconHelper = unitRepresentation.GetComponent<UIUnitIconHelper>();
        iconHelper.UnitUIIcon.sprite = spriteIcon.sprite;
        // Add the UI icon to the GameObject, use an Helper Class
        return unitRepresentation;
    }

    private void DisplayDeityOnCharacters()
    {
        foreach (Unit unit in _partyMembers)
        {
            if (unit.linkedDeity == null) continue;
            {
                string name = unit.unitTemplate.unitName;
                if (_partyMembersIconsDictionary.TryGetValue(name, out GameObject uiIcon))
                {
                    var deitySprite = unit.linkedDeity.gameObject.GetComponent<Unit>().unitTemplate.unitPortrait;
                    var iconHelper = uiIcon.GetComponent<UIUnitIconHelper>();
                    iconHelper.ShowDeityIcon(deitySprite);
                    // Display on uiIcon, possibly have an helper class with a slot for the Deity representation
                    // Display Deity Animation
                }
            }
        }

        // Check the existing party stats
        // If Buffs Exist, show buff value on the corresponding character icon
    }

private void DisplayBuffOnCharacters()
{
    foreach (Unit unit in _partyMembers)
    {
        if (unit == null) continue;

        // 1. Get the controller
        var buffController = unit.GetComponent<UnitBuffController>();
        if (buffController == null) continue;

        // 2. Get the active buffs
        var activeBuffs = buffController.GetActiveBuffs();

        // 3. Find the UI icon for this unit
        string unitName = unit.unitTemplate.unitName;
        if (_partyMembersIconsDictionary.TryGetValue(unitName, out GameObject uiIcon))
        {
            var iconHelper = uiIcon.GetComponent<UIUnitIconHelper>();

            // 4. Loop through each buff type (Attack, Defense, etc.)
            foreach (var kvp in activeBuffs)
            {
                FoodBuff.FoodBuffType type = kvp.Key;
                List<UnitBuffController.AppliedBuffEntry> entries = kvp.Value;

                // Calculate total value if there are multiple buffs of the same type
                float totalValue = 0;
                foreach (var entry in entries)
                {
                    totalValue += entry.AppliedValue;
                }

                // 5. Send data to UI Helper
                // You will need to create this 'SetBuffDisplay' method in your Helper class
                iconHelper.SetBuffDisplay(type, totalValue);
            }
        }
    }
}
}
