using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Gameplay Flow")]
    [SerializeField] DeityAchievementsController deityAchievementsController;
    public EnemyTurnManager enemyTurnManager;

    [Header("Actors on Battlefield")]
    public GameObject[] enemiesOnBattlefield;
    public Deity deity;
    public EnemySelection enemySelection;

    [Header("Prizes Logic")]
    public BattleRewardsController battleRewardsController;
    public int captureCrystalsRewardPool;

    [Header("UI")]
    [SerializeField] PlayableDirector mainCameraPlayableDirector;
    [SerializeField] float battleMomentsScreenDeactivationTime;

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
    }
    private void TrackEnemiesOnBattlefield()
    {
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
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
    public void UnlockNextLevel()
    {
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