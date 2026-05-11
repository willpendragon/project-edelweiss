using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using ProjectEdelweiss.Utils;

public abstract class DeityBehavior : ScriptableObject
{
    private System.Random localRandom;
    public abstract void ExecuteBehavior(Deity deity);

    public abstract void ExecuteBuffBehaviour(Deity deity, Unit unit);

    public void MoveObelisk(Deity deity)
    {
        // Logic.
        MoveDeityToRandomTile(deity);
        GameObject deitySpawnerGameObject = GameObject.FindGameObjectWithTag(GameTags.DEITY_SPAWNER);
        // Only the Obelisk conduit moves on the Battlefield.
        DeitySpawner deitySpawner = deitySpawnerGameObject.GetComponent<DeitySpawner>();
        // Physically move the Obelisk.        
        deitySpawner.MoveObeliskOnGridMap();
        DOVirtual.DelayedCall(1f,
            () => BattleInterface.Instance.SetDeityNotification($"Deity {deity.gameObject.name} moved its Altar."));
    }

    public void MoveDeityToRandomTile(Deity deity)
    {
        if (localRandom == null)
        {
            localRandom = new System.Random(); // No seed to guarantee fresh randomness at each run.
        }

        List<Vector2Int> tileCoordinates = GridManager.Instance.GetExistingTileCoordinates();

        // Filter out occupied tiles
        List<TileController> validTiles = tileCoordinates
            .Select(coord => GridManager.Instance.GetTileControllerInstance(coord.x, coord.y))
            .Where(tile => tile != null &&
                           tile.currentSingleTileCondition == SingleTileCondition.free &&
                           tile.detectedUnit == null &&
                           tile.gameObject.CompareTag(GameTags.TILE))
            .ToList();

        if (validTiles.Count == 0)
        {
            Debug.Log("Anguana couldn't find any valid tile to move.");
            return;
        }

        int randomIndex = localRandom.Next(validTiles.Count);
        TileController randomTile = validTiles[randomIndex];

        MoveDeityToTile(deity, randomTile);

        Debug.Log($"Laurinus moved to: ({randomTile.tileXCoordinate}, {randomTile.tileYCoordinate})");
    }

    private void MoveDeityToTile(Deity deity, TileController destinationTile)
    {
        TileController startTile = deity.gameObject.GetComponent<Unit>().ownedTile;

        if (startTile != null)
        {
            startTile.detectedUnit = null;
            startTile.currentSingleTileCondition = SingleTileCondition.free;
        }

        deity.gameObject.GetComponent<Unit>().ownedTile = destinationTile;
        destinationTile.detectedUnit = deity.gameObject;
        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

        deity.gameObject.GetComponent<Unit>().currentXCoordinate = destinationTile.tileXCoordinate;
        deity.gameObject.GetComponent<Unit>().currentYCoordinate = destinationTile.tileYCoordinate;
    }
}