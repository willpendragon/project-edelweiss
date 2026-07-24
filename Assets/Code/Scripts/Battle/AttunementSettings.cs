using UnityEngine;

/// <summary>
/// ScriptableObject configuration for Deity Attunement mechanics.
/// Controls QTE windows, base rates, health modifiers, tribute bonuses, and capture chance clamping.
/// </summary>
[CreateAssetMenu(fileName = "AttunementSettings", menuName = "Battle/Attunement Settings")]
public class AttunementSettings : ScriptableObject
{
    [Header("QTE Window Thresholds (0-1 normalized)")]
    [Tooltip("Distance from center beyond which the QTE counts as a miss (e.g., 0.90 = 90% off-center)")]
    [Range(0f, 1f)]
    public float missThreshold = 0.90f;
    
    [Tooltip("Distance from center for normal window tolerance (e.g., 0.45 = 40-50% tolerance)")]
    [Range(0f, 1f)]
    public float normalThreshold = 0.45f;
    
    [Tooltip("Distance from center for perfect window tolerance (e.g., 0.15 = 10-20% tolerance)")]
    [Range(0f, 1f)]
    public float perfectThreshold = 0.15f;

    [Header("Base Capture Rates")]
    [Tooltip("Base capture rate for missed QTE (default: 0%)")]
    [Range(0f, 1f)]
    public float missBaseRate = 0.0f;
    
    [Tooltip("Base capture rate for normal QTE timing (default: 25%)")]
    [Range(0f, 1f)]
    public float normalBaseRate = 0.25f;
    
    [Tooltip("Base capture rate for perfect QTE timing (default: 50%)")]
    [Range(0f, 1f)]
    public float perfectBaseRate = 0.50f;

    [Header("Health Modifier")]
    [Tooltip("Maximum health modifier bonus when deity is at 0% HP (default: 0.50 = 50%)")]
    [Range(0f, 1f)]
    public float maxHealthModifier = 0.50f;
    
    [Tooltip("Use linear (false) or curved (true) health scaling")]
    public bool useHealthCurve = false;
    
    [Tooltip("AnimationCurve for health modifier scaling (X: deity HP %, Y: modifier bonus)")]
    public AnimationCurve healthModifierCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 0f);

    [Header("Tribute Modifiers")]
    [Tooltip("Capture chance bonus per tribute used (default: 0.10 = 10%)")]
    [Range(0f, 0.5f)]
    public float tributeBonusPerUse = 0.10f;

    [Header("Capture Chance Limits")]
    [Tooltip("Minimum final capture chance (default: 0% = no minimum)")]
    [Range(0f, 1f)]
    public float minCaptureChance = 0.0f;
    
    [Tooltip("Maximum final capture chance (default: 1.0 = 100%)")]
    [Range(0f, 1f)]
    public float maxCaptureChance = 1.0f;

    [Header("QTE Slider Settings")]
    [Tooltip("Speed of the QTE slider movement (units per second)")]
    [Range(0.1f, 5f)]
    public float sliderSpeed = 1.5f;
    
    [Tooltip("Time window for player to input (seconds)")]
    [Range(1f, 10f)]
    public float inputTimeWindow = 3f;

    /// <summary>
    /// Calculates the health modifier based on current deity HP percentage.
    /// </summary>
    public float CalculateHealthModifier(float healthPercentage)
    {
        if (useHealthCurve)
        {
            return healthModifierCurve.Evaluate(healthPercentage);
        }
        else
        {
            // Linear: At 100% HP = 0%, At 0% HP = maxHealthModifier
            return (1.0f - healthPercentage) * maxHealthModifier;
        }
    }

    /// <summary>
    /// Determines the base rate based on QTE timing result.
    /// </summary>
    public float GetBaseRateForTiming(float distanceFromCenter)
    {
        if (distanceFromCenter <= perfectThreshold)
            return perfectBaseRate;
        else if (distanceFromCenter <= normalThreshold)
            return normalBaseRate;
        else if (distanceFromCenter <= missThreshold)
            return missBaseRate;
        else
            return 0f; // Complete miss beyond threshold
    }

    /// <summary>
    /// Calculates total tribute modifier based on number of stacks used.
    /// </summary>
    public float CalculateTributeModifier(int tributeStacks)
    {
        return tributeStacks * tributeBonusPerUse;
    }

    /// <summary>
    /// Clamps the final capture chance to configured min/max values.
    /// </summary>
    public float ClampCaptureChance(float rawChance)
    {
        return Mathf.Clamp(rawChance, minCaptureChance, maxCaptureChance);
    }
}
