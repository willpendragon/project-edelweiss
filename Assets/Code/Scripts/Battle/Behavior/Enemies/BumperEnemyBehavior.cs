using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BumperEnemyBehavior", menuName = "EnemyBehavior/BumperEnemy")]
public class BumperEnemyBehavior : EnemyBehavior
{
    [SerializeField] int meleeRange = 2; // Attack range
    [SerializeField] int movementLimit = 4; // Movement limit
    [SerializeField] GameObject attackVFXAnimator;
    [SerializeField] int actionsPerTurn = 2; // Numero di azioni che può fare
    public float actionDelay = 1.0f; // Pausa tra un'azione e l'altra (per le animazioni)

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void BumperEnemyAttack(string notification);
    public static event BumperEnemyAttack OnBumperEnemyAttack;

    public delegate void MovementDisabled(string notification);
    public static event MovementDisabled OnMovementDisabled;

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag == "DeadEnemy" ||
            enemyAgent.GetComponentInParent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            Debug.Log("Enemy is dead and cannot act.");
            OnCheckPlayer?.Invoke();
            return;
        }

        if (enemyAgent.GetComponentInParent<Unit>().unitStatusController.unitCurrentStatus == UnitStatus.stun)
        {
            OnMovementDisabled($"{enemyAgent.GetComponentInParent<Unit>().unitTemplate.unitName} can't move...");
            OnCheckPlayer?.Invoke();
            return;
        }

        Unit enemyUnit = enemyAgent.GetComponent<Unit>();

        PerformNextAction(enemyUnit, enemyAgent, actionsPerTurn);
    }

    private void PerformNextAction(Unit enemyUnit, EnemyAgent enemyAgent, int actionsLeft)
    {
        if (actionsLeft <= 0)
        {
            OnCheckPlayer?.Invoke();
            return;
        }

        Unit targetPlayerUnit = enemyAgent.EnemyAIPriority.SelectTargetPlayerUnit(enemyUnit);

        if (targetPlayerUnit == null || targetPlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            OnCheckPlayer?.Invoke();
            return;
        }

        // Validate range and attack.
        if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
        {
            PerformAttack(enemyUnit, enemyAgent, targetPlayerUnit);

            DOVirtual.DelayedCall(actionDelay, () => PerformNextAction(enemyUnit, enemyAgent, actionsLeft - 1));
        }
        else
        {
            bool moveSuccess = MoveEnemyToPlayerTarget(targetPlayerUnit, enemyAgent);

            if (moveSuccess)
            {
                GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
                DOVirtual.DelayedCall(actionDelay, () => PerformNextAction(enemyUnit, enemyAgent, actionsLeft - 1));
            }
            else
            {
                OnCheckPlayer?.Invoke();
            }
        }
    }

    public bool CheckAttackRange(TileController attackerTile, TileController defenderTile)
    {
        int distance = GetDistance(attackerTile, defenderTile);
        bool inRange = distance <= meleeRange;

        Debug.Log(inRange
            ? "Enemy is within attack range."
            : "Enemy is out of attack range.");
        return inRange;
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

        OnBumperEnemyAttack?.Invoke($"{enemyUnit.unitTemplate.unitName} used Bump");
    }

    public bool MoveEnemyToPlayerTarget(Unit defenderPlayerUnit, EnemyAgent enemyAttacker)
    {
        Unit enemyUnit = enemyAttacker.GetComponent<Unit>();
        TileController startTile = enemyUnit.ownedTile;
        TileController targetTile = defenderPlayerUnit.ownedTile;

        if (startTile == null || targetTile == null) return false;

        List<TileController> fullPath = RetracePathToTarget(startTile, targetTile);

        if (fullPath == null || fullPath.Count == 0) return false;

        List<TileController> limitedPath = LimitPath(fullPath, movementLimit, targetTile);

        if (limitedPath.Count > 0 && limitedPath.Last() == targetTile)
        {
            limitedPath.RemoveAt(limitedPath.Count - 1);
        }

        // Backtrack the path until we find a finalized valid destination where we can stop
        // e.g. If the final planned tile has a prize on it, trim the path slightly shorter.
        while (limitedPath.Count > 0)
        {
            TileController prospectiveDestination = limitedPath.Last();
            
            if (IsTileValidDestination(prospectiveDestination))
            {
                MoveUnitToTile(enemyUnit, prospectiveDestination);
                return true;
            }
            
            // Tile is occupied by another Unit or a Prize, step backward by 1 evaluating the previous tile.
            limitedPath.RemoveAt(limitedPath.Count - 1);
        }

        // Failed to find any suitable ground along the path
        return false;
    }

    protected List<TileController> RetracePathToTarget(TileController startTile, TileController targetTile)
    {
        List<TileController> openSet = new List<TileController> { startTile };
        HashSet<TileController> closedSet = new HashSet<TileController>();

        startTile.gCost = 0;
        startTile.hCost = GetDistance(startTile, targetTile);
        startTile.parent = null;

        while (openSet.Count > 0)
        {
            TileController currentTile = openSet.OrderBy(tile => tile.FCost).First();
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile);
            }

            foreach (TileController neighbor in GetNeighbours(currentTile))
            {
                if (closedSet.Contains(neighbor) ||
                   (neighbor.currentSingleTileCondition == SingleTileCondition.occupied && neighbor != targetTile))
                {
                    continue;
                }

                int newCostToNeighbor = currentTile.gCost + GetDistance(currentTile, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetTile);
                    neighbor.parent = currentTile;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No valid path found to the target.");
        return null;
    }

    protected List<TileController> LimitPath(List<TileController> fullPath, int movementLimit, TileController targetTile)
    {
        return fullPath.Take(movementLimit).ToList(); // Temporarily allow paths that include the target.
    }

    private List<TileController> RetracePath(TileController startTile, TileController endTile)
    {
        List<TileController> path = new List<TileController>();
        TileController currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }

        path.Add(startTile);
        path.Reverse();
        return path;
    }

    private List<TileController> GetNeighbours(TileController tile)
    {
        List<TileController> neighbors = new List<TileController>();

        int[,] offsets = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int neighborX = tile.tileXCoordinate + offsets[i, 0];
            int neighborY = tile.tileYCoordinate + offsets[i, 1];

            TileController neighbor = GridManager.Instance.GetTileControllerInstance(neighborX, neighborY);

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public void MoveUnitToTile(Unit unit, TileController destinationTile)
    {
        TileController startTile = unit.ownedTile;

        startTile.detectedUnit = null;
        startTile.currentSingleTileCondition = SingleTileCondition.free;

        unit.ownedTile = destinationTile;
        destinationTile.detectedUnit = unit.gameObject;
        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

        unit.transform.position = GridManager.Instance.GetWorldPositionFromGridCoordinates(
            destinationTile.tileXCoordinate, destinationTile.tileYCoordinate);
        unit.transform.position += new Vector3(0, 0.5f, 0);
        unit.currentXCoordinate = destinationTile.tileXCoordinate;
        unit.currentYCoordinate = destinationTile.tileYCoordinate;

        Debug.Log($"Unit moved to tile: ({destinationTile.tileXCoordinate}, {destinationTile.tileYCoordinate})");
    }

    private int GetDistance(TileController tileA, TileController tileB)
    {
        return Mathf.Abs(tileA.tileXCoordinate - tileB.tileXCoordinate) +
               Mathf.Abs(tileA.tileYCoordinate - tileB.tileYCoordinate);
    }
}
