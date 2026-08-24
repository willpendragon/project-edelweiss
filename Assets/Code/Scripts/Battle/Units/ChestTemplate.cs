using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Chest Template", menuName = "UnitTemplates/ChestTemplate")]
public class ChestTemplate : UnitTemplate
{
    public enum ChestPrizeType
    {
        SimpleKey,
        MinibossKey,
        BossKey,
        Ingredient // Appended at end to preserve serialized int values of existing templates
    }

    [Header("Chest Specifics")]
    public ChestPrizeType chestPrizeType;

    [Tooltip("Optional direct reference. Overrides the Resources-folder naming convention when assigned.")]
    public ItemFieldPrize prizeOverride;
}