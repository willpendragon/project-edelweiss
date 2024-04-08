using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BumperEnemyBehavior", menuName = "EnemyBehavior/BumperEnemy")]
public class BumperEnemyBehavior : EnemyBehavior
{
    [SerializeField] int minEnemyMoveRollRange;
    [SerializeField] int maxEnemyMoveRollRange;
    public int opportunity;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    [SerializeField] GameObject attackVFXAnimator;
    public float attackPower;
    //Beware, magic number


    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag != "DeadEnemy" && enemyAgent.gameObject.GetComponentInParent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        //The Enemy Unit doesn't evaluate the next move if it's dead.
        {
            //Enemy Unit selects the Target
            Unit targetPlayerUnit = SelectTargetPlayerUnit();
            MoveToPlayerTarget(targetPlayerUnit, enemyAgent);
            float reducedDamage = enemyAgent.GetComponentInParent<Unit>().unitTemplate.meleeAttackPower; //* damageReductionFactor//
            targetPlayerUnit.HealthPoints -= (reducedDamage);
            targetPlayerUnit.OnTakenDamage.Invoke(reducedDamage);

            //const float targetUnitReductionFactor = 0.05f;
            //float damageReductionFactor = (1.0f - (targetUnit.unitAmorRating * targetUnitReductionFactor) / (1.0f + targetUnitReductionFactor * targetUnitReductionFactor));

            //27032024 Note: Reintroduce attack feedback here.
            OnCheckPlayer();

            Debug.Log("Enemy Attacking");

            opportunity -= 1;
        }
        else
        {
            Debug.Log("Enemy Unit is dead and can't attack anymore");
        }
    }
    public Unit SelectTargetPlayerUnit()
    {
        //This method selects the Target Unit choosing from the Player Unit with the Highest HP
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        Unit unitWithHighestHP = playerUnitsOnBattlefield
        .Select(go => go.GetComponent<Unit>())
        .Where(unit => unit != null)
        .OrderByDescending(unit => unit.unitHealthPoints)
        .FirstOrDefault();
        return unitWithHighestHP;
    }
    public void MoveToPlayerTarget(Unit defenderPlayerUnit, EnemyAgent enemyAttacker)
    {
        if (CheckDistanceBetweenUnits(defenderPlayerUnit.ownedTile, enemyAttacker.gameObject.GetComponent<Unit>().ownedTile) == true)
        {
            Debug.Log("Enemy Unit is near Player Target. Enemy Unit stays still");
        }
        else
        {
            // Get all neighboring tiles
            List<TileController> destinationNeighborTilesList = GridManager.Instance.GetComponentInChildren<GridMovementController>().GetNeighbours(defenderPlayerUnit.ownedTile);

            // Filter out the tiles that are free
            List<TileController> freeTiles = destinationNeighborTilesList.Where(tile => tile.currentSingleTileCondition == SingleTileCondition.free).ToList();

            // Check if there are any free tiles available
            if (freeTiles.Count > 0)
            {
                // Select a random free tile
                int randomIndex = UnityEngine.Random.Range(0, freeTiles.Count);
                TileController destinationTile = freeTiles[randomIndex];

                // Move the unit to the selected tile
                enemyAttacker.GetComponent<Unit>().ownedTile.detectedUnit = null;
                enemyAttacker.GetComponent<Unit>().ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                enemyAttacker.GetComponent<Unit>().MoveUnit(destinationTile.tileXCoordinate, destinationTile.tileYCoordinate);
                destinationTile.detectedUnit = enemyAttacker.gameObject;
                enemyAttacker.gameObject.GetComponent<Unit>().ownedTile = destinationTile;
                destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;
                GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
            }
            else
            {
                Debug.Log("No free tiles available");
            }
        }
    }
    //GetTileControllerInstance(defenderPlayerUnit.currentXCoordinate - 1, defenderPlayerUnit.currentYCoordinate - 1);

    //if (destinationTile.currentSingleTileCondition == SingleTileCondition.occupied)
    //{
    //    Debug.Log(Enemy Unit is near Player Target, but Enemy Unit stays still because the tile has already been occupied");
    //}
    //else
    //{
    //    enemyAttacker.gameObject.GetComponent<Unit>().MoveUnit(defenderPlayerUnit.currentXCoordinate - 1, defenderPlayerUnit.currentYCoordinate - 1);
    //    destinationTile.detectedUnit = enemyAttacker.gameObject;
    //    enemyAttacker.gameObject.GetComponent<Unit>().ownedTile = destinationTile;
    //        //    destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;
    //        //    Debug.Log("Enemy Unit is distant from Player Target. Moving Enemy Unit closer to Player Unit");
    //        }
    //    }
    //

    public bool CheckDistanceBetweenUnits(TileController attackerTile, TileController defenderTile)
    {
        //Change magic number later
        GridMovementController gridMovementController = GridManager.Instance.gridMovementController;
        if (gridMovementController.GetDistance(attackerTile, defenderTile) <= 2)
        {
            Debug.Log("Distance Check: Enemy Attacker is close to Defender. Enemy will stay in place.");
            return true;
        }
        else
        {
            Debug.Log("Distance Check: Enemy Attacker is distant from Defender. Enemy Attack will move towards Defender");
            return false;
        }
    }
}
