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
    public delegate void EnemyTurn(string enemyTurn);
    public static event EnemyTurn OnEnemyTurn;
    public delegate void EnemyTurnSwap();
    public static event EnemyTurnSwap OnEnemyTurnSwap;

    public Turn currentTurn;
    public GameObject[] playerUnitsOnBattlefield;

    // Start is called before the first frame update

    public void OnEnable()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatus;
        EnemyAgent.OnCheckPlayer += PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurn += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurn += RestorePlayerUnitsOpportunityPoints;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
        EnemyAgent.OnCheckPlayer -= PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurn -= RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurn -= RestorePlayerUnitsOpportunityPoints;
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
            //Disable Player UI
            OnEnemyTurn("Enemy Turn");
            OnEnemyTurnSwap();
            Debug.Log("Player Turn is over. Hand over turn to the Enemy Party");
        }
    }

    public void PlayerGameOverCheck()
    {
        if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated");
            //Activate Game Over UI
            //Active Game Over Flow
        }
    }
    public void RestorePlayerUnitsOpportunityPoints()
    {
        Debug.Log("Restoring Player Opportunity");
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            Unit playerUnitComponent = playerUnit.GetComponent<Unit>();
            playerUnitComponent.unitOpportunityPoints = playerUnitComponent.unitTemplate.unitOpportunityPoints;
            playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
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
