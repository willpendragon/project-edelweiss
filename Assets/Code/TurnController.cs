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

    public int turnCounter;

    public Turn currentTurn;
    public GameObject[] playerUnitsOnBattlefield;
    public GameObject[] enemyUnitsOnBattlefield;

    public delegate void BattleEnd(string battleEndMessage);
    public static event BattleEnd OnBattleEnd;

    // Start is called before the first frame update

    public void OnEnable()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatus;
        EnemyAgent.OnCheckPlayer += PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver += GameOverCheck;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
        EnemyAgent.OnCheckPlayer -= PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver -= GameOverCheck;

    }
    void Start()
    {
        currentTurn = Turn.playerTurn;
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Player");
        enemyUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
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
            OnBattleEnd("Player Party was defeated");
            //Activate Game Over UI
            //Active Game Over Flow
        }
        else
        {
            Debug.Log("Player Party is still active");
        }
    }

    public void GameOverCheck()
    {
        if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party was defeated");
            OnBattleEnd("Enemy Party was defeated");
            //Activate Game Over UI
            //Active Game Over Flow
        }
        else if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party is still in game");
            //Activate Game Over UI
            //Active Game Over Flow
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated");
            OnBattleEnd("Player Party was defeated");
            //Activate Game Over UI
            //Active Game Over Flow
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party is still in game");
            //Activate Game Over UI
            //Active Game Over Flow
        }
    }
    public void RestorePlayerUnitsOpportunityPoints()
    {
        Debug.Log("Restoring Player Opportunity");
        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            turnCounter++;
            Unit playerUnitComponent = playerUnit.GetComponent<Unit>();
            playerUnitComponent.unitOpportunityPoints = playerUnitComponent.unitTemplate.unitOpportunityPoints;
            playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
            playerUnit.GetComponent<UnitSelectionController>().unitSprite.material.color = Color.white;
        }
    }
}
