using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BumperEnemyBehavior;

[CreateAssetMenu(fileName = "BumperEnemyBehavior", menuName = "EnemyBehavior/BumperEnemy")]
public class BumperEnemyBehavior : EnemyBehavior
{
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void BumperEnemyAttack(string attackName, string attackerName);
    public static event BumperEnemyAttack OnBumperEnemyAttack;

    [SerializeField] GameObject attackVFXAnimator;
    //public float attackPower = 1;
    //Beware, magic number


    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag != "DeadEnemy" && enemyAgent.gameObject.GetComponentInParent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        //The Enemy Unit doesn't evaluate the next move if it's dead.
        {
            //Enemy Unit selects the Target
            Unit enemyUnit = enemyAgent.gameObject.GetComponent<Unit>();
            Unit targetPlayerUnit = SelectTargetPlayerUnit();
            MoveToPlayerTarget(targetPlayerUnit, enemyAgent);
            float reducedDamage = enemyUnit.unitMeleeAttackBaseDamage; //* damageReductionFactor//
            enemyAgent.gameObject.GetComponentInChildren<BattleFeedbackController>().PlayMeleeAttackAnimation(enemyUnit, targetPlayerUnit);
            OnBumperEnemyAttack("Bump", "Godling");

            if (GridManager.Instance.GetComponentInChildren<DistanceController>().CheckDistance(enemyUnit.ownedTile.GetComponent<TileController>(), targetPlayerUnit.ownedTile.GetComponent<TileController>()))
            {
                float attackProximityModifier = 1.5f;
                reducedDamage = reducedDamage * attackProximityModifier;
                targetPlayerUnit.TakeDamage(reducedDamage);
                targetPlayerUnit.OnTakenDamage.Invoke(reducedDamage);
                Debug.Log("If Enemy is near the Player Unit, the Attack Proximity Modifier applies");
            }
            else
            {
                targetPlayerUnit.TakeDamage(reducedDamage);
                targetPlayerUnit.OnTakenDamage.Invoke(reducedDamage);
            }

            //const float targetUnitReductionFactor = 0.05f;
            //float damageReductionFactor = (1.0f - (targetUnit.unitAmorRating * targetUnitReductionFactor) / (1.0f + targetUnitReductionFactor * targetUnitReductionFactor));

            //27032024 Note: Reintroduce attack feedback here.
            OnCheckPlayer();

            Debug.Log("Enemy Attacking");

            //opportunity -= 1;
        }
        else
        {
            Debug.Log("Enemy Unit is dead and can't attack anymore");
        }
    }
    public Unit SelectTargetPlayerUnit()
    {
        // This method selects the Target Unit choosing from the Player Unit with the Lowest HP among those that are not dead
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        Unit targetUnit = playerUnitsOnBattlefield
            .Select(go => go.GetComponent<Unit>())
            .Where(unit => unit != null && unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead) // Filter out null and dead units
            .OrderBy(unit => unit.unitHealthPoints) // Order by HP ascending to find the lowest
            .FirstOrDefault(); // Take the first, which is the one with the lowest HP among alive units

        // If an alive unit is found, return it, otherwise null (indicating no valid target)
        return targetUnit; // No need to check for dead here, as we've already filtered them out
    }
    public void MoveToPlayerTarget(Unit defenderPlayerUnit, EnemyAgent enemyAttacker)
    {
        Unit enemyUnit = enemyAttacker.gameObject.GetComponent<Unit>();

        if (enemyUnit.unitStatusController.unitCurrentStatus == UnitStatus.stun)
        {
            Debug.Log("Enemy Unit is stunned and can't move");
            return; // Early exit if the unit is stunned
        }

        if (CheckDistanceBetweenUnits(defenderPlayerUnit.ownedTile, enemyAttacker.gameObject.GetComponent<Unit>().ownedTile))
        {
            Debug.Log("Enemy Unit is near Player Target. Enemy Unit stays still");
        }
        else
        {
            List<TileController> destinationNeighborTilesList = GridManager.Instance.GetComponentInChildren<GridMovementController>().GetNeighbours(defenderPlayerUnit.ownedTile);

            // Filter for free tiles only
            List<TileController> freeTiles = destinationNeighborTilesList.Where(tile => tile.currentSingleTileCondition == SingleTileCondition.free).ToList();

            // Strategy change: Choose the tile that minimizes the distance to the target
            TileController closestTile = null;
            float closestDistance = float.MaxValue;
            foreach (var tile in freeTiles)
            {
                float distance = GridManager.Instance.gridMovementController.GetDistance(tile, defenderPlayerUnit.ownedTile);
                if (distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = distance;
                }
            }

            if (closestTile != null)
            {
                // Move the unit to the closest tile
                MoveUnitToTile(enemyAttacker.GetComponent<Unit>(), closestTile);
                Debug.Log("Enemy Unit moves closer to Player Unit.");
            }
            else
            {
                Debug.Log("No free tiles available to move closer.");
            }
        }
    }

    private void MoveUnitToTile(Unit unit, TileController destinationTile)
    {
        // Check if the unit is stunned before moving
        if (unit.unitStatusController.unitCurrentStatus == UnitStatus.stun)
        {
            Debug.Log("This Bumper Enemy Unit is stunned and can't move");
            return; // Early exit if the unit is stunned
        }

        // Move and update the unit's tile
        if (unit.MoveUnit(destinationTile.tileXCoordinate, destinationTile.tileYCoordinate, false))
        {
            if (destinationTile.currentSingleTileCondition == SingleTileCondition.free)
            {
                unit.ownedTile.detectedUnit = null;
                unit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                //160720240901 Correct
                GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
                unit.ownedTile = destinationTile;
                destinationTile.detectedUnit = unit.gameObject;
                destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;
                Debug.Log("Bumper Enemy Moved");
                // Potentially update visuals or other game elements here as needed
            }
        }
        else
        {
            Debug.Log("This Bumper Enemy Unit can't move");
        }
    }



    public bool CheckDistanceBetweenUnits(TileController attackerTile, TileController defenderTile)
    {
        GridMovementController gridMovementController = GridManager.Instance.gridMovementController;
        int distance = gridMovementController.GetDistance(attackerTile, defenderTile);

        if (distance < 2) // If the distance is less than 2, they are close.
        {
            Debug.Log("Distance Check: Enemy Attacker is close to Defender. Enemy will stay in place.");
            return true;
        }
        else // If the distance is 2 or more, they are not close.
        {
            Debug.Log("Distance Check: Enemy Attacker is distant from Defender. Enemy Attack will move towards Defender");
            return false;
        }
    }
}
