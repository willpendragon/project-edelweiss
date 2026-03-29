using UnityEngine;
using Edelweiss.Core;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    private int meleeRange;

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0 ||
            activePlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead ||
            activePlayerUnit.unitStatusController.unitCurrentStatus == UnitStatus.Faithless || // Negating Melee if Player is Faithless
            !IsEnemyReachable(activePlayerUnit, targetTile))
            return;

        GameObject enemyObject = targetTile.detectedUnit;
        activePlayerUnit.unitTemplate.physicAttackBehavior.AttackSequence(targetTile.detectedUnit.GetComponent<Unit>(), targetTile, activePlayerUnit);
        ResetTileColours();
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);
        targetTile.tileShaderController.SetTileGlowIntensity(1f);
    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        // Use centralized logic.
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate.unitName);
    }

    public void ResetTileColours()
    {
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
        }
    }
    private bool IsEnemyReachable(Unit activePlayerUnit, TileController targetTile)
    {
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, targetTile);

        meleeRange = activePlayerUnit.unitTemplate.physicAttackBehavior.GetAttackRange(); // Retrieve melee range from SO.

        if (distance > meleeRange)
        {
            return false;
        }
        return true;
    }
}
