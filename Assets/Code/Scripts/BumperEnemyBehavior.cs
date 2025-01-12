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
        // Skip execution if the enemy is dead
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
        // Select the player unit with the lowest HP
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

        List<TileController> limitedPath = LimitPath(fullPath, movementLimit);

        if (limitedPath.Count == 0)
        {
            Debug.Log("No tiles within movement limit.");
            return;
        }

        // Get the next destination tile within the limit
        TileController destinationTile = limitedPath.Last();

        // Prevent stepping onto the target's tile
        if (destinationTile == targetTile)
        {
            Debug.Log("Attempted to move to the target's tile. Adjusting movement.");
            if (limitedPath.Count > 1)
            {
                destinationTile = limitedPath[limitedPath.Count - 2]; // Move to the second-last tile in the path
            }
            else
            {
                Debug.Log("No valid tile to move to within the limit.");
                return;
            }
        }

        if (destinationTile == null || destinationTile.currentSingleTileCondition != SingleTileCondition.free)
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
                // Skip occupied tiles or already visited tiles
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


    private List<TileController> LimitPath(List<TileController> fullPath, int movementLimit)
    {
        return fullPath.Take(movementLimit).ToList();
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

        // Define offsets for adjacent tiles (up, down, left, right)
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

        Debug.Log($"Unit moved to tile: ({destinationTile.tileXCoordinate}, {destinationTile.tileYCoordinate})");
    }

    private int GetDistance(TileController tileA, TileController tileB)
    {
        return Mathf.Abs(tileA.tileXCoordinate - tileB.tileXCoordinate) +
               Mathf.Abs(tileA.tileYCoordinate - tileB.tileYCoordinate);
    }
}
