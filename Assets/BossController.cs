using UnityEngine;

public class BossController : MonoBehaviour
{
    public Unit bossUnit;
    public void AssignBossUnit()
    {
        // Look up for the Unit flagged as boss in the Enemy Party and assign it
        // to the bossUnit variable.
    }
    public void ApplyBuff(DeityPrayerBuff.AffectedStat buffingStat, float amount)
    {
        if (bossUnit == null)
        {
            switch (buffingStat)
            {
                case DeityPrayerBuff.AffectedStat.AttackPower:
                    bossUnit.unitAttackPower += amount;
                    break;
                case DeityPrayerBuff.AffectedStat.MagicPower:
                    bossUnit.unitMagicPower += amount;
                    break;
                case DeityPrayerBuff.AffectedStat.ShieldPower:
                    bossUnit.unitShieldPoints += amount;
                    break;
            }
        }
    }
}
