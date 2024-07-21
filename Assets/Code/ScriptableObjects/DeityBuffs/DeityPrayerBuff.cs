using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deity Prayer Buff", menuName = "Deity Prayer Buff")]

public class DeityPrayerBuff : ScriptableObject
{
    public enum AffectedStat
    {
        AttackPower,
        MagicPower,
        MaxHP
    }

    public int buffAmount;
    public AffectedStat currentAffectedStat;
    public float deityPrayerBuffThreshold;

}
