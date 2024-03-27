using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using static Unit;

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
        StunnerEnemyBehavior.OnCheckPlayer += PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver += GameOverCheck;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
        StunnerEnemyBehavior.OnCheckPlayer -= PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver -= GameOverCheck;
    }
    void Start()
    {
        currentTurn = Turn.playerTurn;
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Player");
        enemyUnitsOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
        SetUnitsInitialPositionOnGrid();
    }

    public void SetUnitsInitialPositionOnGrid()
    {
        Debug.Log("Moving Player Units at Initial Position");

        foreach (var playerUnitGO in playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            playerUnit.GetComponent<Unit>().MoveUnit(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
            playerUnit.GetComponent<Unit>().SetPosition(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
            Debug.Log("Moving Player Units at Initial Position");
        }
        foreach (var enemyUnitGO in enemyUnitsOnBattlefield)
        {
            Unit enemyUnit = enemyUnitGO.GetComponent<Unit>();
            enemyUnit.GetComponent<Unit>().MoveUnit(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);
            enemyUnit.GetComponent<Unit>().SetPosition(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);
            Debug.Log("Moving Player Units at Initial Position");
        }

        RestorePlayerUnitsStatus();
    }

    public void RestorePlayerUnitsStatus()
    {
        foreach (var playerUnitGO in playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            playerUnit.currentUnitLifeCondition = Unit.UnitLifeCondition.unitAlive;
            playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
            playerUnit.GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            playerUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.basic;
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

    public void CheckPlayerUnitsStatus()
    {
        if (playerUnitsOnBattlefield.All(player => player.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting))
        {
            //Disable Player UI
            ApplyTrapEffects();
            OnEnemyTurn("Enemy Turn");
            OnEnemyTurnSwap();
            Debug.Log("Player Turn is over. Hand over turn to the Enemy Party");
        }
    }

    public void ApplyTrapEffects()
    {
        //Need to move this in another class or move in a class of its own, following the single responsibility principle

        foreach (var tile in GridManager.Instance.gridTileControllers)
        {
            TrapController trapTile = tile.GetComponent<TrapController>();
            if (trapTile != null && trapTile.currentTrapActivationStatus == TrapController.TrapActivationStatus.active)
            {
                trapTile.ApplyTrapEffect();
            }
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
            ResetTags();
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
            ResetTags();
            UnlockNextLevel();
            foreach (var player in playerUnitsOnBattlefield)
            {
                player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
            }
            GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
            foreach (var enemy in GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().enemiesOnBattlefield)
            {
                if (enemy.tag == "Enemy" && enemy.GetComponent<Unit>().currentUnitLifeCondition == UnitLifeCondition.unitDead)
                {
                    gameStatsManager.enemiesKilled++;
                    Debug.Log("Adding enemies to kill count");
                }
            }

            gameStatsManager.SaveEnemiesKilled();
            //Applying to each Player's their Health Points, Coins and Experience Rewards Pool
            GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>().SaveCharacterData();
            //Saving each Player's Health Points, Coins and Experience Rewards

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
            ResetTags();
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

    public void ResetTags()
    {
        foreach (var player in GameManager.Instance.playerPartyMembersInstances)
        {
            player.gameObject.tag = "Player";
        }
    }

    public void UnlockNextLevel()
    {
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.highestUnlockedLevel++;
        SaveStateManager.SaveGame(saveData);
        Debug.Log("Unlocking Next Level");
    }
}
