using UnityEngine;

[CreateAssetMenu(fileName = "FaithModifierSettings", menuName = "Battle/Faith Modifier Settings")]
public class FaithModifierSettings : ScriptableObject
{
    [Header("Faith Influence Ramp")]
    [Range(0f, 1f)]
    [Tooltip("Controls how much Faith influences damage, accuracy, and critical strikes (0 = no influence, 1 = maximum influence)")]
    public float faithInfluenceModifier = 0.5f;

    [Header("Damage Modifiers")]
    [Tooltip("Minimum damage multiplier when Faith is 0")]
    public float minDamageMultiplier = 0.3f;

    [Tooltip("Maximum damage multiplier when Faith is at its peak")]
    public float maxDamageMultiplier = 1.5f;

    [Header("Accuracy Modifiers")]
    [Tooltip("Hit chance reduction when Faith is 0 (0.0 = 0%, 1.0 = 100% miss rate)")]
    public float minAccuracyPenalty = 0.6f;

    [Tooltip("Hit chance bonus when Faith is at its peak")]
    public float maxAccuracyBonus = 0.2f;

    [Header("Critical Strike Modifiers")]
    [Tooltip("Minimum critical hit chance multiplier when Faith is 0")]
    public float minCriticalMultiplier = 0.1f;

    [Tooltip("Maximum critical hit chance multiplier when Faith is at its peak")]
    public float maxCriticalMultiplier = 2.0f;

    /// <summary>
    /// Calculates the damage multiplier based on current Faith points and max Faith.
    /// </summary>
    public float CalculateDamageMultiplier(int currentFaith, int maxFaith)
    {
        if (maxFaith <= 0) return minDamageMultiplier;

        float faithRatio = Mathf.Clamp01((float)currentFaith / maxFaith);
        float baseMultiplier = Mathf.Lerp(minDamageMultiplier, maxDamageMultiplier, faithRatio);

        // Apply influence ramp: 0 influence = always base multiplier, 1 influence = full effect
        return Mathf.Lerp(1f, baseMultiplier, faithInfluenceModifier);
    }

    /// <summary>
    /// Calculates the accuracy penalty/bonus based on current Faith.
    /// Returns a value to be added to the hit chance (-0.6 to +0.2).
    /// </summary>
    public float CalculateAccuracyModifier(int currentFaith, int maxFaith)
    {
        if (maxFaith <= 0) return -minAccuracyPenalty;

        float faithRatio = Mathf.Clamp01((float)currentFaith / maxFaith);
        float baseModifier = Mathf.Lerp(-minAccuracyPenalty, maxAccuracyBonus, faithRatio);

        // Apply influence ramp
        return Mathf.Lerp(0f, baseModifier, faithInfluenceModifier);
    }

    /// <summary>
    /// Calculates the critical hit chance multiplier based on current Faith.
    /// </summary>
    public float CalculateCriticalMultiplier(int currentFaith, int maxFaith)
    {
        if (maxFaith <= 0) return minCriticalMultiplier;

        float faithRatio = Mathf.Clamp01((float)currentFaith / maxFaith);
        float baseMultiplier = Mathf.Lerp(minCriticalMultiplier, maxCriticalMultiplier, faithRatio);

        // Apply influence ramp
        return Mathf.Lerp(1f, baseMultiplier, faithInfluenceModifier);
    }
}