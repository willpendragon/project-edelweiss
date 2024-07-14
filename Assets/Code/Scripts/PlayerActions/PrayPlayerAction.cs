using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PrayPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public delegate void PlayerPrayer();
    public static event PlayerPrayer OnPlayerPrayer;

    public void Select(TileController selectedTile)
    {
        if (selectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity)
        {
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
            savedSelectedTile = selectedTile;
            selectionLimiter--;
            Debug.Log("Selected Deity Possessed Tile for Praying");
        }
    }

    public void Deselect()
    {
        savedSelectedTile = null;
        savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
        selectionLimiter++;
    }

    public void Execute()
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Deity linkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().linkedDeity;


        if (savedSelectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity && currentActivePlayerUnit.unitOpportunityPoints > 0)
        {
            Debug.Log("Praying for Deity");
            if (CheckLinkedDeityPrayerPower())
            {
                currentActivePlayerUnit.unitOpportunityPoints--;
                PerformDeityPowerUp(linkedDeity);
            }
            else
            {
                currentActivePlayerUnit.unitOpportunityPoints--;
                OnPlayerPrayer();

                // Plays the Prayer's SFX
                currentActivePlayerUnit.battleFeedbackController.PlayPrayerSFX.Invoke();
            }
        }
        else
        {
            Debug.Log("Active Player Unit is unable to pray");
        }
    }

    private bool CheckLinkedDeityPrayerPower()
    {
        Deity linkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().linkedDeity;

        if (linkedDeity.deityPrayerPower >= linkedDeity.deityPrayerBuff.deityPrayerBuffThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void PerformDeityPowerUp(Deity linkedDeity)
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        switch (linkedDeity.deityPrayerBuff.currentAffectedStat)
        {
            case DeityPrayerBuff.AffectedStat.MaxHP:
                currentActivePlayerUnit.unitMaxHealthPoints += linkedDeity.deityPrayerBuff.buffAmount;
                PlayBuffFeedback(currentActivePlayerUnit, linkedDeity);
                break;
            case DeityPrayerBuff.AffectedStat.MagicPower:
                currentActivePlayerUnit.unitMagicPower += linkedDeity.deityPrayerBuff.buffAmount;
                PlayBuffFeedback(currentActivePlayerUnit, linkedDeity);
                break;
            default:
                Debug.LogError("Unsupported stat type");
                break;
        }

    }

    private void PlayBuffFeedback(Unit affectedUnit, Deity linkedDeity)
    {
        Debug.Log("Playing Buff Feedback");
        BattleInterface.Instance.SetSpellNameOnNotificationPanel("Blessing", linkedDeity.name);
        float moveDistance = 5f;
        float duration = 1f;
        //Instantiate Buff Feedback on Active Player Unit

        GameObject buffIcon = Instantiate(affectedUnit.gameObject.GetComponent<BattleFeedbackController>().buffIcon, affectedUnit.gameObject.transform);

        // Move the arrow upwards
        buffIcon.transform.DOMoveY(buffIcon.transform.position.y + moveDistance, duration)
            .SetEase(Ease.OutQuad);

        // Fade out the arrow
        buffIcon.GetComponent<SpriteRenderer>().DOFade(0, duration)
                .SetEase(Ease.Linear);

        Destroy(buffIcon, duration);
        // Update Active Player Unit UI
    }
}
