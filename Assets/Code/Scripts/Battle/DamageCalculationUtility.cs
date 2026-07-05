using UnityEngine;

/// <summary>
/// Utility class for damage value flattening and rounding.
/// </summary>
public static class DamageCalculationUtility
{
    /// <summary>
    /// Flattens damage values using custom rounding rules:
    /// - Values from 0.0 to 0.4 round down
    /// - Values from 0.4 (exclusive) to 1.0 round up
    /// Example: 14.3 -> 14, 14.4 -> 14, 14.5 -> 15
    /// </summary>
    public static int FlattenDamage(float damageValue)
    {
        float fractionalPart = damageValue - Mathf.Floor(damageValue);

        if (fractionalPart >= 0.5f)
        {
            return Mathf.CeilToInt(damageValue);
        }
        else
        {
            return Mathf.FloorToInt(damageValue);
        }
    }
}