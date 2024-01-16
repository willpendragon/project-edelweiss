using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class TurnController : MonoBehaviour
{
    public enum Turn
    {
        playerTurn,
        enemyTurn,
        deityTurn

    }

    public Turn currentTurn;
    public GameObject[] playerUnitsOnBattlefield;

    // Start is called before the first frame update

    public void OnEnable()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatus;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
    }
    void Start()
    {
        currentTurn = Turn.playerTurn;
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Player");
    }
    public void CheckPlayerUnitsStatus()
    {
        if (playerUnitsOnBattlefield.All(player => player.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting))
        {
            Debug.Log("Player Turn is over. Hand over turn to the Enemy Party");
        }
    }
    /*
    public void CheckPlayerPartyOpportunityPoints()
    {
        for (int i = 0; i < playerUnitsOnBattlefield.Length; i++)
        {
            if (playerUnitsOnBattlefield[i].GetComponent<Unit>().unitOpportunityPoints >= 1)
            {
                Debug.Log("Player Turn is still active");
            }
            else
            {
                Debug.Log("Hand over turn to the Enemy Party");
            }
        }
    }
    */

}
