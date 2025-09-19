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

    public void Execute(TileController targetTile)
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Deity linkedDeity = currentActivePlayerUnit.linkedDeity;

        if (linkedDeity != null && currentActivePlayerUnit.unitOpportunityPoints > 0 && deityLimiter > 0 &&
            targetTile.currentSingleTileCondition == SingleTileCondition.free)
        {
            targetTile.currentSingleTileCondition = SingleTileCondition.occupiedByDeity;
            targetTile.GetComponentInChildren<TileShaderController>()?.AnimateFadeHeight(3f, 0f, Color.green);

            int summoningCost = 10;
            currentActivePlayerUnit.SpendManaPoints(summoningCost);
            SummonDeityOnBattlefield(linkedDeity, currentActivePlayerUnit, targetTile);

            deityLimiter--;

            //string prayLeftMouseButtonInstructionsText = "LMB - Select/Confirm Summon for Praying";
            //string prayRightMouseButtonInstructionsText = "-";
            //InstructionsPanelController.Instance.UpdateInstructions(prayLeftMouseButtonInstructionsText, prayRightMouseButtonInstructionsText);
        }
    }

    public void Deselect()
    {
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

        BattleInterface.Instance.CreateUISummonInfoPanel(deityInstance);
        currentActivePlayerUnit.summonedLinkedDeity = deityInstance.GetComponent<Deity>();
    }
}
