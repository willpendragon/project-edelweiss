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
        unitEdel.unitFaithPoints += faithValue;
        string textNotification = $"Edel's Faith Value updated to" + faithValue;
        if (faithControllerDialogueUIHelper != null)
        {
            faithControllerDialogueUIHelper.DisplayTextNotification(textNotification);
        }
    }
}