using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;

public class SummoningBuffController : MonoBehaviour
{
    public delegate void AppliedDeityBuffMessage(string deityBuff);
    public static event AppliedDeityBuffMessage OnAppliedDeityBuffMessage;

    public void ApplyLinkedDeityPermanentBuff(Unit selectedPlayerUnit)
    {
        if (selectedPlayerUnit.linkedDeity != null)
        {

            switch (selectedPlayerUnit.linkedDeity.deityPrayerBuff.currentAffectedStat)
            {
                case DeityPrayerBuff.AffectedStat.MaxHP:
                    selectedPlayerUnit.unitMaxHealthPoints += selectedPlayerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                    Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MaxHP + "from" + selectedPlayerUnit.linkedDeity + "on" + selectedPlayerUnit.unitTemplate.unitName);
                    OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MaxHP + "MAX HP");
                    break;
                case DeityPrayerBuff.AffectedStat.MagicPower:
                    Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MagicPower + "from" + selectedPlayerUnit.linkedDeity + "on" + selectedPlayerUnit.unitTemplate.unitName);
                    selectedPlayerUnit.unitMagicPower += selectedPlayerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                    OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MagicPower + "MAGIC");
                    break;
                default:
                    Debug.LogError("Unsupported stat type");
                    break;
            }
        }
    }

    public void RemoveLinkedDeityPermanentBuff(Unit selectedPlayerUnit)
    {

        if (selectedPlayerUnit.linkedDeity != null)
        {

            switch (selectedPlayerUnit.linkedDeity.deityPrayerBuff.currentAffectedStat)
            {
                case DeityPrayerBuff.AffectedStat.MaxHP:
                    selectedPlayerUnit.unitMaxHealthPoints -= selectedPlayerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                    Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MaxHP + "from" + selectedPlayerUnit.linkedDeity + "on" + selectedPlayerUnit.unitTemplate.unitName);
                    OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MaxHP + "MAX HP");
                    break;
                case DeityPrayerBuff.AffectedStat.MagicPower:
                    Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MagicPower + "from" + selectedPlayerUnit.linkedDeity + "on" + selectedPlayerUnit.unitTemplate.unitName);
                    selectedPlayerUnit.unitMagicPower -= selectedPlayerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                    OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MagicPower + "MAGIC");
                    break;
                default:
                    Debug.LogError("Unsupported stat type");
                    break;
            }
        }
    }

    public void ResetSummonBuffs()
    {
        foreach (Unit playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            if (playerUnit.linkedDeity != null)
            {
                playerUnit.unitMagicPower = playerUnit.unitTemplate.unitMagicPower;
            }
        }
    }
}