using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static Unit;
public class TurnController : MonoBehaviour
{
    private static TurnController instance;
    [SerializeField] SummonResetHelper summonResetHelper;
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
    public static class Tags
    {
        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";
        public const string GAME_STATS_MANAGER = "GameStatsManager";
        public const string BATTLE_MANAGER = "BattleManager";
        public const string BOSS_CONTROLLER = "BossController";
        public const string END_TURN_BUTTON = "EndTurnButton";
        public const string PLAYER_PARTY_CONTROLLER = "PlayerPartyController";
        public const string ACTIVE_CHARACTER_UNIT_PROFILE = "ActiveCharacterUnitProfile";
    }

    public delegate void PlayerTurn(string enemyTurn);
    public static event PlayerTurn OnPlayerTurn;

    public delegate void EnemyTurn(string enemyTurn);
    public static event EnemyTurn OnEnemyTurn;

    public delegate void EnemyTurnSwap();
    public static event EnemyTurnSwap OnEnemyTurnSwap;

    public delegate void BattleEnd(string battleEndMessage);
    public static event BattleEnd OnBattleEnd;

    public int turnCounter;

    public Turn currentTurn;
    public GameObject[] playerUnitsOnBattlefield;
    public GameObject[] enemyUnitsOnBattlefield;

    public delegate void ResetSummonBuffs();
    public static event ResetSummonBuffs OnResetSummonBuffs;

    [Header("Core Gameplay Logic")]
    public GameStatsManager gameStatsManager;

    [Header("Battle System Elements")]

    public BattleManager battleManager;
    public BattleEndUIHandler battleEndUIHandler;
    public AchievementsManager achievementsManager;

    [Header("Gameplay Stats")]

    public float warFunds;
    public int enemiesKilledInCurrentBattle;
    public int timesSingleTargetSpellWasUsed;

    public void OnEnable() => SubscribeToEvents();
    public void OnDisable() => UnsubscribeFromEvents();

