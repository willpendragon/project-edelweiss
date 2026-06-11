using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTypeController : MonoBehaviour
{
    public enum BattleType
    {
        RegularBattle,
        BattleWithDeity,
        PuzzleBattle,
        BossBattle
    }

    public static BattleTypeController Instance { get; private set; }
    public static event Action OnBattleTypeInitialized;

    private const string BattleTutorialSceneName = "battle_tutorial";
    private const string BattleSceneName = "battle_prototype";

    [SerializeField] AchievementsManager achievementsManager;
    [SerializeField] KeyController keyController;
    [SerializeField] private DeitySpawner _deitySpawner;

    public BattleType currentBattleType;

    // Add these to pass forced roaming deity state into the battle scene
    public static bool isForcedRoamingDeity = false;
    public static GameObject forcedRoamingDeityPrefab;

    private void Awake()
    {
        Instance = this;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == BattleTutorialSceneName)
        {
            BattleSelection();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);
        if (scene.name == BattleSceneName)
        {
            BattleSelection();
        }
    }
    private void BattleSelection()
    {
        // At the start of a battle, decide the level type.

        currentBattleType = RetrieveBattleType();

        if (currentBattleType == BattleType.PuzzleBattle || currentBattleType == BattleType.BossBattle)
        {
            GameObject fixedDeity = GridManager.Instance.currentMapData.RetrieveDeity();
            if (fixedDeity == null)
            {
                Debug.LogWarning($"{currentBattleType} map data is missing a Deity!");
            }
            else
            {
                _deitySpawner.SpawnDeity(fixedDeity);
            }
        }
        else if (currentBattleType == BattleType.RegularBattle && achievementsManager != null)
        {
            currentBattleType = achievementsManager.TriggerDeityAchievementLogic();
        }

        OnBattleTypeInitialized?.Invoke();
    }

    public BattleType RetrieveBattleType()
    {
        // Retrieve the Battle Type from MapData Scriptable Object
        switch (GridManager.Instance.currentMapData.levelType)
        {
            case (MapData.LevelType.Regular):
                return BattleType.RegularBattle;
            case (MapData.LevelType.Puzzle):
                return BattleType.PuzzleBattle;
            case (MapData.LevelType.Boss):
            case (MapData.LevelType.Miniboss):
                return BattleType.BossBattle;
            default:
                return BattleType.RegularBattle;
        }
    }
}