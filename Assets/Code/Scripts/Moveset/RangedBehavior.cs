using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "New Ranged Behavior", menuName = "Moveset/RangedBehavior")]
public class RangedBehavior : PhysicalAttackBehavior
{
    [Header("Ranged Specifics")]
    public int maxAttackRange = 10;
    public int minAttackRange = 2;

    [Header("Ranged Damage Modifiers")]
    [Tooltip("Ranged modifier based on distance à la FFT. % damage increase per flat tile of distance (e.g.: 0.1 for +10% per tile)")]
    public float distanceDamageMultiplier = 0.1f;
    [Tooltip("Ranged modifier based on height à la FFT. % damage increase per tile of vertical advantage (e.g.: 0.15 for +15% per tile higher)")]
    public float heightDamageMultiplier = 0.15f;

    [Header("Visual Feedback")]
    public GameObject _projectilePrefab;
    // public string projectileResourcePath = "VFX/ArrowVFX"; // Harcoded, better to use const
    [Tooltip("How high the projectile arcs into the air")]
    public float projectileArcHeight = 2f;
    [Tooltip("Dictates how long the projectile takes to reach the target (feedback)")]
    public float projectileDuration = 0.6f;
    public float spawnYOffset = 1f; 

    public override void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        // Calculates position on the grid
        Vector3Int attackerPos = activePlayerUnit.ownedTile.gridPosition;
        Vector3Int targetPos = targetTile.gridPosition;

        if (targetUnit != null && targetUnit.ownedTile != null)
        {
            targetPos = targetUnit.ownedTile.gridPosition;
        }

        // Calculate Distance (Chebyshev / Square style on the flat X/Z plane)
        int distanceX = Mathf.Abs(attackerPos.x - targetPos.x);
        int distanceZ = Mathf.Abs(attackerPos.z - targetPos.z);
        int flatDistance = Mathf.Max(distanceX, distanceZ);

        if (flatDistance < minAttackRange || flatDistance > maxAttackRange)
        {
            Debug.LogWarning($"Attack canceled. Target is at flat distance {flatDistance}, but range is {minAttackRange}-{maxAttackRange}.");
            return;
        }

        // activePlayerUnit.unitOpportunityPoints -= 1;

        // Damage/elevation difference pre-calculation
        int elevationDifference = attackerPos.y - targetPos.y;
        int flattenedDamage = 0;

        if (targetUnit != null && targetUnit.unitType != Unit.UnitType.Deity) // I should check this out later, this probably impedes attacking the Deity obelisk.
        {
            float baseDamageOutput = activePlayerUnit.unitAttackPower * activePlayerUnit.unitMeleeAttackBaseDamage;
            float distanceBonus = distanceDamageMultiplier * flatDistance;
            float elevationBonus = elevationDifference > 0 ? (heightDamageMultiplier * elevationDifference) : 0f;
            
            float finalDamage = baseDamageOutput * (1f + distanceBonus + elevationBonus);
            flattenedDamage = DamageCalculationUtility.FlattenDamage(finalDamage);
        }

        System.Action onHitCallback = () =>
        {
            if (targetTile.tileType == TileType.Obstacle)
            {
                Beacon beacon = targetTile.detectedUnit?.GetComponent<Beacon>();
                if (beacon != null) beacon.OnHitByUnit();
            }
            else if (targetUnit != null && targetUnit.unitType != Unit.UnitType.Deity)
            {
                string message = targetUnit.unitType == Unit.UnitType.DeityShard
                    ? $"{activePlayerUnit.unitTemplate.unitName} attacked Shard"
                    : $"{activePlayerUnit.unitTemplate.unitName} used Ranged Attack";
                BroadcastAttackNotification(message);
                targetUnit.TakeDamage(flattenedDamage);
                Debug.Log($"{activePlayerUnit.unitTemplate.unitName} fired a ranged attack! Final Damage: {flattenedDamage}");
            }
        };

       // Create the 3D projectile feedback
        // GameObject loadedPrefab = Resources.Load<GameObject>(projectileResourcePath);
        
        if (_projectilePrefab != null)
        {
            Vector3 startPos = activePlayerUnit.transform.position + new Vector3(0, spawnYOffset, 0);
            Vector3 endPos = (targetUnit != null ? targetUnit.transform.position : targetTile.transform.position) + new Vector3(0, spawnYOffset, 0);

            GameObject projectile = Instantiate(_projectilePrefab, startPos, Quaternion.identity);
            
            // We store the position to track the trajectory frame-by-frame
            Vector3 previousPos = projectile.transform.position;
            
            // DOJump creates an arc on the Y axis while moving toward the target
            projectile.transform.DOJump(endPos, projectileArcHeight, 1, projectileDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() => 
                {
                    // Calculate the actual direction of travel this frame
                    Vector3 currentPos = projectile.transform.position;
                    Vector3 moveDirection = currentPos - previousPos;
                    
                    // If we have moved, align the Z-axis (forward) with the trajectory
                    if (moveDirection != Vector3.zero)
                    {
                        projectile.transform.forward = moveDirection.normalized;
                    }
                    
                    // Update previous position for the next frame
                    previousPos = currentPos;
                })
                .OnComplete(() => 
                {
                    onHitCallback.Invoke();
                    Destroy(projectile);
                });
        }
        else
        {
            Debug.LogWarning($"[RangedBehavior] Could not find projectile prefab. Just attack.");
            onHitCallback.Invoke();
        }
    }

    public override int GetAttackRange()
    {
        return maxAttackRange;
    }
}