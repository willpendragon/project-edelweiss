using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using System.Collections.Generic;
using ProjectEdelweiss.Utils;

public static class PartyUtility
{
    public static Unit RetrieveActivePlayerUnit()
    {
        GameObject unitObject = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        return unitObject?.GetComponent<Unit>();
    }
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Gameplay Flow")] [SerializeField]
    DeityAchievementsController deityAchievementsController;

    [SerializeField] private DeityPowerController _deityPowerController;
    [SerializeField] private TextMeshProUGUI _bloodMoonBattleWarningText;
    
    public EnemyTurnManager enemyTurnManager;

    [Header("Actors on Battlefield")] public GameObject[] enemiesOnBattlefield;
    public Deity deity;
    public EnemySelection enemySelection;

    [Header("Prizes Logic")] public BattleRewardsController battleRewardsController;
    public int captureCrystalsRewardPool;

    [Header("UI")] [SerializeField] PlayableDirector mainCameraPlayableDirector;
    [SerializeField] float battleMomentsScreenDeactivationTime;
    private string battleStartMessage = "Battle Begins!";
    [Header("Camera")] [SerializeField] private CameraController _cameraController;

    public delegate void SavePlayerHealth(float finalPlayerHealth);
    public static event SavePlayerHealth OnSavePlayerHealth;

    public delegate void SavePlayerCoinsReward(float coinsReward);
    public static event SavePlayerCoinsReward OnSavePlayerCoinsReward;

    public delegate void SavePlayerExperienceReward(float experienceReward);
    public static event SavePlayerExperienceReward OnSavePlayerExperienceReward;

    public delegate void BattleEndResultsScreen(string battleEndMessage);
    public static event BattleEndResultsScreen OnBattleEndResultsScreen;

    public GridManager gridManager;
    public DeityPowerController DeityPowerController => _deityPowerController;

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
        TrackEnemiesOnBattlefield();
        StartCoroutine(BeginBattleCoroutine());
    }

    private System.Collections.IEnumerator BeginBattleCoroutine()
    {
        yield return null;

        if (PixelCrushers.DialogueSystem.DialogueManager.isConversationActive)
        {
            yield return new WaitUntil(() => !PixelCrushers.DialogueSystem.DialogueManager.isConversationActive);
        }

        // Display blood moon warning if active
        DisplayBloodMoonWarning();
        
        BattleInterface.Instance.battleMomentsScreenHelper?.ActivateBattleMomentsScreen(battleStartMessage);
        BattleSFXManager.PlaySound(SoundType.BATTLEBEGINS, 1);
        
        // Apply blood moon modifier to all enemies
        ApplyBloodMoonModifier();
    }

    private void DisplayBloodMoonWarning()
    {
        _bloodMoonBattleWarningText.gameObject.SetActive(false);
        BloodMoonManager bloodMoonManager = BloodMoonManager.Instance;
        if (bloodMoonManager != null && bloodMoonManager.IsBloodMoonActive)
        {
            if (_bloodMoonBattleWarningText != null)
            {
                _bloodMoonBattleWarningText.text = "BLOOD MOON - ENEMIES STRENGTHENED";
                _bloodMoonBattleWarningText.color = new Color(1f, 0.2f, 0.2f);
                _bloodMoonBattleWarningText.gameObject.SetActive(true);
                
                StartCoroutine(FadeOutBloodMoonWarning());
            }
        }
    }

    private System.Collections.IEnumerator FadeOutBloodMoonWarning()
    {
        yield return new WaitForSeconds(3f);
        
        if (_bloodMoonBattleWarningText != null)
        {
            _bloodMoonBattleWarningText.gameObject.SetActive(false);
        }
    }

    private void ApplyBloodMoonModifier()
    {
        BloodMoonManager bloodMoonManager = BloodMoonManager.Instance;
        if (bloodMoonManager == null || !bloodMoonManager.IsBloodMoonActive)
            return;

        float multiplier = bloodMoonManager.EnemyAttackMultiplier;

        foreach (GameObject enemy in enemiesOnBattlefield)
        {
            if (enemy == null) continue;
            
            Unit enemyUnit = enemy.GetComponent<Unit>();
            if (enemyUnit != null)
            {
                // Modify the base melee damage that gets multiplied in the damage calculation
                enemyUnit.unitMeleeAttackBaseDamage *= multiplier;
                Debug.Log($"[Blood Moon] {enemy.name} melee base damage increased by {(multiplier - 1) * 100}% (now {enemyUnit.unitMeleeAttackBaseDamage})");
            }
        }
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

        if (saveData.clearedNodesId == null)
            saveData.clearedNodesId = new List<int>();

        if (!saveData.clearedNodesId.Contains(currentId))
        {
            saveData.clearedNodesId.Add(currentId);
            saveData.highestUnlockedLevel++;
        }

        SaveStateManager.SaveGame(saveData);
        Debug.Log($"Cleared Node {currentId}!");
    }

    public void PlayCameraBattleEndAnimation()
    {
        _cameraController.ApplyBattleEndCameraSettings();
    }
}