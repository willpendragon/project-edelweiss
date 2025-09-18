using UnityEngine;

public class PrayPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;

    public void Select(TileController selectedTile)
    {
    }

    public void Deselect()
    {
    }

    public void Execute(TileController targetTile)
    {
        Unit currentActivePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (targetTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity &&
            currentActivePlayerUnit.unitOpportunityPoints > 0)
        {
            if (CheckLinkedDeityPrayerPower())
            {
                BattleInterface.Instance.summonedUnitInfoPanelHelper.PrayDeity();
            }
        }
    }

    private bool CheckLinkedDeityPrayerPower()
    {
        Deity summonedLinkedDeity = GameObject.FindGameObjectWithTag("ActivePlayerUnit")
            .GetComponent<Unit>().summonedLinkedDeity;

        if (summonedLinkedDeity.deityPrayerPower <= summonedLinkedDeity.deityPrayerBuff.deityPrayerBuffThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}