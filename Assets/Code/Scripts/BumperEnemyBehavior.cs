using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BumperEnemyBehavior", menuName = "EnemyBehavior/BumperEnemy")]
public class BumperEnemyBehavior : EnemyBehavior
{
    [SerializeField] int meleeRange = 2; // Attack range
    [SerializeField] int movementLimit = 4; // Movement limit
    [SerializeField] GameObject attackVFXAnimator;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void BumperEnemyAttack(string attackName, string attackerName);
    public static event BumperEnemyAttack OnBumperEnemyAttack;

    public override void ExecuteBehavior(EnemyAgent enemyAgent)
    {
        if (enemyAgent.gameObject.tag == "DeadEnemy" ||
            enemyAgent.GetComponentInParent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            Debug.Log("Enemy is dead and cannot act.");
            return;
        }

        Unit enemyUnit = enemyAgent.GetComponent<Unit>();
        Unit targetPlayerUnit = SelectTargetPlayerUnit();

        if (targetPlayerUnit == null)
        {
            Debug.Log("No valid target found for the enemy.");
            return;
        }

        if (CheckAttackRange(enemyUnit.ownedTile, targetPlayerUnit.ownedTile))
        {
            PerformAttack(enemyUnit, enemyAgent, targetPlayerUnit);
        }
        else
        {
            MoveEnemyToPlayerTarget(targetPlayerUnit, enemyAgent);
        }
    }

    public Unit SelectTargetPlayerUnit()
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>()
            .playerUnitsOnBattlefield;

        return playerUnitsOnBattlefield
            .Select(go => go.GetComponent<Unit>())
            .Where(unit => unit != null && unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            .OrderBy(unit => unit.unitHealthPoints)
            .FirstOrDefault();
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

        OnBumperEnemyAttack?.Invoke("Bump", enemyUnit.unitTemplate.unitName);
        OnCheckPlayer?.Invoke();

        Debug.Log($"Enemy attacked {targetPlayerUnit.unitTemplate.unitName} for {finalDamage} damage.");
    }

    public void MoveEnemyToPlayerTarget(Unit defenderPlayerUnit, EnemyAgent enemyAttacker)
    {
        Unit enemyUnit = enemyAttacker.GetComponent<Unit>();
        TileController startTile = enemyUnit.ownedTile;
        TileController targetTile = defenderPlayerUnit.ownedTile;

        if (startTile == null || targetTile == null)
        {
            Debug.LogError("Start or target tile is null. Cannot move enemy.");
            return;
        }

        List<TileController> fullPath = RetracePathToTarget(startTile, targetTile);

        if (fullPath == null || fullPath.Count == 0)
        {
            Debug.Log("No valid path to the target.");
            return;
        }

        List<TileController> limitedPath = LimitPath(fullPath, movementLimit, targetTile);

        if (limitedPath.Count == 0)
        {
            Debug.Log("No tiles within movement limit.");
            return;
        }

        TileController destinationTile = limitedPath.Last();

        // Commenting out the destination tile abort check temporarily for testing purposes.
        // if (destinationTile == targetTile)
        // {
        //     Debug.LogError("Final destination is still the target player's tile. Aborting movement.");
        //     return;
        // }

        if (destinationTile == null || destinationTile.currentSingleTileCondition != SingleTileCondition.free || destinationTile.detectedUnit != null)
        {
            Debug.Log("Destination tile is invalid or occupied.");
            return;
        }

        MoveUnitToTile(enemyUnit, destinationTile);

        Debug.Log($"Enemy moved closer to Player. Position: ({destinationTile.tileXCoordinate}, {destinationTile.tileYCoordinate})");
    }

    private List<TileController> RetracePathToTarget(TileController startTile, TileController targetTile)
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
                if (neighbor.currentSingleTileCondition == SingleTileCondition.occupied || closedSet.Contains(neighbor))
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

    private List<TileController> LimitPath(List<TileController> fullPath, int movementLimit, TileController targetTile)
    {
        return fullPath.Take(movementLimit).ToList(); // Temporarily allow paths that include the target
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

    private void MoveUnitToTile(Unit unit, TileController destinationTile)
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
