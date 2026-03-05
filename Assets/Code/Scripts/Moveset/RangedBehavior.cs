using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Behavior", menuName = "Moveset/RangedBehavior")]

public class RangedBehavior : PhysicalAttackBehavior
{
    public override void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        Debug.Log("Using Ranged Attack");
    }
}

