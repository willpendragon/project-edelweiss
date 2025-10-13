using UnityEngine;
using DG.Tweening;
using Edelweiss.Core;

public class SummonPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public int deityLimiter = 1;

    public void Select(TileController selectedTile)
    {
    }
    public void Deselect()
    {
    }

    public void Execute(TileController targetTile)
    {
        if (targetTile.currentSingleTileCondition != SingleTileCondition.free)
            return;

        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Deity linkedDeity = currentActivePlayerUnit.linkedDeity;

        if (linkedDeity == null)
            return;

        if (currentActivePlayerUnit.unitOpportunityPoints <= 0)
            return;

        if (currentActivePlayerUnit.summonedLinkedDeity != null)
            return;

        //targetTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
        //targetTile.GetComponentInChildren<TileShaderController>()?.AnimateFadeHeight(3f, 0f, Color.green);

        int summoningCost = 10;
        currentActivePlayerUnit.SpendManaPoints(summoningCost);
        // A smaller version of the Deity appears on the Battlefield.

        SummonDeityOnBattlefield(linkedDeity, currentActivePlayerUnit, targetTile);

        // Darkened environment VFX Plays
        StageMoodController.Instance.ActivateDarkness();
        // Resets darkness VFX
        float darknessResetWaitTime = 1.5f;
        StageMoodController.Instance.StartResetDarkness(darknessResetWaitTime);
        // Play Summoning SFX.
        currentActivePlayerUnit.battleFeedbackController.PlayPrayerSFX.Invoke();

        var summonedDeity = currentActivePlayerUnit.summonedLinkedDeity;
        // Hide the Deity stats bar
        summonedDeity.GetComponentInChildren<DeityHealthBar>().HideBars();

        // Apply the summoning buff.
        summonedDeity.summoningBehaviour.ExecuteBehavior(summonedDeity);

        // Set the Deity as summoned.
        summonedDeity.currentDeityStatus = Deity.DeityStatus.Summoned;

        // Display Notification
        string summonedLinkedDeityUnitName = summonedDeity.gameObject.GetComponent<Unit>().unitTemplate.unitName;
        string currentActivePlayerUnitName = currentActivePlayerUnit.gameObject.GetComponent<Unit>().unitTemplate.unitName;
        BattleInterface.Instance.SetSummonEffectNameOnNotificationPanel(summonedLinkedDeityUnitName, currentActivePlayerUnitName);
        // Make Summoned Deity disappear from the battlefield.
        ResetSummon(currentActivePlayerUnit, summonedDeity);
    }
    private void ResetSummon(Unit currentActivePlayerUnit, Deity summonedLinkedDeity)
    {
        PlayDespawnTween(summonedLinkedDeity.gameObject);
        //Destroy(summonedLinkedDeity.gameObject, 3);
    }

    private void SummonDeityOnBattlefield(Deity linkedDeity, Unit currentActivePlayerUnit, TileController targetTile)
    {
        var summonPosition = targetTile.transform.position + new Vector3(0, 3, 0);
        GameObject deityInstance = Instantiate(linkedDeity.gameObject, summonPosition, Quaternion.identity);

        deityInstance.transform.localScale = Vector3.zero;
        deityInstance.transform.DOMoveY(summonPosition.y + 1f, 0.3f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);

        deityInstance.transform.DOScale(new Vector3(2, 2, 2), 0.5f)
            .SetEase(Ease.OutBack);

        //BattleInterface.Instance.CreateUISummonInfoPanel(deityInstance);
        currentActivePlayerUnit.summonedLinkedDeity = deityInstance.GetComponent<Deity>();
    }

    private void PlayDespawnTween(GameObject deityGO)
    {
        Sequence despawnSequence = DOTween.Sequence();

        despawnSequence.AppendInterval(2.5f);

        // Quick scale shake to simulate a flicker or instability
        despawnSequence.Append(deityGO.transform.DOShakeScale(0.2f, strength: 0.2f, vibrato: 10)
            .SetEase(Ease.OutQuad));

        // Instantly shrink (like vanishing)
        despawnSequence.Append(deityGO.transform.DOScale(Vector3.zero, 0.1f)
            .SetEase(Ease.InFlash));

        // Very quick up/down movement
        despawnSequence.Join(deityGO.transform.DOMoveY(deityGO.transform.position.y + 0.2f, 0.1f));

        despawnSequence.OnComplete(() =>
        {
            //Destroy(deityGO);
            Debug.Log("Summoned Deity teleported away.");
        });
    }
}