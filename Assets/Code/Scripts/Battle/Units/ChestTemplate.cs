using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Chest Template", menuName = "UnitTemplates/ChestTemplate")]
public class ChestTemplate : UnitTemplate
{
    public enum ChestPrizeType
    {
        SimpleKey,
        MinibossKey,
        BossKey
    }

    [Header("Chest Specifics")]
    public ChestPrizeType chestPrizeType;
}