    private void SubscribeToEvents()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatusWrapper;
        BumperEnemyBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        StunnerEnemyBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        BossSimildeBehaviour.OnCheckPlayer += PlayerUnitsLifeCheck;
        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver += GameOverCheck;
    }
    private void UnsubscribeFromEvents()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatusWrapper;
        BumperEnemyBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        StunnerEnemyBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        BossSimildeBehaviour.OnCheckPlayer -= PlayerUnitsLifeCheck;
        EnemyTurnManager.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver -= GameOverCheck;
    }
    private void Start()
    {
        playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag(Tags.PLAYER_PARTY_CONTROLLER).GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        currentTurn = Turn.playerTurn;
        OnPlayerTurn("Player Turn");
        enemyUnitsOnBattlefield = GameObject.FindGameObjectsWithTag(Tags.ENEMY);
        gameStatsManager = GameObject.FindGameObjectWithTag(Tags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
        RestorePlayerUnitsOpportunityPoints();
    }
    private void CheckPlayerUnitsStatusWrapper()
    {
        // Call the method and, if the condition is validated, act on the result.
        bool allUnitsDone = CheckPlayerUnitsStatus();
        if (allUnitsDone)
        {
            SwapTurns();
        }
    }
    public void SwapTurns()
    {
        // If all units are dead or waiting, proceed to swap turns.
        if (CheckPlayerUnitsStatus())
        {
            OnEnemyTurn("Enemy Turn");
            OnEnemyTurnSwap();
        }
        else
        {
            Debug.Log("At least one player unit can still take actions.");
        }
    }
    public bool CheckPlayerUnitsStatus()
    {
        // Check if all units are either dead, waiting, or faithless
        return playerUnitsOnBattlefield.All(unitObject =>
        {
            Unit unit = unitObject.GetComponent<Unit>();
            var unitLifeCondition = unit.currentUnitLifeCondition;
            var selectionStatus = unit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus;
            var status = unit.GetComponent<UnitStatusController>().unitCurrentStatus;

            return unitLifeCondition == Unit.UnitLifeCondition.unitDead
                || selectionStatus == UnitSelectionController.UnitSelectionStatus.unitWaiting
                || status == UnitStatus.Faithless;
        });
    }

    public void PlayerUnitsLifeCheck()
    {
        // Check if there are any units that are NOT dead, indicating the player party is still active.
        bool isAnyPlayerUnitAlive = playerUnitsOnBattlefield.Any(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead);

        if (!isAnyPlayerUnitAlive) // If no units are alive, then the player party has been defeated.
        {
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
        else
        {
            Debug.Log("Player Party is still active");
        }
    }
    public void GameOverCheck()
    {
        // This method fires different handling of the Game Over sequence, depending on the Battle Type.
        switch (BattleTypeController.Instance.currentBattleType)
        {
            case BattleTypeController.BattleType.RegularBattle:
                HandleRegularBattle(gameStatsManager);
                break;

            case BattleTypeController.BattleType.BattleWithDeity:
                HandleBattleWithDeity(gameStatsManager);
                break;

            case BattleTypeController.BattleType.BossBattle:
                HandleBossBattle();
                break;

            default:
                Debug.LogWarning("Unknown battle type encountered.");
                break;
        }
    }

    public void FaithlessGameOverCheck()
    {
        if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().unitStatusController.unitCurrentStatus == UnitStatus.Faithless))
        {
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
    }

    private void HandleRegularBattle(GameStatsManager gameStatsManager)
    {
        // Handle the game over sequence in a Regular Battle against enemies.
        if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            BattleFlowController.Instance.PlayerPartyVictorySequence("Victory", warFunds);
        }
        else if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party is still in game");
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party is still in game");
        }
    }
    private void HandleBattleWithDeity(GameStatsManager gameStatsManager)
    {
        // Handle the game over sequence in a Battle against a Deity.
        if (GameObject.FindGameObjectWithTag(Tags.ENEMY).GetComponent<Unit>().unitHealthPoints <= 0)
        {
            BattleFlowController.Instance.PlayerPartyVictorySequence("Killed Deity", warFunds);
            Debug.Log("Deity's HP is over and Player won the battle. The Deity fled");
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated by the Deity");
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
    }
    private void HandleBossBattle()
    {
        BossController currentBossController = GameObject.FindGameObjectWithTag(Tags.BOSS_CONTROLLER).GetComponent<BossController>();
        if (currentBossController.bossUnit != null && currentBossController.bossUnit.HealthPoints <= 0)
        {
            Debug.Log("Boss Defeated");
            BattleFlowController.Instance.PlayerPartyVictorySequence("Boss Defeated", warFunds);
        }
    }
    public void RestorePlayerUnitsOpportunityPoints()
    {
        Debug.Log("Restoring Player Opportunity");
        foreach (var playerUnit in TurnController.Instance.playerUnitsOnBattlefield)
        {
            // Increases Turn Counter
            TurnController.Instance.turnCounter++;
            Unit playerUnitComponent = playerUnit.GetComponent<Unit>();
            playerUnitComponent.unitOpportunityPoints = playerUnitComponent.unitTemplate.unitOpportunityPoints;
            playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;

            // Hides Waiting Icons on Player Units
            playerUnit.GetComponent<UnitIconsController>().HideWaitingIcon();
        }

        // Try to find the End Turn button and enable it, if it exists
        GameObject endTurnButtonObject = GameObject.FindGameObjectWithTag(Tags.END_TURN_BUTTON);
        if (endTurnButtonObject != null)
        {
            Button endTurnButton = endTurnButtonObject.GetComponent<Button>();
            if (endTurnButton != null)
            {
                endTurnButton.interactable = true;
            }
        }
        else
        {
            Debug.LogWarning("End Turn button not found in the current scene.");
        }
    }
    public void RunFromBattle()
    {
        OnBattleEnd("Fleed");
        BattleManager.Instance.PlayCameraBattleEndAnimation();
        BattleFlowController.Instance.ResetBattleToInitialStatus();
        BattleManager.Instance.battleRewardsController.ApplyPartyRewardsAndSave(warFunds);
        BattleFlowController.Instance.UpdateBattleEndUIPanel(warFunds);
    }
}