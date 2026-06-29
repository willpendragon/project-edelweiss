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
        // 1. Assicuriamoci che la flag sia falsa all'inizio del turno
        enemyAgent.isTurnComplete = false; 

        if (enemyAgent.gameObject.tag == "DeadEnemy" ||
            enemyAgent.GetComponentInParent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            Debug.Log("Enemy is dead and cannot act.");
            enemyAgent.isTurnComplete = true; // Ha finito!
            OnCheckPlayer?.Invoke();
            return;
        }

        if (enemyAgent.GetComponentInParent<Unit>().unitStatusController.unitCurrentStatus == UnitStatus.stun)
        {
            OnMovementDisabled($"{enemyAgent.GetComponentInParent<Unit>().unitTemplate.unitName} can't move...");
            enemyAgent.isTurnComplete = true; // Ha finito!
            OnCheckPlayer?.Invoke();
            return;
        }

        Unit enemyUnit = enemyAgent.GetComponent<Unit>();

        PerformNextAction(enemyUnit, enemyAgent, actionsPerTurn);
    }

    private void PerformNextAction(Unit enemyUnit, EnemyAgent enemyAgent, int actionsLeft)
    {
        // 2. Se non ci sono più azioni, il turno è UFFICIALMENTE concluso
        if (actionsLeft <= 0)
        {
            enemyAgent.isTurnComplete = true; 
            OnCheckPlayer?.Invoke();
            return;
        }

        Unit targetPlayerUnit = enemyAgent.EnemyAIPriority.SelectTargetPlayerUnit(enemyUnit);

        // 3. Se non ci sono bersagli validi, il turno è concluso
        if (targetPlayerUnit == null || targetPlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            enemyAgent.isTurnComplete = true;
            OnCheckPlayer?.Invoke();
            return;
        }

        // Validate range and attack.
        if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
        {
            PerformAttack(enemyUnit, enemyAgent, targetPlayerUnit, actionsLeft);
            // Nessun DOVirtual qui sotto, perché è già gestito nel callback di PerformAttack
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
                // 4. Se non può attaccare e non può muoversi (percorso bloccato), il turno finisce qui
                enemyAgent.isTurnComplete = true;
                OnCheckPlayer?.Invoke();
            }
        }
    }

    public bool CheckAttackRange(TileController attackerTile, TileController defenderTile)
    {
        // 1. Calculate Manhattan distance purely on the horizontal plane
        int horizontalDistance = Mathf.Abs(attackerTile.gridPosition.x - defenderTile.gridPosition.x) +
                                 Mathf.Abs(attackerTile.gridPosition.z - defenderTile.gridPosition.z);

        // 2. Check the elevation difference
        int verticalDistance = Mathf.Abs(attackerTile.gridPosition.y - defenderTile.gridPosition.y);

        // Valid if horizontally in range AND vertically adjacent (max 1 tile elevation difference)
        bool inRange = horizontalDistance <= meleeRange && verticalDistance <= 1;

        Debug.Log(inRange
            ? $"Enemy is within attack range. (H-Dist: {horizontalDistance}, V-Dist: {verticalDistance})"
            : $"Enemy is out of attack range. (H-Dist: {horizontalDistance}, V-Dist: {verticalDistance})");

        return inRange;
    }

    private void PerformAttack(Unit enemyUnit, EnemyAgent enemyAgent, Unit targetPlayerUnit, int actionsLeft)
    {
        float baseDamage = enemyUnit.unitMeleeAttackBaseDamage;
        float proximityModifier = 1.5f;
        float finalDamage = baseDamage;

        if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
        {
            finalDamage *= proximityModifier;
        }

        DefenseRequirement defReq = DefenseRequirement.Parryable;

        enemyAgent.StartAttackSequence(targetPlayerUnit, finalDamage, defReq,
            () =>
            {
                DOVirtual.DelayedCall(actionDelay, () => PerformNextAction(enemyUnit, enemyAgent, actionsLeft - 1));
            });

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
        while (limitedPath.Count > 0)
        {
            TileController prospectiveDestination = limitedPath.Last();

            if (IsTileValidDestination(prospectiveDestination))
            {
                // Substitute the instant teleport with our step-by-step visual sequencer
                AnimateMovementAlongPath(enemyUnit, limitedPath);
                return true;
            }

            // Tile is occupied by another Unit or a Prize, step backward by 1 evaluating the previous tile.
            limitedPath.RemoveAt(limitedPath.Count - 1);
        }

        // Failed to find any suitable ground along the path
        return false;
    }

    public void AnimateMovementAlongPath(Unit unit, List<TileController> path)
    {
        if (path == null || path.Count == 0) return;

        TileController startTile = unit.ownedTile;
        TileController destinationTile = path.Last();

        // Instantly update logical coordinates
        startTile.detectedUnit = null;
        startTile.currentSingleTileCondition = SingleTileCondition.free;

        unit.ownedTile = destinationTile;
        destinationTile.detectedUnit = unit.gameObject;
        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

        unit.currentXCoordinate = destinationTile.tileXCoordinate;
        unit.currentYCoordinate = destinationTile.tileYCoordinate;

        DG.Tweening.Sequence movementSequence = DG.Tweening.DOTween.Sequence();
        float stepDelay = 0.05f; // Matches the exact yield WaitForSeconds from Player's FollowPath

        foreach (TileController stepTile in path)
        {
            // Snap the unit to the tile exactly like the Player does
            movementSequence.AppendCallback(() =>
            {
                GridManager.Instance.PlaceUnitOnTileSurface(unit.gameObject, stepTile);
            });

            // Wait a tiny fraction of a second before the next step
            movementSequence.AppendInterval(stepDelay);
        }

        Debug.Log(
            $"Unit snap-animating along path to: ({destinationTile.tileXCoordinate}, {destinationTile.tileYCoordinate})");
    }

    protected List<TileController> RetracePathToTarget(TileController startTile, TileController targetTile)
    {
        List<TileController> openSet = new List<TileController> { startTile };
        HashSet<TileController> closedSet = new HashSet<TileController>();

        startTile.gCost = 0;
        startTile.hCost = GetDistance(startTile, targetTile);
        startTile.parent = null;

        // Variables to remember the closest tile we could reach during the search
        TileController bestTile = startTile;
        int bestDistance = startTile.hCost;

        while (openSet.Count > 0)
        {
            TileController currentTile = openSet.OrderBy(tile => tile.FCost).First();
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            // Track the tile closest to the target in case the path is blocked
            if (currentTile.hCost < bestDistance)
            {
                bestDistance = currentTile.hCost;
                bestTile = currentTile;
            }

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

        // --- FALLBACK PARTIAL PATHING ---
        // If the player is completely walled off (e.g., by an ally on the stairs),
        // return the path routing us as close as possible instead of giving up!
        if (bestTile != startTile)
        {
            return RetracePath(startTile, bestTile);
        }

        Debug.LogWarning("No valid path found to the target.");
        return null;
    }

    protected List<TileController> LimitPath(List<TileController> fullPath, int movementLimit,
        TileController targetTile)
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
        // Delegate pathfinding neighbor detection to the new 3D Voxel controller
        if (GridManager.Instance != null && GridManager.Instance.gridMovementController != null)
        {
            return GridManager.Instance.gridMovementController.GetNeighbours(tile);
        }

        // Fallback for safety (Legacy 2D)
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

        // FIX: Use Voxel-safe placement taking the exact physical Y position of the target block, 
        // avoiding top-down raycast assumptions which mistakenly beamed units to the roof.
        GridManager.Instance.PlaceUnitOnTileSurface(unit.gameObject, destinationTile);

        unit.currentXCoordinate = destinationTile.tileXCoordinate;
        unit.currentYCoordinate = destinationTile.tileYCoordinate;

        Debug.Log(
            $"Unit moved to voxel block: ({destinationTile.gridPosition.x}, {destinationTile.gridPosition.y}, {destinationTile.gridPosition.z})");
    }

    private int GetDistance(TileController tileA, TileController tileB)
    {
        int dstX = Mathf.Abs(tileA.gridPosition.x - tileB.gridPosition.x);
        int dstZ = Mathf.Abs(tileA.gridPosition.z - tileB.gridPosition.z);
        int dstY = Mathf.Abs(tileA.gridPosition.y - tileB.gridPosition.y);

        // Strict Manhattan distance forces 4-way pathing without diagonal preferences
        return dstX + dstZ + dstY;
    }
}