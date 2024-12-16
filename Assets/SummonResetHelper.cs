using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonResetHelper : MonoBehaviour
{
    public void ResetSummonTemporaryBuffs()
    {
        GameObject[] playerUnitsOnBattlefield = TurnController.Instance.playerUnitsOnBattlefield;

        foreach (var unit in playerUnitsOnBattlefield)
        {
            unit.GetComponent<Unit>().unitShieldPoints = unit.GetComponent<Unit>().unitTemplate.unitShieldPoints;
        }
    }
}