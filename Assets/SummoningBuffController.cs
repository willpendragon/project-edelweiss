using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;

public class SummoningBuffController : MonoBehaviour
{
    public delegate void AppliedDeityBuffMessage(string deityBuff);
    public static event AppliedDeityBuffMessage OnAppliedDeityBuffMessage;

    public void OnEnable()
    {
        TurnController.OnResetSummonBuffs += ResetSummonBuffs;
    }
    public void OnDisable()
    {
        TurnController.OnResetSummonBuffs -= ResetSummonBuffs;
    }
    public void Start()
    {
        ApplyLinkedDeityBuff();
    }
    public void ApplyLinkedDeityBuff()
    {
        foreach (Unit playerUnit in GameManager.Instance.playerPartyMembersInstances)


            if (playerUnit.linkedDeity != null)
            {

                switch (playerUnit.linkedDeity.deityPrayerBuff.currentAffectedStat)
                {
                    case DeityPrayerBuff.AffectedStat.MaxHP:
                        playerUnit.unitMaxHealthPoints += playerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                        Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MaxHP + "from" + playerUnit.linkedDeity + "on" + playerUnit.unitTemplate.unitName);
                        OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MaxHP + "MAX HP");

                        //PlayBuffFeedback(currentActivePlayerUnit, linkedDeity);
                        break;
                    case DeityPrayerBuff.AffectedStat.MagicPower:
                        Debug.Log("Applying" + DeityPrayerBuff.AffectedStat.MagicPower + "from" + playerUnit.linkedDeity + "on" + playerUnit.unitTemplate.unitName);
                        playerUnit.unitMagicPower += playerUnit.linkedDeity.deityPrayerBuff.buffAmount;
                        OnAppliedDeityBuffMessage("+" + DeityPrayerBuff.AffectedStat.MagicPower + "MAGIC");

                        //PlayBuffFeedback(currentActivePlayerUnit, linkedDeity);
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
