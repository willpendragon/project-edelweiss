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

        if (currentBattleType == BattleType.PuzzleBattle)
        {
            // Add specific logic for Puzzle Battles, to be further developed
            // Spawn Puzzle Deity (demo logic).
            currentBattleType = BattleType.PuzzleBattle;
            if (GridManager.Instance.currentMapData.RetrieveDeity() == null)
                return;
            else
            {
                GameObject puzzleDeity = GridManager.Instance.currentMapData.RetrieveDeity(); // Retrieve Deity
                _deitySpawner.SpawnDeity(puzzleDeity);
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
            default:
                return BattleType.RegularBattle;
        }
    }
}