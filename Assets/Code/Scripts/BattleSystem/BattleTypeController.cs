using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTypeController : MonoBehaviour
{
    public static BattleTypeController Instance { get; private set; }
    public static event Action OnBattleTypeInitialized;

    private const string BattleTutorialSceneName = "battle_tutorial";
    private const string BattleSceneName = "battle_prototype";

    public enum BattleType
    {
        RegularBattle,
        BattleWithDeity,
        PuzzleBattle,
        BossBattle
    }

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

    [SerializeField] AchievementsManager achievementsManager;
    [SerializeField] KeyController keyController;

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
        Debug.Log("BattleSelection started.");

        if (keyController != null && keyController.ValidateKey())
        {
            currentBattleType = keyController.UnlockLevelLogic();
        }
        else if (currentBattleType == BattleType.RegularBattle && achievementsManager != null)
        {
            currentBattleType = achievementsManager.TriggerDeityAchievementLogic();
        }
        OnBattleTypeInitialized?.Invoke();
    }
}