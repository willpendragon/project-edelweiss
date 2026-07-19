using DG.Tweening;
using ProjectEdelweiss.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            enemyAgent.isTurnComplete = true;
            Debug.Log($"<color=cyan>[RockEnemyBehavior] {enemyAgent.name} turn complete (Dead/Stunned)</color>");
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
        DOVirtual.DelayedCall(actionDelay, () =>
        {
            enemyAgent.isTurnComplete = true;
            Debug.Log($"<color=cyan>[RockEnemyBehavior] {enemyAgent.name} turn complete (Second action done)</color>");
            OnCheckPlayer?.Invoke();
        });
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
        TileController startTile = enemyUnit.ownedTile;

        if (startTile == null || targetTile == null) return;

        List<TileController> fullPath = RetracePathToTarget(startTile, targetTile);

        if (fullPath == null || fullPath.Count == 0) return;

        // Uses property from BumperEnemyBehavior
        // By default we use 4, ideally this should pull from a protected movementLimit in the base class.
        List<TileController> limitedPath = LimitPath(fullPath, 4, targetTile); 

        // REMOVED: In standard Enemy AI, the last tile is removed because it belongs to the player. 
        // Here, the target is an environmental tile, so we MUST land exactly on it! We do NOT remove the last index.

        // Backtrack to find a valid tile that isn't occupied by a prize or unit
        while (limitedPath.Count > 0)
        {
            TileController prospectiveDestination = limitedPath.Last();
            
            if (IsTileValidDestination(prospectiveDestination))
            {
                // Substitute the instant teleport with your new 3D step-by-step visual sequencer
                AnimateMovementAlongPath(enemyUnit, limitedPath);

                // Check if the destination we actually landed on matches the elemental tile we wanted
                if (prospectiveDestination.tileElement == targetTile.tileElement)
                {
                    enemyAgent.ReceiveElement(prospectiveDestination, enemyAgent);
                    OnRockEnemyAttack?.Invoke($"{enemyUnit.unitTemplate.unitName} receives {enemyAgent.elementalImbue} buff");
                }
                return;
            }
            
            limitedPath.RemoveAt(limitedPath.Count - 1);
        }
    }
}
