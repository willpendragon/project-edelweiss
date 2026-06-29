using UnityEngine;

public abstract class EnemyBehavior : ScriptableObject
{
    public enum DefenseRequirement
    {
        Parryable, // Player can use parry.
        Unblockable // Can't be blocked.
    }

    public abstract void ExecuteBehavior(EnemyAgent enemyAgent);

    /// <summary>
    /// Centralized base logic allowing an Enemy to determine if a destination path tile 
    /// is legal to end their movement on.
    /// </summary>
    protected bool IsTileValidDestination(TileController tile)
    {
        if (tile == null) return false;

        // An Enemy cannot stop if the tile is technically occupied, there's another Unit inside it, 
        // or a Field Prize currently holds the spot.
        return tile.currentSingleTileCondition == SingleTileCondition.free
               && tile.detectedUnit == null
               && tile.tileCurrentFieldPrize == null;
    }
}