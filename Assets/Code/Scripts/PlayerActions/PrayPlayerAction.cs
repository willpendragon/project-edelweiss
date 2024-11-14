using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

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
        Deity summonedLinkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().summonedLinkedDeity;

        if (savedSelectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity && currentActivePlayerUnit.unitOpportunityPoints > 0)
        {
            if (CheckLinkedDeityPrayerPower())
            {
                BattleInterface.Instance.SetSpellNameOnNotificationPanel(summonedLinkedDeity.name, "is fulfilling the Prayer from" + currentActivePlayerUnit.unitTemplate.name);
                currentActivePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitProfile(currentActivePlayerUnit);
                //PerformDeityPowerUp(linkedDeity);
                summonedLinkedDeity.summoningBehaviour.ExecuteBehavior(summonedLinkedDeity);
                Debug.Log("The Deity is fulfilling the Current Active User's prayer.");
                ResetSummonBehaviour(currentActivePlayerUnit);
                Destroy(summonedLinkedDeity.gameObject, 3);
                Debug.Log("Summoned Deity disappears from the Battlefield.");
            }
            else
            {
                BattleInterface.Instance.SetSpellNameOnNotificationPanel(currentActivePlayerUnit.unitTemplate.name, "is praying" + summonedLinkedDeity.name);
                currentActivePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitProfile(currentActivePlayerUnit);
                OnPlayerPrayer();

                // Plays the Prayer's SFX
                currentActivePlayerUnit.battleFeedbackController.PlayPrayerSFX.Invoke();
                Debug.Log("The Current Active Unit is praying to the Linked Deity");
            }
        }
        else
        {
            Debug.Log("Active Player Unit is unable to pray");
        }
    }

    private bool CheckLinkedDeityPrayerPower()
    {
        Deity summonedLinkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().summonedLinkedDeity;

        if (summonedLinkedDeity.deityPrayerPower >= summonedLinkedDeity.deityPrayerBuff.deityPrayerBuffThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ResetSummonBehaviour(Unit currentActivePlayerUnit)
    {
        SummoningUIController currentActivePlayerUnitSummoningUIController = currentActivePlayerUnit.gameObject.GetComponent<SummoningUIController>();
        currentActivePlayerUnitSummoningUIController.currentSummonPhase = SummoningUIController.SummonPhase.summoning;
        currentActivePlayerUnitSummoningUIController.currentButton.GetComponentInChildren<Text>().text = "Summon";
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
    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
        Debug.Log("Updating OP points after using Prayer");
    }
}
