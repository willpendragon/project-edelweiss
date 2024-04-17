using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using static Unit;

public class TurnController : MonoBehaviour
{
    private static TurnController instance;
    public static TurnController Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TurnController>();
            return instance;
        }
    }
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

    public float warFunds;
    public int enemiesKilledInCurrentBattle;
    public BattleManager battleManager;
    public BattleEndUIHandler battleEndUIHandler;
    public int timesSingleTargetSpellWasUsed;
    public AchievementsManager achievementsManager;

    public void OnEnable()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatus;
        BumperEnemyBehavior.OnCheckPlayer += PlayerGameOverCheck;
        StunnerEnemyBehavior.OnCheckPlayer += PlayerGameOverCheck;
        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver += GameOverCheck;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
        BumperEnemyBehavior.OnCheckPlayer -= PlayerGameOverCheck;
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
        RestorePlayerUnitsOpportunityPoints();
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
        // Get all player units
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        // Check if all units are either dead or waiting (which means no unit is in a state that can take action)
        bool allUnitsDeadOrWaiting = playerUnitsOnBattlefield.All(unitObject =>
        {
            Unit unit = unitObject.GetComponent<Unit>();
            return unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead || unit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting;
        });

        // If all units are dead or waiting, we proceed to swap turns
        if (allUnitsDeadOrWaiting)
        {
            Debug.Log("All units are either dead or waiting. Swapping to enemy turn.");
            // Disable Player UI
            ApplyTrapEffects();
            OnEnemyTurn("Enemy Turn");
            OnEnemyTurnSwap();
        }
        else
        {
            Debug.Log("At least one player unit can still take actions.");
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
        // Check if there are any units that are NOT dead, indicating the player party is still active.
        bool isAnyPlayerUnitAlive = playerUnitsOnBattlefield.Any(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead);

        if (!isAnyPlayerUnitAlive) // If no units are alive, then the player party has been defeated.
        {
            Debug.Log("Player Party was defeated");
            OnBattleEnd("Player Party was defeated");
            // Activate Game Over UI
            // Active Game Over Flow
            ResetTags();
            DeactivateActivePlayerUnitPanel();
        }
        else
        {
            Debug.Log("Player Party is still active");
        }
    }
    public void GameOverCheck()
    {
        if (battleManager.currentBattleType == BattleType.regularBattle)
        {
            if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
            {
                Debug.Log("Enemy Party was defeated");
                OnBattleEnd("Victory");
                ResetTags();
                DeactivateActivePlayerUnitPanel();
                UnlockNextLevel();
                foreach (var player in playerUnitsOnBattlefield)
                {
                    player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
                    warFunds += player.GetComponent<Unit>().unitCoins;
                }
                GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
                foreach (var enemy in GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().enemiesOnBattlefield)
                {
                    if (enemy.tag == "Enemy" && enemy.GetComponent<Unit>().currentUnitLifeCondition == UnitLifeCondition.unitDead)
                    {
                        enemiesKilledInCurrentBattle++;
                        gameStatsManager.enemiesKilled++;
                        Debug.Log("Adding enemies to kill count");
                    }
                }

                gameStatsManager.SaveEnemiesKilled();
                gameStatsManager.SaveCharacterData();
                gameStatsManager.SaveWarFunds(warFunds);
                gameStatsManager.SaveUsedSingleTargetSpells();
                Debug.Log("Saving Character Stats Data");

                UpdateBattleEndUIPanel();

                //Applying to each Player's their Health Points, Coins and Experience Rewards Pool
                //Saving each Player's Health Points, Coins and Experience Rewards

                //Activate Game Over UI
                //Active Game Over Flow
            }
            else if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
            {
                Debug.Log("Enemy Party is still in game");
            }
            else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
            {
                Debug.Log("Player Party was defeated");
                OnBattleEnd("Defeat");
                ResetTags();
                DeactivateActivePlayerUnitPanel();
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
        else if (battleManager.currentBattleType == BattleType.battleWithDeity)
        {
            if (GameObject.FindGameObjectWithTag("Enemy").GetComponent<Unit>().unitHealthPoints <= 0)
            {
                GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();

                Debug.Log("Deity's HP is over and Player won the battle. The Deity fled");
                OnBattleEnd("Victory");
                ResetTags();
                DeactivateActivePlayerUnitPanel();
                UnlockNextLevel();
                foreach (var player in playerUnitsOnBattlefield)
                {
                    player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
                    warFunds += player.GetComponent<Unit>().unitCoins;
                }
                gameStatsManager.SaveCharacterData();
                gameStatsManager.SaveWarFunds(warFunds);
                UpdateBattleEndUIPanel();
            }
            else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
            {
                Debug.Log("Player Party was defeated by the Deity");
                OnBattleEnd("Defeat");
                ResetTags();
                DeactivateActivePlayerUnitPanel();
            }
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

    public void DeactivateActivePlayerUnitPanel()
    {
        Destroy(GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile"));
    }

    public void UpdateBattleEndUIPanel()

    {
        battleEndUIHandler.battleEndEnemiesKilledText.text = enemiesKilledInCurrentBattle.ToString();
        battleEndUIHandler.battleEndWarFundsGainedText.text = warFunds.ToString();
    }

    public void RunFromBattle()
    {
        Debug.Log("Player Party was defeated");
        OnBattleEnd("Fleed");
        ResetTags();
        DeactivateActivePlayerUnitPanel();
        //Activate Game Over UI
        //Active Game Over Flow
    }

}