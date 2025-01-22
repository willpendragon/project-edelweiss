using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static Unit;
using UnityEngine.Playables;
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

    // Add Delegate to manage battle end's buffs behaviour

    public delegate void ResetUnitUI();
    public static event ResetUnitUI OnResetUnitUI;

    public delegate void ResetSummonBuffs();
    public static event ResetSummonBuffs OnResetSummonBuffs;

    [Header("Battle System Elements")]

    public BattleManager battleManager;
    public BattleEndUIHandler battleEndUIHandler;
    public AchievementsManager achievementsManager;

    [Header("Gameplay Stats")]

    public float warFunds;
    public int enemiesKilledInCurrentBattle;
    public int timesSingleTargetSpellWasUsed;

    [Header("Camera Work")]

    [SerializeField] PlayableDirector mainCameraPlayableDirector;

    public void OnEnable()
    {
        UnitSelectionController.OnUnitWaiting += CheckPlayerUnitsStatus;
        BumperEnemyBehavior.OnCheckPlayer += PlayerGameOverCheck;
        StunnerEnemyBehavior.OnCheckPlayer += PlayerGameOverCheck;
        BossSimildeBehaviour.OnCheckPlayer += PlayerGameOverCheck;

        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap += RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver += GameOverCheck;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnUnitWaiting -= CheckPlayerUnitsStatus;
        BumperEnemyBehavior.OnCheckPlayer -= PlayerGameOverCheck;
        StunnerEnemyBehavior.OnCheckPlayer -= PlayerGameOverCheck;
        BossSimildeBehaviour.OnCheckPlayer -= PlayerGameOverCheck;

        EnemyTurnManager.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Deity.OnPlayerTurnSwap -= RestorePlayerUnitsOpportunityPoints;
        Unit.OnCheckGameOver -= GameOverCheck;
    }
    void Start()
    {
        currentTurn = Turn.playerTurn;
        playerUnitsOnBattlefield = GameObject.FindGameObjectsWithTag(Tags.PLAYER);
        enemyUnitsOnBattlefield = GameObject.FindGameObjectsWithTag(Tags.ENEMY);
        RestorePlayerUnitsOpportunityPoints();
    }

    public void CheckPlayerUnitsStatus()
    {
        // Get all Player Units
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag(Tags.PLAYER_PARTY_CONTROLLER).GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

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

            OnEnemyTurn("Enemy Turn");
            OnEnemyTurnSwap();
        }
        else
        {
            Debug.Log("At least one player unit can still take actions.");
        }
    }
    public void PlayerGameOverCheck()
    {
        // Check if there are any units that are NOT dead, indicating the player party is still active.
        bool isAnyPlayerUnitAlive = playerUnitsOnBattlefield.Any(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead);

        if (!isAnyPlayerUnitAlive) // If no units are alive, then the player party has been defeated.
        {
            Debug.Log("Player Party was defeated");
            OnBattleEnd("Defeat");
            PlayCameraBattleEndAnimation();
            ResetBattleToInitialStatus();
        }
        else
        {
            Debug.Log("Player Party is still active");
        }
    }
    public void GameOverCheck()
    {
        Debug.Log("Performing Game Over Check");
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(Tags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();

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
    private void HandleRegularBattle(GameStatsManager gameStatsManager)
    {
        if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party was defeated");
            OnBattleEnd("Victory");
            PlayCameraBattleEndAnimation();
            ResetBattleToInitialStatus();
            UnlockNextLevel();

            foreach (var player in playerUnitsOnBattlefield)
            {
                player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
                warFunds += player.GetComponent<Unit>().unitCoins;
            }

            foreach (var enemy in GameObject.FindGameObjectWithTag(Tags.BATTLE_MANAGER).GetComponent<BattleManager>().enemiesOnBattlefield)
            {
                if (enemy.tag == Tags.ENEMY && enemy.GetComponent<Unit>().currentUnitLifeCondition == UnitLifeCondition.unitDead)
                {
                    enemiesKilledInCurrentBattle++;
                    gameStatsManager.enemiesKilled++;
                    Debug.Log("Adding enemies to kill count");
                }
            }
            ApplyRewardsAndSave(gameStatsManager);
            Debug.Log("Rolling Convo Unlock");
            ConversationManager.Instance.UnlockRandomConversation();
            UpdateBattleEndUIPanel();
        }
        else if (enemyUnitsOnBattlefield.All(enemy => enemy.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party is still in game");
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated");
            OnBattleEnd("Defeat");
            ResetBattleToInitialStatus();
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party is still in game");
        }
    }
    private void HandleBattleWithDeity(GameStatsManager gameStatsManager)
    {
        if (GameObject.FindGameObjectWithTag(Tags.ENEMY).GetComponent<Unit>().unitHealthPoints <= 0)
        {
            Debug.Log("Deity's HP is over and Player won the battle. The Deity fled");
            OnBattleEnd("Victory");
            PlayCameraBattleEndAnimation();
            ResetBattleToInitialStatus();
            UnlockNextLevel();

            foreach (var player in playerUnitsOnBattlefield)
            {
                player.GetComponent<BattleRewardsController>().ApplyRewardsToThisUnit();
                warFunds += player.GetComponent<Unit>().unitCoins;
            }

            gameStatsManager.SaveCharacterData();
            gameStatsManager.SaveWarFunds(warFunds);
            gameStatsManager.SaveCaptureCrystalsCount();
            UpdateBattleEndUIPanel();
        }
        else if (playerUnitsOnBattlefield.All(player => player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated by the Deity");
            OnBattleEnd("Defeat");
            ResetBattleToInitialStatus();
        }
    }
    private void HandleBossBattle()
    {
        BossController currentBossController = GameObject.FindGameObjectWithTag(Tags.BOSS_CONTROLLER).GetComponent<BossController>();
        if (currentBossController.bossUnit != null && currentBossController.bossUnit.HealthPoints <= 0)
        {
            Debug.Log("Boss Defeated");
            OnBattleEnd("Victory");
            PlayCameraBattleEndAnimation();
            ResetBattleToInitialStatus();
        }
    }
    public void UnlockNextLevel()
    {
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.highestUnlockedLevel++;
        SaveStateManager.SaveGame(saveData);
        Debug.Log("Unlocking Next Level");
    }
    private void ApplyRewardsAndSave(GameStatsManager gameStatsManager)
    {
        // Applying to each Player's their Health Points, Coins and Experience Rewards Pool
        // Saving each Player's Health Points, Coins and Experience Rewards

        gameStatsManager.captureCrystalsCount += BattleManager.Instance.captureCrystalsRewardPool;
        gameStatsManager.SaveEnemiesKilled();
        gameStatsManager.SaveCharacterData();
        gameStatsManager.SaveWarFunds(warFunds);
        gameStatsManager.SaveUsedSingleTargetSpells();
        gameStatsManager.SaveCaptureCrystalsCount();
        Debug.Log("Saving Character Stats Data");
    }
    private void ResetBattleToInitialStatus()
    {
        ResetTags();
        OnResetUnitUI();
        DeactivateActivePlayerUnitPanel();
        TurnController.Instance.summonResetHelper.ResetSummonTemporaryBuffs();
    }
    public void ResetTags()
    {
        foreach (var player in GameManager.Instance.playerPartyMembersInstances)
        {
            player.gameObject.tag = Tags.PLAYER;
        }
    }

    private void DeactivateActivePlayerUnitPanel()
    {
        Destroy(GameObject.FindGameObjectWithTag(Tags.ACTIVE_CHARACTER_UNIT_PROFILE));
    }

    private void UpdateBattleEndUIPanel()

    {
        battleEndUIHandler.battleEndEnemiesKilledText.text = enemiesKilledInCurrentBattle.ToString();
        battleEndUIHandler.battleEndWarFundsGainedText.text = warFunds.ToString();
        battleEndUIHandler.battleEndCrystalObtainedText.text = battleManager.captureCrystalsRewardPool.ToString();
    }
    public void EndTurnViaButton()
    {
        // Check if it's Player Turn and no Active Unit is in play

        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();

        if (turnController.currentTurn == Turn.playerTurn)
        {
            playerUnitsOnBattlefield = turnController?.playerUnitsOnBattlefield;
            foreach (var playerUnit in playerUnitsOnBattlefield)
            {
                playerUnit?.GetComponent<UnitSelectionController>()?.StopUnitAction();
                playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitWaiting;
            }

            Button endTurnButton = GameObject.FindGameObjectWithTag(Tags.END_TURN_BUTTON).GetComponent<Button>();
            endTurnButton.interactable = false;
            CheckPlayerUnitsStatus();
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
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(Tags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
        Debug.Log("Player Party ran away.");
        OnBattleEnd("Fleed");
        PlayCameraBattleEndAnimation();
        ResetBattleToInitialStatus();
        gameStatsManager.SaveCharacterData();
        gameStatsManager.SaveWarFunds(warFunds);
        gameStatsManager.SaveCaptureCrystalsCount();
        UpdateBattleEndUIPanel();
    }

    private void PlayCameraBattleEndAnimation()
    {
        if (mainCameraPlayableDirector != null)
        {
            mainCameraPlayableDirector.Play();
        }
    }
}