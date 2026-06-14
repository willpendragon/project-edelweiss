using UnityEngine;

/// <summary>
/// Utility class for calculating Faith-based modifiers on gameplay mechanics.
/// Acts as a static accessor to Faith modifier settings.
/// </summary>
public static class FaithModifierCalculator
{
    private static FaithModifierSettings _settings;

    public static FaithModifierSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                _settings = Resources.Load<FaithModifierSettings>("FaithModifierSettings");
                if (_settings == null)
                {
                    Debug.LogError("FaithModifierSettings not found in Resources folder. Please create one at Resources/FaithModifierSettings.asset");
                }
            }
            return _settings;
        }
    }

    /// <summary>
    /// Applies Faith modifier to damage calculation.
    /// </summary>
    public static float ApplyFaithDamageModifier(float baseDamage, Unit unit)
    {
        if (unit == null || Settings == null) return baseDamage;

        float multiplier = Settings.CalculateDamageMultiplier(unit.unitFaithPoints, unit.unitTemplate.unitFaithPoints);
        return baseDamage * multiplier;
    }

    /// <summary>
    /// Applies Faith modifier to accuracy/hit chance.
    /// </summary>
    public static float ApplyFaithAccuracyModifier(float baseHitChance, Unit unit)
    {
        if (unit == null || Settings == null) return baseHitChance;

        float modifier = Settings.CalculateAccuracyModifier(unit.unitFaithPoints, unit.unitTemplate.unitFaithPoints);
        return Mathf.Clamp01(baseHitChance + modifier);
    }

    /// <summary>
    /// Applies Faith modifier to critical hit chance.
    /// </summary>
    public static float ApplyFaithCriticalModifier(float baseCritChance, Unit unit)
    {
        if (unit == null || Settings == null) return baseCritChance;

        float multiplier = Settings.CalculateCriticalMultiplier(unit.unitFaithPoints, unit.unitTemplate.unitFaithPoints);
        return Mathf.Clamp01(baseCritChance * multiplier);
    }
}