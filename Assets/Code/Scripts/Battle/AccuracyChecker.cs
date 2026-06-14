using UnityEngine;

/// <summary>
/// Utility class for performing accuracy checks on attacks and spells.
/// Integrates with the Faith modifier system to affect hit chances.
/// </summary>
public static class AccuracyChecker
{
    /// <summary>
    /// Checks if a melee attack hits the target.
    /// </summary>
    public static bool CheckMeleeAccuracy(Unit attacker, Unit defender, float baseAccuracy = 0.95f)
    {
        if (attacker == null) return true; // Fallback to hit if missing attacker

        float accuracy = baseAccuracy;

        // Apply Faith modifier to accuracy
        accuracy = FaithModifierCalculator.ApplyFaithAccuracyModifier(accuracy, attacker);

        Debug.Log($"{attacker.unitTemplate.unitName} melee attack accuracy: {accuracy * 100f}% (base: {baseAccuracy * 100f}%)");

        return Random.value < accuracy;
    }

    /// <summary>
    /// Checks if a spell hits the target.
    /// </summary>
    public static bool CheckSpellAccuracy(Unit caster, Unit target, Spell spell)
    {
        if (caster == null || spell == null) return true; // Fallback to hit if missing data

        float accuracy = spell.baseAccuracy;

        // Apply Faith modifier to accuracy
        accuracy = FaithModifierCalculator.ApplyFaithAccuracyModifier(accuracy, caster);

        Debug.Log($"{caster.unitTemplate.unitName} cast {spell.spellName} with accuracy: {accuracy * 100f}% (base: {spell.baseAccuracy * 100f}%)");

        return Random.value < accuracy;
    }

    /// <summary>
    /// Checks if an AOE spell hits a specific target within the affected area.
    /// </summary>
    public static bool CheckAOEAccuracy(Unit caster, Unit target, Spell spell)
    {
        if (caster == null || spell == null) return true;

        float accuracy = spell.baseAccuracy;

        // Apply Faith modifier to accuracy
        accuracy = FaithModifierCalculator.ApplyFaithAccuracyModifier(accuracy, caster);

        return Random.value < accuracy;
    }
}