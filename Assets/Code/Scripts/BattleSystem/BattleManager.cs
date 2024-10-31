using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

public enum TurnOrder
{
    playerTurn,
    enemyTurn
}

public enum FieldEffectStatus
{
    active,
    inactive
}

public enum BattleType
{
    regularBattle,
    battleWithDeity,
    BossBattle
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] GameObject battleMomentsScreen;
    [SerializeField] BattleInterface battleInterface;
    [SerializeField] float battleMomentsScreenDeactivationTime;

    [SerializeField] DeityAchievementsController deityAchievementsController;

    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI turnTracker;
    public int turnCounter;
    public GameObject[] enemiesOnBattlefield;
    public Deity deity;
    public TurnOrder currentTurnOrder;
    public EnemySelection enemySelection;

    public int captureCrystalsRewardPool;

    public FieldEffectStatus fieldEffectStatus;

    private GameManager gameManager;

    public EnemyTurnManager enemyTurnManager;

    public BattleType currentBattleType;

    public delegate void SavePlayerHealth(float finalPlayerHealth);
    public static event SavePlayerHealth OnSavePlayerHealth;

    public delegate void SavePlayerCoinsReward(float coinsReward);
    public static event SavePlayerCoinsReward OnSavePlayerCoinsReward;

    public delegate void SavePlayerExperienceReward(float experienceReward);
    public static event SavePlayerExperienceReward OnSavePlayerExperienceReward;

    public delegate void BattleEndResultsScreen(string battleEndMessage);
    public static event BattleEndResultsScreen OnBattleEndResultsScreen;

    public UnityEvent PlayerTurnStarts;
    public UnityEvent PlayerTurnEnds;

    public GridManager gridManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        EnemyTurnManager.OnPlayerTurn += ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn += ActivateBattleMomentsScreen;
    }

    private void OnEnable()
    {
        TurnController.OnEnemyTurn += ActivateBattleMomentsScreen;
    }

    private void OnDisable()
    {
        EnemyTurnManager.OnPlayerTurn -= ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn -= ActivateBattleMomentsScreen;
        TurnController.OnEnemyTurn -= ActivateBattleMomentsScreen;

    }

    public void SetBattleType(BattleType battleType)
    {
        currentBattleType = battleType;
    }
    void Start()
    {
        //Looks for the Game Manager at the start of the battle.
        gameManager = GameManager.Instance;
        ActivateBattleMomentsScreen("Battle Begins!");
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");

        SetTurnOrder();
    }
    public void SetTurnOrder()
    {
        currentTurnOrder = TurnOrder.playerTurn;
        turnDisplay.text = "Player Turn";
        turnCounter += 1;
    }
    private int frameCounter = 0;
    void Update()
    {
        if (frameCounter % 10 == 0) // Run every 10 frames
        {
            ClearTilesWithMissingUnits();
        }
        frameCounter++;
    }
    void ClearTilesWithMissingUnits()
    {
        Debug.Log("Test Tiles Clearing Behaviour");
        if (currentBattleType == BattleType.battleWithDeity)
        {
            foreach (TileController tile in gridManager.gridTileControllers)
            {
                if (tile.detectedUnit == null || !tile.detectedUnit) // This catches both null and missing references
                {
                    tile.currentSingleTileCondition = SingleTileCondition.free;
                }
            }
        }
    }

    public void PassTurnToPlayer()
    {
        //Hands the turn to the Player.
        currentTurnOrder = TurnOrder.playerTurn;
        UpdateTurnCounter();
        Debug.Log("Turn Passed to Player");

        Button endTurnButton = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
        endTurnButton.interactable = true;
        PlayerTurnStarts.Invoke();

    }
    public bool AllEnemiesOpportunityZero()
    {
        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            EnemyAgent enemyScript = enemy.GetComponent<EnemyAgent>();

            if (enemyScript != null && enemyScript.opportunity > 0)
            {
                return false;
            }
        }
        return true;
    }
    public void UpdateTurnCounter()
    {
        turnDisplay.text = "Player Turn";
        turnCounter += 1;
        turnTracker.text = turnCounter.ToString();
    }

    public void RestoreOpportunityEnemies()
    {
        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            EnemyAgent enemyScript = enemy.GetComponent<EnemyAgent>();
            enemyScript.opportunity = 1;
        }
    }

    public void ActivateBattleMomentsScreen(string battleMomentText)
    {
        battleMomentsScreen.SetActive(true);
        battleMomentsScreen.GetComponentInChildren<TextMeshProUGUI>().text = battleMomentText;
        StartCoroutine("DeactivateBattleMomentsScreen");

    }
    IEnumerator DeactivateBattleMomentsScreen()
    {
        yield return new WaitForSeconds(battleMomentsScreenDeactivationTime);
        battleMomentsScreen.SetActive(false);
    }
}
