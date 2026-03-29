using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Behavior", menuName = "Moveset/RangedBehavior")]
public class RangedBehavior : PhysicalAttackBehavior
{
    [Header("Ranged Specifics")]
    public int maxAttackRange = 10; // Set to 10 as requested!
    public int minAttackRange = 1;  // Set to 2 if you want "dead zones" right next to the archer

    public override void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        // 1. Get Positions
        Vector2Int attackerPos = activePlayerUnit.GetGridPosition();

        // If we have a unit, get its position. If we are hitting an empty tile/obstacle, get the tile's position.
        Vector2Int targetPos = targetUnit != null ? targetUnit.GetGridPosition() : GetTilePosition(targetTile);

        // 2. Calculate Distance (Chebyshev / Square style for easy diagonal targeting)
        int distanceX = Mathf.Abs(attackerPos.x - targetPos.x);
        int distanceY = Mathf.Abs(attackerPos.y - targetPos.y);
        int distance = Mathf.Max(distanceX, distanceY);

        /* // NOTE: If you decide you want the classic Final Fantasy Tactics "Diamond" shape range instead, 
        // comment out the line above and uncomment the line below:
        // int distance = distanceX + distanceY; 
        */

        // 3. Validate Range
        if (distance < minAttackRange || distance > maxAttackRange)
        {
            Debug.LogWarning($"Attack canceled! Target is at distance {distance}, but range is {minAttackRange}-{maxAttackRange}.");
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

            // Trigger Ranged Animation here when you have it set up in your BattleFeedbackController
            // activePlayerUnit.GetComponent<BattleFeedbackController>().PlayRangedAttackAnimation(activePlayerUnit, targetUnit);

            return;
        }

        // 5. Handle Unit Attacks (Direct Damage, NO Knockback)
        if (targetUnit != null && targetUnit.unitType != Unit.UnitType.Deity)
        {
            // Subtract Opportunity Points
            activePlayerUnit.unitOpportunityPoints -= 1;

            // Apply Damage directly. We pass 'false' because knockback modifiers don't apply here.
            HitTarget(activePlayerUnit, targetUnit, false);

            Debug.Log($"{activePlayerUnit.unitTemplate.unitName} fired a ranged attack at {targetUnit.unitTemplate.unitName} from {distance} tiles away!");
        }
    }

    // Helper method to extract grid coordinates from a TileController
    private Vector2Int GetTilePosition(TileController tile)
    {
        // Assuming your TileController has an X and Y coordinate. 
        // You may need to tweak this depending on how your specific TileController stores its grid position!
        // Example: return new Vector2Int(tile.gridX, tile.gridY);

        if (tile != null)
        {
            // Fallback: If your tile doesn't store gridX/gridY but uses world position, 
            // you might need to convert transform.position to grid coordinates here.
            Debug.LogWarning("Make sure GetTilePosition is correctly pulling your tile's grid coordinates!");
        }

        return Vector2Int.zero;
    }

    public override int GetAttackRange()
    {
        return maxAttackRange;
    }
}