using UnityEngine;
using Edelweiss.Core;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    //public int selectionLimiter = 1;
    private int meleeRange = 2; // Move to attack SO

    //public Vector2Int knockbackDirection;
    //public int knockbackStrength = 2;

    //public delegate void UsedMeleeAction(string notification);
    //public static event UsedMeleeAction OnUsedMeleeAction;

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
        // Insert Interaction Logic here (for mirrors) - should have a dedicated behaviur

        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);

        //OnUsedMeleeAction($"{activePlayerUnit.unitTemplate.unitName} used Melee Attack");
        //activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, defender);
        // After executing a Melee Attack, resets the Enemy initial tile (typically, shows the Movement Range - must take into account other cases in the future).
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
        if (distance > meleeRange)
        {
            return false;
        }
        return true;
    }
}
