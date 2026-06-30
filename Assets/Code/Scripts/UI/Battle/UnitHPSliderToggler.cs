using UnityEngine;

/// <summary>
/// Manages enemy HP slider visibility based on tile hover (Into the Breach style).
/// Sliders are hidden by default and only shown when hovering over an enemy's tile.
/// </summary>
public class UnitHPSliderToggler : MonoBehaviour
{
    /// <summary>
    /// Shows the HP slider for a unit. Only works for enemies.
    /// </summary>
    public static void ShowHPSlider(GameObject unit)
    {
        if (unit == null)
            return;

        // Only toggle sliders for enemy units
        if (!unit.CompareTag("Enemy"))
            return;

        var enemyProfileController = EnemyProfileController.Instance;
        if (enemyProfileController != null)
        {
            enemyProfileController.ShowEnemySlider(unit);
        }
        else
        {
            Debug.LogWarning("[Slider] EnemyProfileController instance not found");
        }
    }

    /// <summary>
    /// Hides the HP slider for a unit. Only works for enemies.
    /// </summary>
    public static void HideHPSlider(GameObject unit)
    {
        if (unit == null)
            return;

        // Only toggle sliders for enemy units
        if (!unit.CompareTag("Enemy"))
            return;

        var enemyProfileController = EnemyProfileController.Instance;
        if (enemyProfileController != null)
        {
            enemyProfileController.HideEnemySlider(unit);
        }
        else
        {
            Debug.LogWarning("[Slider] EnemyProfileController instance not found");
        }
    }
}