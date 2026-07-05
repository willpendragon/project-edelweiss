using UnityEngine;

public static class DamageCalculationUtility
{
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