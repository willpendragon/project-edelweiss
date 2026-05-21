using DG.Tweening;
using System;
using System.Linq;
using ProjectEdelweiss.Utils;
using UnityEngine;
using UnityEngine.UI;
using static Unit;

public class TurnController : MonoBehaviour
{
    public static TurnController instance;
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
        PlayerTurn,
        EnemyTurn
    }

    public static class Tags
    {
        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";
        public const string ACTIVE_PLAYER_UNIT = "ActivePlayerUnit";
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

    public bool battleStarted;

    [Header("Core Gameplay Logic")] public GameStatsManager gameStatsManager;

    [Header("Battle System Elements")] public BattleManager battleManager;
    public BattleEndUIHandler battleEndUIHandler;
    public AchievementsManager achievementsManager;
    [SerializeField] private DeitySpawner _deitySpawner;
    [SerializeField] private UnitSelectionController _unitSelectionController;

    [Header("Gameplay Stats")] public float warFunds;
    public int enemiesKilledInCurrentBattle;
    public int timesSingleTargetSpellWasUsed;

    public delegate void DeityKilled(Deity deity);

    public static event DeityKilled OnDeityKilled;

    public void OnEnable() => SubscribeToEvents();
    public void OnDisable() => UnsubscribeFromEvents();

    private void SubscribeToEvents()
    {
        UnitSelectionController.OnUnitTurnEnded += DecideTurn;
        BumperEnemyBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        StunnerEnemyBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        RockEnemyBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        DeityKingLaurinusBehavior.OnCheckPlayer += PlayerUnitsLifeCheck;
        EnemyTurnManager.OnPlayerTurnSwap += RestorePlayerUnits;
        Deity.OnPlayerTurnSwap += RestorePlayerUnits;
        Unit.OnCheckGameOver += GameOverCheck;
    }

    private void UnsubscribeFromEvents()
    {
        UnitSelectionController.OnUnitTurnEnded -= DecideTurn;
        BumperEnemyBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        StunnerEnemyBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        RockEnemyBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        DeityKingLaurinusBehavior.OnCheckPlayer -= PlayerUnitsLifeCheck;
        EnemyTurnManager.OnPlayerTurnSwap -= RestorePlayerUnits;
        Deity.OnPlayerTurnSwap -= RestorePlayerUnits;
        Unit.OnCheckGameOver -= GameOverCheck;
    }

    private void Start()
    {
        // Execute immediately so UI logic (like HP bars) can bind!
        RetrieveUnits();
        RestorePlayerUnits();
        battleStarted = true;

        // Start the check in a sequestered coroutine
        StartCoroutine(StartTurnationCheckCoroutine());
    }

    private System.Collections.IEnumerator StartTurnationCheckCoroutine()
    {
        yield return null;

        if (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            // Halt ONLY the turn assignment!
            yield return new WaitUntil(() => !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive);
        }

        // Only after dialogue finishes do we decide who actually attacks.
        DecideTurn();
    }

    private void RetrieveUnits()
    {
        playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag(Tags.PLAYER_PARTY_CONTROLLER)
            .GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;
        enemyUnitsOnBattlefield = GameObject.FindGameObjectsWithTag(Tags.ENEMY);
        gameStatsManager = GameObject.FindGameObjectWithTag(Tags.GAME_STATS_MANAGER).GetComponent<GameStatsManager>();
    }

    private void SetTurn(Turn turn)
    {
        currentTurn = turn;
    }

    public void DecideTurn()
    {
        // If all Player Units are Waiting or all Dead except one, proceed to swap turns.
        if (PlayerPartyAvailable())
        {
            SetTurn(Turn.PlayerTurn);
        }
        else
        {
            SetTurn(Turn.EnemyTurn);
        }

        AssignTurn();
    }

    private void AssignTurn()
    {
        switch (currentTurn)
        {
            case Turn.PlayerTurn:
                StartPlayerTurn();
                break;
            case Turn.EnemyTurn:
                StartEnemyTurn();
                break;
        }
    }

    private void StartPlayerTurn()
    {
        // Send Player Turn UI notification.
        OnPlayerTurn("Player Turn");
        // Allow the Player to select characters.
        SetPlayerUnitsToActive();
    }

    private void SetPlayerUnitsToActive()
    {
        foreach (var unitGO in playerUnitsOnBattlefield)
        {
            unitGO.GetComponent<Unit>().currentUnitPhase = UnitPhase.Active;
        }
    }

    private void StartEnemyTurn()
    {
        OnEnemyTurn("Enemy Turn");
        BattleSFXManager.PlaySound(SoundType.NEXTTURN);
        DOVirtual.DelayedCall(1.5f, () => OnEnemyTurnSwap?.Invoke());
    }

    public bool PlayerPartyAvailable()
    {
        if (playerUnitsOnBattlefield.All(unitGO =>
                unitGO.GetComponent<Unit>().currentUnitLifeCondition == UnitLifeCondition.unitDead
                || unitGO.GetComponent<Unit>().currentUnitPhase == UnitPhase.Waiting
                || unitGO.GetComponent<UnitStatusController>().unitCurrentStatus == UnitStatus.Faithless))
            return false;
        else
            return true;
    }

    public void PlayerUnitsLifeCheck()
    {
        // Check if there are any units that are NOT dead, indicating the Player Party is still active.
        bool isAnyPlayerUnitAlive = playerUnitsOnBattlefield.Any(player =>
            player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead);

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
        // Fires different handling of the Game Over sequence, depending on the Battle Type.
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

            case BattleTypeController.BattleType.PuzzleBattle:
                HandlePuzzleBattle(gameStatsManager);
                break;

            default:
                Debug.LogWarning("Unknown battle type encountered.");
                break;
        }
    }

    public void FaithlessGameOverCheck()
    {
        if (playerUnitsOnBattlefield.All(player =>
                player.GetComponent<Unit>().unitStatusController.unitCurrentStatus == UnitStatus.Faithless))
        {
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
    }

    private void HandleRegularBattle(GameStatsManager gameStatsManager)
    {
        if (enemyUnitsOnBattlefield.All(enemy =>
                enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            BattleFlowController.Instance.PlayerPartyVictorySequence("Victory", warFunds);
        }
        else if (enemyUnitsOnBattlefield.All(enemy =>
                     enemy.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Enemy Party is still in game");
        }
        else if (playerUnitsOnBattlefield.All(player =>
                     player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
        else if (playerUnitsOnBattlefield.All(player =>
                     player.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party is still in game");
        }
    }

    private void HandleBattleWithDeity(GameStatsManager gameStatsManager)
    {
        if (GameObject.FindGameObjectWithTag(Tags.ENEMY).GetComponent<Unit>().unitHealthPoints <= 0)
        {
            BattleFlowController.Instance.PlayerPartyVictorySequence("Deicide", warFunds);
            // Add Deity to the Killed Deity Dictionary
            OnDeityKilled(_deitySpawner.currentUnboundDeity);
            Debug.Log("Deity's HP is over and Player won the battle. The Deity fled");
        }
        else if (playerUnitsOnBattlefield.All(player =>
                     player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead))
        {
            Debug.Log("Player Party was defeated by the Deity");
            BattleFlowController.Instance.PlayerPartyDefeatSequence();
        }
    }

    private void HandleBossBattle()
    {
        //BossController currentBossController = GameObject.FindGameObjectWithTag(Tags.BOSS_CONTROLLER).GetComponent<BossController>();
        //if (currentBossController.bossUnit != null && currentBossController.bossUnit.HealthPoints <= 0)
        //{
        //    Debug.Log("Boss Defeated");
        //    BattleFlowController.Instance.PlayerPartyVictorySequence("Boss Defeated", warFunds);
        //}
    }

        private void HandlePuzzleBattle(GameStatsManager gameStatsManager)
        {
            // Try to find the Puzzle Deity / Resident Deity in the scene dynamically
            GameObject deityObject = GameObject.FindGameObjectWithTag(GameTags.Deity);
            Unit residentDeityUnit = deityObject != null ? deityObject.GetComponent<Unit>() : null;

            bool isResidentDeityPresent = residentDeityUnit != null;
            bool isDeityBoss = isResidentDeityPresent && residentDeityUnit.unitType == Unit.UnitType.DeityBoss;
            bool residentDeityDefeated = false;

            if (isResidentDeityPresent)
            {
                if (residentDeityUnit.currentUnitLifeCondition == UnitLifeCondition.unitDead || residentDeityUnit.HealthPoints <= 0)
                {
                    residentDeityDefeated = true;
                }
            }

            // Check if all standard enemies are dead
            bool allEnemiesDefeated = enemyUnitsOnBattlefield.All(enemy =>
                enemy != null && enemy.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead);

            bool playerPartyDefeated = playerUnitsOnBattlefield.All(player =>
                player.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead);

            if (playerPartyDefeated)
            {
                BattleFlowController.Instance.PlayerPartyDefeatSequence();
                return;
            }

            // Victory conditions
            if (isResidentDeityPresent)
            {
                if (isDeityBoss)
                {
                    // Scenario 1 & 3: Resident deity is a killable DeityBoss.
                    // Victory triggers ONLY when it is defeated. Regular enemies' status doesn't matter.
                    if (residentDeityDefeated)
                    {
                        BattleFlowController.Instance.PlayerPartyVictorySequence("Victory", warFunds);
                    }
                }
                else
                {
                    // Scenario: Resident deity is an untouchable normal Deity. 
                    // Victory triggers when all standard enemies are dead.
                    if (allEnemiesDefeated)
                    {
                        BattleFlowController.Instance.PlayerPartyVictorySequence("Victory", warFunds);
                    }
                }
            }
            else
            {
                // Scenario 2: Resident deity is NOT present at all. 
                // Victory triggers when all standard enemies are dead.
                if (allEnemiesDefeated)
                {
                    BattleFlowController.Instance.PlayerPartyVictorySequence("Victory", warFunds);
                }
            }
        }

    public void RestorePlayerUnits()
    {
        currentTurn = Turn.PlayerTurn;
        foreach (var playerUnit in TurnController.Instance.playerUnitsOnBattlefield)
        {
            // TurnController.Instance.turnCounter++;
            Unit playerUnitComponent = playerUnit.GetComponent<Unit>();
            if (playerUnitComponent.currentUnitLifeCondition != UnitLifeCondition.unitDead)
            {
                playerUnitComponent.unitOpportunityPoints = playerUnitComponent.unitTemplate.unitOpportunityPoints;
                playerUnit.GetComponent<UnitIconsController>().HideWaitingIcon();
            }
        }

        // Refresh the remaining Moves on the UI.
        DOVirtual.DelayedCall(0.1f,
            () => { BattleInterface.Instance.PlayerPartyProfilesUIManager.RefreshPartyMovesCounter(); });

        // Decrease the Deity Move Cooldowns.
        BattleInterface.Instance.PlayerPartyProfilesUIManager.TickDeityCooldowns();

        RestoreActivePlayerUnit();
        SetPlayerUnitsToActive();

        // Try to find the End Turn button and enable it, if it exists.
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

    private void RestoreActivePlayerUnit()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag(Tags.ACTIVE_PLAYER_UNIT);

        if (activePlayerUnit == null)
            return;

        _unitSelectionController.SpawnUnitInfoPanel(activePlayerUnit.GetComponent<Unit>());
        // Display the Attackable Enemies outline.
        _unitSelectionController.OutlineAttackableEnemies(activePlayerUnit.GetComponent<Unit>());
        // Display the tiles reachable by the Active Player Unit.
        _unitSelectionController.tileVisualizer.ShowReachableTiles();
    }

    public void RunFromBattle()
    {
        OnBattleEnd("Fleed");
        BattleFlowController.Instance.ResetBattleToInitialStatus();
        BattleManager.Instance.battleRewardsController.ApplyPartyRewardsAndSave(warFunds);
        BattleFlowController.Instance.UpdateBattleEndUIPanel(warFunds);
    }
}