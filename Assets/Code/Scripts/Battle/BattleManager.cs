using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Distribute responsibilities.

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

    private string battleStartMessage = "Battle Begins!";

    public delegate void SavePlayerHealth(float finalPlayerHealth);
    public static event SavePlayerHealth OnSavePlayerHealth;

    public delegate void SavePlayerCoinsReward(float coinsReward);
    public static event SavePlayerCoinsReward OnSavePlayerCoinsReward;

    public delegate void SavePlayerExperienceReward(float experienceReward);
    public static event SavePlayerExperienceReward OnSavePlayerExperienceReward;

    public delegate void BattleEndResultsScreen(string battleEndMessage);
    public static event BattleEndResultsScreen OnBattleEndResultsScreen;

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
        BeginBattle();
    }
    private void BeginBattle()
    {
        BattleInterface.Instance.battleMomentsScreenHelper?.ActivateBattleMomentsScreen(battleStartMessage);
        BattleSFXManager.PlaySound(SoundType.BATTLEBEGINS, 1);
        TrackEnemiesOnBattlefield();
    }
    private void TrackEnemiesOnBattlefield()
    {
        enemiesOnBattlefield = GameObject.FindGameObjectsWithTag("Enemy");
    }
    private int frameCounter = 0;
    void Update()
    {
        if (frameCounter % 10 == 0)
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
                if (tile.detectedUnit == null || !tile.detectedUnit)
                {
                    tile.currentSingleTileCondition = SingleTileCondition.free;
                }
            }
        }
    }
    public void UnlockNextLevel()
    {
        GameSaveData saveData = SaveStateManager.saveData;
        int currentId = saveData.currentNodeId;

        // Failsafe configuration
        if (saveData.clearedNodesId == null)
            saveData.clearedNodesId = new List<int>();

        // Only append to cleared list and increase logic if the player hasn't already beaten this node
        if (!saveData.clearedNodesId.Contains(currentId))
        {
            saveData.clearedNodesId.Add(currentId);
            saveData.highestUnlockedLevel++; // We retain this metric for legacy scripts relying on total progression counting
        }
        
        SaveStateManager.SaveGame(saveData);
        Debug.Log($"Cleared Node {currentId}!");
    }

    public void PlayCameraBattleEndAnimation()
    {
        if (mainCameraPlayableDirector != null)
        {
            mainCameraPlayableDirector.Play();
        }
    }
}