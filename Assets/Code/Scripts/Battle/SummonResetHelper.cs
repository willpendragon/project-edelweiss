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
            // Caveat: the following line resets all of the characters possible summon feedback. 
            unit.GetComponentInChildren<TileShaderController>()?.AnimateFadeHeight(0f, 0, Color.cyan);

        }
    }
}