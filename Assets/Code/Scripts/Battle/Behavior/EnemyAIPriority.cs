using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Possible prioritization.
public enum TargetPriorityType
{
    LowestHealth,
    HighestHealth,
    Closest,
    Random
}

[CreateAssetMenu(fileName = "NewEnemyAIPriority", menuName = "EnemyBehavior/AIPriority")]
public class EnemyAIPriority : ScriptableObject
{
    [Header("Enemy AI Settings")]
    [Tooltip("Dictates how the Enemy prioritizes the target.")]
    public TargetPriorityType targetPriority;

    public Unit SelectTargetPlayerUnit(Unit attacker)
    {
        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>()
            .playerUnitsOnBattlefield;

        // Filter out dead units.
        IEnumerable<Unit> validTargets = playerUnitsOnBattlefield
            .Select(go => go.GetComponent<Unit>())
            .Where(unit => unit != null && unit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead);

        if (!validTargets.Any()) return null;

        switch (targetPriority)
        {
            case TargetPriorityType.LowestHealth:
                return validTargets.OrderBy(unit => unit.unitHealthPoints).FirstOrDefault();

            case TargetPriorityType.HighestHealth:
                return validTargets.OrderByDescending(unit => unit.unitHealthPoints).FirstOrDefault();

            case TargetPriorityType.Closest:
                return validTargets.OrderBy(unit => Vector3.Distance(attacker.transform.position, unit.transform.position)).FirstOrDefault();

            case TargetPriorityType.Random:
                return validTargets.OrderBy(unit => Random.value).FirstOrDefault();

            default:
                return validTargets.FirstOrDefault();
        }
    }
}