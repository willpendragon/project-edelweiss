using UnityEngine;
using DG.Tweening;
using Edelweiss.Core;
using ProjectEdelweiss.Utils;

public class AttunementPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    private System.Random localRandom = new System.Random();

    [Header("Configuration")]
    [Tooltip("Attunement settings ScriptableObject (optional - will use defaults if not set)")]
    [SerializeField] private AttunementSettings settings;

    // Events
    public delegate void AttunementAttempted(float captureChance);
    public static event AttunementAttempted OnAttunementAttempted;
    
    public delegate void AttunementSuccess(string message);
    public static event AttunementSuccess OnAttunementSuccess;
    
    public delegate void AttunementFailed(string message);
    public static event AttunementFailed OnAttunementFailed;

    private const string FAILED_CAPTURE_MESSAGE = "The binding attempt failed.";
    private const string SUCCESS_CAPTURE_MESSAGE = "Deity was Captured!";
    
    // Fallback values if no ScriptableObject is assigned
    private const float FALLBACK_NORMAL_BASE = 0.25f;
    private const float FALLBACK_PERFECT_BASE = 0.50f;
    private const float FALLBACK_MAX_HEALTH_MOD = 0.50f;
    public delegate void BattleEndCapturedDeity(string battleEndMessage);
    public static event BattleEndCapturedDeity OnBattleEndCapturedDeity;
    public void Select(TileController selectedTile)
    {
        // Not needed for this action
    }

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit)?.GetComponent<Unit>();
        if (activePlayerUnit == null)
        {
            Debug.LogWarning("AttunementPlayerAction: No active player unit found.");
            return;
        }

        // Check OP cost
        if (activePlayerUnit.unitOpportunityPoints <= 0)
        {
            BattleInterface.Instance.SetDeityNotification("Not enough Energy...");
            return;
        }

        // Verify we're in a deity battle
        var battleTypeController = BattleTypeController.Instance;
        if (battleTypeController == null || 
            battleTypeController.currentBattleType != BattleTypeController.BattleType.BattleWithDeity)
        {
            BattleInterface.Instance.SetDeityNotification("No deity to attune with...");
            return;
        }

        // Get deity reference
        var deitySpawner = FindAnyObjectByType<DeitySpawner>();
        if (deitySpawner == null || deitySpawner.currentUnboundDeity == null)
        {
            BattleInterface.Instance.SetDeityNotification("Deity not found...");
            return;
        }

        Deity deity = deitySpawner.currentUnboundDeity;

        // Consume OP
        activePlayerUnit.unitOpportunityPoints--;
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(
            activePlayerUnit.unitTemplate.unitName);

        // Trigger QTE and process result
        TriggerQTE(deity, activePlayerUnit);
    }

    private void TriggerQTE(Deity deity, Unit activePlayerUnit)
    {
        // Try to find QTE controller in scene
        var qteController = FindAnyObjectByType<AttunementQTEController>();
        // Just retrieve settings from the slot I've manually populated in the scene.
        settings = qteController.settings;

        if (qteController != null && settings != null)
        {
            // // Pass settings to QTE controller
            // qteController.SetSettings(settings);
            
            // Start QTE with callback
            qteController.StartQTE((distanceFromCenter) => {
                OnQTEComplete(distanceFromCenter, deity, activePlayerUnit);
            });
        }
        else
        {
            // Fallback: simulate QTE if controller not found or settings missing
            Debug.LogWarning("AttunementPlayerAction: QTE Controller or Settings not found. Using simulation.");
            float simulatedDistance = SimulateQTE();
            OnQTEComplete(simulatedDistance, deity, activePlayerUnit);
        }
    }

    private void OnQTEComplete(float distanceFromCenter, Deity deity, Unit activePlayerUnit)
    {
        // Determine base rate from QTE result
        float qteBaseRate = GetBaseRateFromQTE(distanceFromCenter);
        
        // Calculate final capture chance
        float finalCaptureChance = CalculateCaptureChance(deity, qteBaseRate);
        
        Debug.Log($"AttunementPlayerAction: Final capture chance = {finalCaptureChance * 100:F1}%");
        OnAttunementAttempted?.Invoke(finalCaptureChance);

        // Roll for capture
        if (AttemptCapture(finalCaptureChance))
        {
            HandleCaptureSuccess(activePlayerUnit, deity);
        }
        else
        {
            var deitySpawner = FindAnyObjectByType<DeitySpawner>();
            HandleCaptureFailure(deitySpawner);
        }
    }

    private float GetBaseRateFromQTE(float distanceFromCenter)
    {
        if (settings != null)
        {
            return settings.GetBaseRateForTiming(distanceFromCenter);
        }
        else
        {
            // Fallback thresholds
            if (distanceFromCenter <= 0.15f) return FALLBACK_PERFECT_BASE;
            if (distanceFromCenter <= 0.45f) return FALLBACK_NORMAL_BASE;
            return 0f;
        }
    }

    /// <summary>
    /// Simulates QTE for fallback when controller is not available.
    /// </summary>
    private float SimulateQTE()
    {
        // Simulate as "normal" timing
        Debug.Log("AttunementPlayerAction: QTE simulation - using NORMAL timing");
        return 0.30f; // Within normal threshold but not perfect
    }

    /// <summary>
    /// Calculates final capture chance: Base + Health Modifier + Tribute Modifiers
    /// </summary>
    private float CalculateCaptureChance(Deity deity, float qteBaseRate)
    {
        Unit deityUnit = deity.GetComponent<Unit>();
        if (deityUnit == null) return qteBaseRate;

        int maxHP = deityUnit.unitTemplate.unitMaxHealthPoints;
        int currentHP = deityUnit.unitTemplate.unitHealthPoints;
        float healthPercentage = (float)currentHP / maxHP;

        // Health modifier calculation
        float healthModifier;
        if (settings != null)
        {
            healthModifier = settings.CalculateHealthModifier(healthPercentage);
        }
        else
        {
            // Fallback: Linear scaling
            healthModifier = (1.0f - healthPercentage) * FALLBACK_MAX_HEALTH_MOD;
        }

        // Tribute modifier from stacks
        float tributeModifier;
        if (settings != null)
        {
            tributeModifier = settings.CalculateTributeModifier(TributeModifierTracker.Instance.TributeStacks);
        }
        else
        {
            tributeModifier = TributeModifierTracker.Instance.TotalModifier;
        }

        // Final calculation
        float rawChance = qteBaseRate + healthModifier + tributeModifier;
        
        // Clamp
        float finalChance;
        if (settings != null)
        {
            finalChance = settings.ClampCaptureChance(rawChance);
        }
        else
        {
            finalChance = Mathf.Clamp01(rawChance);
        }

        Debug.Log($"Capture Calc: Base={qteBaseRate:F2}, Health%={healthPercentage:F2}, " +
                  $"HealthMod={healthModifier:F2}, TributeMod={tributeModifier:F2}, Final={finalChance:F2}");

        return finalChance;
    }

    /// <summary>
    /// Rolls for capture attempt based on calculated probability
    /// </summary>
    private bool AttemptCapture(float captureProbability)
    {
        float captureRoll = (float)localRandom.NextDouble();
        bool success = captureRoll < captureProbability;
        Debug.Log($"AttunementPlayerAction: Roll={captureRoll:F2}, Required<{captureProbability:F2}, Success={success}");
        return success;
    }

    /// <summary>
    /// Handles successful deity capture - ends battle and creates save entry
    /// </summary>
    private void HandleCaptureSuccess(Unit activePlayerUnit, Deity capturedDeity)
    {
        // Capture before ResetTags() below wipes the ActivePlayerUnit tag, so the end-camera can focus on them.
        BattleManager.Instance.SetBattleEndFocusUnit(activePlayerUnit);

        OnAttunementSuccess?.Invoke(SUCCESS_CAPTURE_MESSAGE);
        BattleInterface.Instance.SetDeityNotification(SUCCESS_CAPTURE_MESSAGE);

        // Trigger battle end sequence
        OnBattleEndCapturedDeity("Deity was Captured");

        BattleFlowController.Instance.ResetTags();
        BattleManager.Instance.UnlockNextLevel();
        
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag(GameTags.GAME_STATS_MANAGER)
            ?.GetComponent<GameStatsManager>();
        gameStatsManager?.SaveCaptureCrystalsCount();

        // Create save entry linking player to deity
        string activePlayerUnitId = activePlayerUnit.Id;
        CreateDictionaryEntry(capturedDeity, activePlayerUnitId);
        GameManager.Instance.DeityLinkManager.ApplyDeityLinks();

        Debug.Log($"AttunementPlayerAction: Capture successful! {capturedDeity.name} bound to {activePlayerUnit.unitTemplate.unitName}");
    }

    /// <summary>
    /// Handles failed capture attempt - shows visual feedback
    /// </summary>
    private void HandleCaptureFailure(DeitySpawner deitySpawner)
    {
        OnAttunementFailed?.Invoke(FAILED_CAPTURE_MESSAGE);
        BattleInterface.Instance.SetDeityNotification(FAILED_CAPTURE_MESSAGE);
        
        GameObject deityObelisk = deitySpawner.DeityObelisk;
        if (deityObelisk != null)
        {
            PlayFailureFeedback(deityObelisk);
        }

        Debug.Log("AttunementPlayerAction: Capture failed!");
    }

    /// <summary>
    /// Visual feedback for failed capture - shake and flash obelisk
    /// </summary>
    private void PlayFailureFeedback(GameObject deityObelisk)
    {
        if (deityObelisk == null) return;

        Renderer rend = deityObelisk.GetComponentInChildren<MeshRenderer>();
        if (rend == null) return;
        
        Material mat = rend.material;
        Color originalColor = mat.color;

        Sequence seq = DOTween.Sequence();
        seq.Join(deityObelisk.transform.DOShakePosition(0.3f, 0.2f, 10, 90, false, true));
        seq.Join(mat.DOColor(Color.red, 0.1f));
        seq.Append(mat.DOColor(originalColor, 0.2f));
    }

    /// <summary>
    /// Creates persistent save entry linking captured deity to player unit.
    /// If player already has a bond, adds deity to unassigned pool instead.
    /// </summary>
    private void CreateDictionaryEntry(Deity capturedDeity, string playerId)
    {
        GameSaveData saveData = SaveStateManager.saveData;
        
        // Check if this player unit already has a deity link
        if (saveData.unitsLinkedToDeities.ContainsKey(playerId))
        {
            // Unit already bonded - add deity to unassigned pool instead
            string deityId = capturedDeity.Id;
            if (!saveData.unassignedCapturedDeities.Contains(deityId))
            {
                saveData.unassignedCapturedDeities.Add(deityId);
                SaveStateManager.SaveGame(saveData);
                Debug.Log($"AttunementPlayerAction: {capturedDeity.name} captured but unassigned (player already bonded). Available in Deity Altar.");
                BattleInterface.Instance.SetDeityNotification($"{capturedDeity.GetComponent<Unit>().unitTemplate.unitName} captured! Visit Deity Altar to assign.");
            }
        }
        else
        {
            // No existing bond - create link normally
            saveData.unitsLinkedToDeities.Add(playerId, capturedDeity.Id);
            SaveStateManager.SaveGame(saveData);
            Debug.Log($"AttunementPlayerAction: Saved link - Player:{playerId} -> Deity:{capturedDeity.Id}");
        }
    }

    public void Deselect()
    {
        // Not needed for this action
    }
}
