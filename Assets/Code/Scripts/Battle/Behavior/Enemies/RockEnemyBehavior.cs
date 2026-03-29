using DG.Tweening;
using ProjectEdelweiss.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "RockEnemyBehavior", menuName = "EnemyBehavior/RockEnemy")]
public class RockEnemyBehavior : BumperEnemyBehavior
{
    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void RockEnemyAttack(string notification);
    public static event RockEnemyAttack OnRockEnemyAttack;

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        Unit enemyUnit = enemyAgent.GetComponent<Unit>();

        if (enemyUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead ||
            enemyUnit.unitStatusController.unitCurrentStatus == UnitStatus.stun)
        {
            OnCheckPlayer?.Invoke();
            return;
        }
        PerformFirstAction(enemyUnit, enemyAgent);
    }

    private void PerformFirstAction(Unit enemyUnit, EnemyAgent enemyAgent)
    {
        if (enemyAgent.elementalImbue == EnemyAgent.ElementalImbue.None)
        {
            TileController targetTile = enemyAgent.EnemyAITilePriority.SelectTargetTile(enemyUnit);

            if (targetTile != null)
            {
                MoveEnemyToTileTarget(targetTile, enemyAgent);
            }
            else
            {
                Unit targetPlayerUnit = enemyAgent.EnemyAIPriority.SelectTargetPlayerUnit(enemyUnit);
                if (targetPlayerUnit != null)
                {
                    MoveEnemyToPlayerTarget(targetPlayerUnit, enemyAgent);
                }
            }
        }
        else
        {
            Unit targetPlayerUnit = enemyAgent.EnemyAIPriority.SelectTargetPlayerUnit(enemyUnit);

            if (targetPlayerUnit != null)
            {
                if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
                {
                    PerformAttack(enemyUnit, enemyAgent, targetPlayerUnit);
                }
                else
                {
                    MoveEnemyToPlayerTarget(targetPlayerUnit, enemyAgent);
                }
            }
        }

        var camDistanceController = GameObject.FindGameObjectWithTag(GameTags.CAMERA_DISTANCE_CONTROLLER).GetComponent<CameraDistanceController>();
        camDistanceController.SortUnits(); // Updates units Z-order.

        // Passa alla seconda mossa
        DOVirtual.DelayedCall(actionDelay, () => PerformSecondAction(enemyUnit, enemyAgent));
    }

    private void PerformSecondAction(Unit enemyUnit, EnemyAgent enemyAgent)
    {
        if (enemyAgent.elementalImbue == EnemyAgent.ElementalImbue.None) // Enemy is not charged with an Element.
        {
            TileController targetTile = enemyAgent.EnemyAITilePriority.SelectTargetTile(enemyUnit); // Search again.
            if (targetTile != null)
            {
                MoveEnemyToTileTarget(targetTile, enemyAgent);
            }
        }
        else // Enemy is charged with an Elemental Imbue, so it can go after a Player Unit to attack.
        {
            Unit targetPlayerUnit = enemyAgent.EnemyAIPriority.SelectTargetPlayerUnit(enemyUnit);

            if (targetPlayerUnit != null)
            {
                if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
                {
                    PerformAttack(enemyUnit, enemyAgent, targetPlayerUnit); // Is Charged AND in Range, Rock attacks.
                }
                else
                {
                    MoveEnemyToPlayerTarget(targetPlayerUnit, enemyAgent); // Is Charged but not in Range, Rock moves closer to Player Unit.
                }
            }
        }
        DOVirtual.DelayedCall(actionDelay, () => OnCheckPlayer?.Invoke());
    }

    private void PerformAttack(Unit enemyUnit, EnemyAgent enemyAgent, Unit targetPlayerUnit)
    {
        float baseDamage = enemyUnit.unitMeleeAttackBaseDamage;
        float proximityModifier = 1.5f;
        float finalDamage = baseDamage;

        if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
        {
            finalDamage *= proximityModifier;
        }

        targetPlayerUnit.TakeDamage(finalDamage);
        targetPlayerUnit.OnTakenDamage.Invoke(finalDamage);

        enemyAgent.gameObject.GetComponentInChildren<BattleFeedbackController>()
            .PlayMeleeAttackAnimation(enemyUnit, targetPlayerUnit);

        OnRockEnemyAttack?.Invoke($"{enemyUnit.unitTemplate.unitName} used Elemental Bump");
    }

    private void MoveEnemyToTileTarget(TileController targetTile, EnemyAgent enemyAgent) // Reach Tile, receive imbue.
    {
        Unit enemyUnit = enemyAgent.GetComponent<Unit>();
        MoveUnitToTile(enemyUnit, targetTile);
        enemyAgent.ReceiveElement(targetTile, enemyAgent);
        OnRockEnemyAttack?.Invoke($"{enemyUnit.unitTemplate.unitName} receives {enemyAgent.elementalImbue} buff");
    }
}
