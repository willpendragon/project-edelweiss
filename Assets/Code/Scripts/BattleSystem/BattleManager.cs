using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Playables;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Gameplay Flow")]
    [SerializeField] float battleMomentsScreenDeactivationTime;
    [SerializeField] DeityAchievementsController deityAchievementsController;
    public EnemyTurnManager enemyTurnManager;

    [Header("Actors on Battlefield")]
    public GameObject[] enemiesOnBattlefield;
    public Deity deity;
    public EnemySelection enemySelection;

    [Header("Prizes Logic")]
    public BattleRewardsController battleRewardsController;
    public int captureCrystalsRewardPool;

    [Header("Camera Work")]
    [SerializeField] PlayableDirector mainCameraPlayableDirector;
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI turnTracker;
    public int turnCounter;

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
    }

    void Start()
    {
        ExecuteBattleStartingSequence();
    }
    private void ExecuteBattleStartingSequence()
    {
        BattleInterface.Instance.battleMomentsScreenHelper?.ActivateBattleMomentsScreen("Battle Begins!");
        TrackEnemiesOnBattlefield();
        //SetTurnOrder();
    }
    private void TrackEnemiesOnBattlefield()
    {
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
    }
    public void SetTurnOrder()
    {
        //currentTurnOrder = TurnOrder.playerTurn;
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
        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
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
        //turnDisplay.text = "Player Turn";
        //turnCounter += 1;
        //turnTracker.text = turnCounter.ToString();
    }
    public void RestoreOpportunityEnemies()
    {
        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            EnemyAgent enemyScript = enemy.GetComponent<EnemyAgent>();
            enemyScript.opportunity = 1;
        }
    }
    public void UnlockNextLevel()
    {
        // This probably belongs more to a Progression Manager
        GameSaveData saveData = SaveStateManager.saveData;
        saveData.highestUnlockedLevel++;
        SaveStateManager.SaveGame(saveData);
        Debug.Log("Unlocking Next Level");
    }

    public void PlayCameraBattleEndAnimation()
    {
        if (mainCameraPlayableDirector != null)
        {
            mainCameraPlayableDirector.Play();
        }
    }
}