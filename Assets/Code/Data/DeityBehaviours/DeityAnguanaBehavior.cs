using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "AnguanaBehavior", menuName = "DeityBehavior/Anguana")]
public class DeityAnguanaBehavior : DeityBehavior
{
    public float vfxDurationDelay = 1f;
    private string deityName = "Anguana";
    public string attackName;

    private System.Random localRandom;

    public override void ExecuteBehavior(Deity deity)
    {
        if (deity.currentDeityStatus == Deity.DeityStatus.Summoned)
            return;

        BattleManager battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();

        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            if (deity.PerformDeityEnmityCheck())
            {
                DOVirtual.DelayedCall(1.5f, () => Attack(deity));
            }
            else
            {
                DOVirtual.DelayedCall(1f, () => BattleInterface.Instance.SetDeityNotification($"Deity {deityName} placidly looks around"));
            }
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            Attack(deity);
            MoveDeityToRandomTile(deity);
            GameObject deitySpawnerGameObject = GameObject.FindGameObjectWithTag("DeitySpawner");
            DeitySpawner deitySpawner = deitySpawnerGameObject.GetComponent<DeitySpawner>();
            deitySpawner.MoveObeliskOnGridMap();
        }
    }

    public void Attack(Deity deity)
    {
        BattleInterface.Instance.SetDeityNotification($"Deity {deityName} used {attackName}");
        deity.deityCry.Play();

        GameObject[] playerUnitsOnBattlefield = GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield;

        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            GameObject newDeityAttackVFX = Instantiate(deity.deityAttackVFX, playerUnit.GetComponent<Unit>().ownedTile.transform.position, Quaternion.identity);
            Vector3 attackVFXOffset = new Vector3(0, 1, 0);
            newDeityAttackVFX.transform.localPosition += attackVFXOffset;
            Destroy(newDeityAttackVFX, vfxDurationDelay);
            playerUnit.GetComponent<Unit>().TakeDamage(deity.deitySpecialAttackPower);
        }

        deity.enmity = 0;
    }

    private void MoveDeityToRandomTile(Deity deity)
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
                           tile.detectedUnit == null)
            .ToList();

        if (validTiles.Count == 0)
        {
            Debug.Log("Anguana couldn't find any valid tile to move.");
            return;
        }

        int randomIndex = localRandom.Next(validTiles.Count);
        TileController randomTile = validTiles[randomIndex];

        MoveDeityToTile(deity, randomTile);

        Debug.Log($"Anguana moved to: ({randomTile.tileXCoordinate}, {randomTile.tileYCoordinate})");
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

        deity.transform.position = GridManager.Instance.GetWorldPositionFromGridCoordinates(
            destinationTile.tileXCoordinate, destinationTile.tileYCoordinate);
        deity.transform.position += new Vector3(0, 0.5f, 0); // Optional height offset
        deity.gameObject.GetComponent<Unit>().currentXCoordinate = destinationTile.tileXCoordinate;
        deity.gameObject.GetComponent<Unit>().currentYCoordinate = destinationTile.tileYCoordinate;
    }
}
