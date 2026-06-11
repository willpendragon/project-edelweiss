using System.Collections.Generic;
using UnityEngine;
public class FaithControllerDialogueHelper : MonoBehaviour
{
    public List<Unit> playerUnits;
    int faithPointsBuff;
    int faithPointsDebuff;
    public Unit unitAliza;
    public Unit unitEdel;
    public Unit unitViolet;

    public FaithControllerDialogueUIHelper faithControllerDialogueUIHelper;
    private void Start()
    {
        playerUnits = new List<Unit>();
        if (playerUnits.Count <= 0)
        {
            {
                GameObject[] playerUnitsInCafe = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject playerUnitGameObject in playerUnitsInCafe)
                {
                    Unit playerUnit = playerUnitGameObject.GetComponent<Unit>();
                    playerUnits.Add(playerUnit);
                }
            }
            SortUnitsForDialogueInteractions();
        }
    }
    private void SortUnitsForDialogueInteractions()
    {
        foreach (Unit playerUnit in playerUnits)
        {
            string playerUnitName = playerUnit.unitTemplate.unitName;
            AssignUnits(playerUnitName, playerUnit);
        }
    }
    private void AssignUnits(string playerName, Unit playerUnit)
    {
        switch (playerName)
        {
            case "Aliza":
                unitAliza = playerUnit;
                break;
            case "Edelweiss":
                unitEdel = playerUnit;
                break;
            case "Violet":
                unitViolet = playerUnit;
                break;
        }
    }
    public void ManageEdelweissFaith(int faithValue)
    {
        if (faithValue == 0) return;

        unitEdel.unitFaithPoints += faithValue;

        string textNotification;

        if (faithValue > 0)
        {
            textNotification = $"Edel's Faith increased by {faithValue}";
        }
        else // faithValue < 0
        {
            textNotification = $"Edel's Faith suffered a {Mathf.Abs(faithValue)} point reduction";
        }

        if (faithControllerDialogueUIHelper != null)
        {
            faithControllerDialogueUIHelper.DisplayTextNotification(textNotification);
        }
        SaveFaithStat();
    }
    private void SaveFaithStat()
    {
        GameSaveData characterSaveData = SaveStateManager.saveData;

        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            CharacterData existingCharacterData = characterSaveData.characterData.Find(character => character.unitId == playerUnit.Id);
            if (existingCharacterData != null)
            {
                // Update existing character data
                existingCharacterData.unitFaithPoints = playerUnit.unitFaithPoints;

                // Update other stats as necessary
            }
        }
        SaveStateManager.SaveGame(characterSaveData);
    }
}