using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    void Start()
    {
        currentTurn = Turn.playerTurn;
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Player");
    }
    public void CheckPlayerPartyOpportunityPoints()
    {
        for (int i = 0; i < playerUnitsOnBattlefield.Length; i++)
        {
            if (playerUnitsOnBattlefield[i].GetComponent<Unit>().opportunityPoints >= 1)
            {
                Debug.Log("Player Turn is still active");
            }
            else
            {
                Debug.Log("Hand over turn to the Enemy Party");
            }
        }
    }

}
