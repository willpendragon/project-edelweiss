using UnityEngine;

public class PrayPlayerAction : MonoBehaviour, IPlayerAction
{
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
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
        if (savedSelectedTile.currentSingleTileCondition == SingleTileCondition.occupiedByDeity && currentActivePlayerUnit.unitOpportunityPoints > 0)
        {
            if (CheckLinkedDeityPrayerPower())
            {
                BattleInterface.Instance.summonedUnitInfoPanelHelper.PrayDeity();
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