using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Behavior", menuName = "Moveset/RangedBehavior")]
public class RangedBehavior : PhysicalAttackBehavior
{
    [Header("Ranged Specifics")]
    public int maxAttackRange = 10;
    public int minAttackRange = 2;

    [Header("Ranged Damage Modifiers")]
    [Tooltip("Percentage damage increase per flat tile of distance (e.g. 0.1 for +10% per tile)")]
    public float distanceDamageMultiplier = 0.1f;
    [Tooltip("Percentage damage increase per tile of vertical advantage (e.g. 0.15 for +15% per tile higher)")]
    public float heightDamageMultiplier = 0.15f;

    public override void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        // 1. Get completely accurate 3D Voxel positions
        Vector3Int attackerPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int targetPos = targetTile.gridPosition;

        if (targetUnit != null && targetUnit.ownedTile != null)
        {
            targetPos = targetUnit.ownedTile.gridPosition;
        }

        // 2. Calculate Distance (Chebyshev / Square style on the flat X/Z plane)
        int distanceX = Mathf.Abs(attackerPos.x - targetPos.x);
        int distanceZ = Mathf.Abs(attackerPos.z - targetPos.z);
        int flatDistance = Mathf.Max(distanceX, distanceZ);

        // 3. Validate Range (We enforce Minimum and Maximum horizontally only, ignoring Y to prevent illegal close drop-shots)
        if (flatDistance < minAttackRange || flatDistance > maxAttackRange)
        {
            Debug.LogWarning($"Attack canceled! Target is at flat distance {flatDistance}, but range is {minAttackRange}-{maxAttackRange}.");
            return;
        }

        // 4. Handle Obstacles
        if (targetTile.tileType == TileType.Obstacle)
        {
            // Subtract Opportunity Points
            activePlayerUnit.unitOpportunityPoints -= 1;

            // Hit the beacon if it exists
            Beacon beacon = targetTile.detectedUnit?.GetComponent<Beacon>();
            if (beacon != null) beacon.OnHitByUnit();

            // Trigger Animation 
            // activePlayerUnit.GetComponent<BattleFeedbackController>().PlayRangedAttackAnimation(activePlayerUnit, targetUnit);

            return;
        }

        // 5. Handle Unit Attacks (Direct Damage calculation allowing custom height/range multipliers)
        if (targetUnit != null && targetUnit.unitType != Unit.UnitType.Deity)
        {
            // Subtract Opportunity Points
            activePlayerUnit.unitOpportunityPoints -= 1;

            // Calculate vertical difference
            int elevationDifference = attackerPos.y - targetPos.y;

            // Calculate base identical to old HitTarget setup
            float baseDamageOutput = activePlayerUnit.unitAttackPower * activePlayerUnit.unitMeleeAttackBaseDamage;
            
            // Apply Distant & Elevation Multipliers
            float distanceBonus = distanceDamageMultiplier * flatDistance;
            float elevationBonus = elevationDifference > 0 ? (heightDamageMultiplier * elevationDifference) : 0f;

            // Compile the final damage output
            float finalDamage = baseDamageOutput * (1f + distanceBonus + elevationBonus);

            // Apply Damage directly and fire UI text via Broadcast
            targetUnit.TakeDamage(finalDamage);
            BroadcastAttackNotification($"{activePlayerUnit.unitTemplate.unitName} used Ranged Attack");

            Debug.Log($"{activePlayerUnit.unitTemplate.unitName} fired a ranged attack at {targetUnit.unitTemplate.unitName}! Distance: {flatDistance}, Elevation: {elevationDifference}, Final Damage: {finalDamage}");
        }
    }

    public override int GetAttackRange()
    {
        return maxAttackRange;
    }
}