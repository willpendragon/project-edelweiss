using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Possible prioritization.
public enum TargetTileType
{
    Ice,
    Fire,
    Lighting
}

[CreateAssetMenu(fileName = "NewEnemyAITiLEPriority", menuName = "EnemyBehavior/AITilePriority")]
public class EnemyAITilePriority : ScriptableObject
{
    [Header("Enemy AI Settings")]
    [Tooltip("Dictates which tile the Enemy prioritizes as target.")]
    public TargetTileType targetTypePriority;

    public TileController SelectTargetTile(Unit attacker)
    {
        TileController[] tiles = GridManager.Instance.gridTileControllers;

        // Select only free titles
        IEnumerable<TileController> validTiles = tiles
            .Select(go => go.GetComponent<TileController>())
            .Where(tile => tile != null && tile.currentSingleTileCondition == SingleTileCondition.free);

        if (!validTiles.Any()) return null;

        IEnumerable<TileController> elementalTiles = null;

        switch (targetTypePriority)
        {
            case TargetTileType.Ice:
                elementalTiles = validTiles.Where(t => t.tileElement == TileElement.Ice);
                break;
            case TargetTileType.Lighting:
                elementalTiles = validTiles.Where(t => t.tileElement == TileElement.Lighting);
                break;
            case TargetTileType.Fire:
                elementalTiles = validTiles.Where(t => t.tileElement == TileElement.Fire);
                break;
        }

        if (elementalTiles == null || !elementalTiles.Any())
            return null;

        return elementalTiles
            .OrderBy(tile => Vector3.Distance(attacker.transform.position, tile.transform.position))
            .FirstOrDefault();
    }
}