using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KingLaurinusBehavior", menuName = "DeityBehavior/KingLaurinus")]
public class DeityKingLaurinusBehavior : DeityBehavior
{
    public delegate void UsedCursedGarden(Unit deityUnit);
    public static event UsedCursedGarden OnUsedCursedGarden;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    private string deityName = "King Laurinus";

    private System.Random localRandom;

    public override void ExecuteBehavior(Deity deity)
    {
        //deity.deityCry.Play();
        //DOVirtual.DelayedCall(1.5f, () => SpreadCurse());
        //DOVirtual.DelayedCall(3f, () => AttemptAttack(deity));
        if (deity.currentDeityStatus == Deity.DeityStatus.Summoned)
            return;

        BattleManager battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();

        if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.RegularBattle)
        {
            DOVirtual.DelayedCall(1.5f, () => SpreadCurse());
            DOVirtual.DelayedCall(3f, () => AttemptAttack(deity));
        }
        else if (BattleTypeController.Instance.currentBattleType == BattleTypeController.BattleType.BattleWithDeity)
        {
            DOVirtual.DelayedCall(1.5f, () => SpreadCurse());
            DOVirtual.DelayedCall(3f, () => AttemptAttack(deity));
            DOVirtual.DelayedCall(3.5f, () => MoveObelisk(deity));
        }
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {
        throw new NotImplementedException();
    }

    private void AttemptAttack(Deity deity)
    {
        float enmity = BattleManager.Instance.enemyTurnManager.deity.GetComponent<Deity>().enmity;

        if (deity.PerformDeityEnmityCheck())
        {
            AttackPlayerUnits(deity, enmity);
            AttackEnemyUnits(deity, enmity);
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName} used Cursed Garden");
            // Reset Laurinus' enmity
            ResetDeityEnmityWrapper();
        }
        else
        {
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName} smells the roses...");
            Debug.Log($"{enmity} - {BattleManager.Instance.deity._maxEnmity}");
        }
    }

    private void ResetDeityEnmityWrapper()
    {
        var deityReference = BattleManager.Instance.enemyTurnManager.deity.GetComponent<Deity>();
        deityReference.ResetDeityEnmity();
    }

    private void SpreadCurse()
    {
        var allTiles = GridManager.Instance.gridTileControllers;

        // Check if all tiles are already cursed
        bool allCursed = allTiles.All(t => t.currentTileCurseStatus == TileCurseStatus.cursed);

        if (allCursed)
        {
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName}'s cursed the entire field.");
            return;
        }

        TileController[] gridTiles = ExtractRandomTiles();
        foreach (var tile in gridTiles)
        {
            tile.currentTileCurseStatus = TileCurseStatus.cursed;
            Instantiate(Resources.Load("KingLaurinusOccupiedTileEffect"), tile.transform);
        }
        BattleInterface.Instance.SetDeityNotification($"Deity {deityName}'s Curse spreads");
    }

    // Extracts a number of random tiles. Laurinus will curse these random tiles.
    private TileController[] ExtractRandomTiles()
    {
        int randomCursedTileMinRange = 20;
        int maxCursedTileRangeMaxRange = 30;

        int cursedTileNumber = UnityEngine.Random.Range(randomCursedTileMinRange, maxCursedTileRangeMaxRange);

        if (GridManager.Instance == null || GridManager.Instance.gridTileControllers == null)
        {
            throw new InvalidOperationException("GridManager instance or gridTileControllers array is not initialized.");
        }

        // Filter only non-cursed tiles
        var nonCursedTiles = GridManager.Instance.gridTileControllers
            .Where(t => t.currentTileCurseStatus != TileCurseStatus.cursed)
            .ToList();

        if (nonCursedTiles.Count < cursedTileNumber)
        {
            cursedTileNumber = nonCursedTiles.Count; // Adjust to max available
        }

        return nonCursedTiles
            .OrderBy(t => Guid.NewGuid()) // Randomize
            .Take(cursedTileNumber)
            .ToArray();
    }

    private void AttackPlayerUnits(Deity deity, float enmity)
    {
        float scaledDamage = deity.deitySpecialAttackPower + (enmity * 0.5f);

        GameObject[] playerUnits = GameObject.FindGameObjectWithTag("PlayerPartyController")
            .GetComponent<PlayerPartyController>()
            .playerUnitsOnBattlefield;

        float totalDamage = 0f;

        foreach (var playerUnit in playerUnits)
        {
            if (playerUnit == null) continue;

            Unit unit = playerUnit.GetComponent<Unit>();
            if (unit == null || unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead) continue;

            TileController tile = unit.ownedTile;
            if (tile == null) continue;

            if (tile.currentTileCurseStatus == TileCurseStatus.cursed)
            {
                unit.TakeDamage(scaledDamage);
                unit.OnTakenDamage.Invoke(scaledDamage);
                OnCheckPlayer?.Invoke();
                totalDamage += scaledDamage;
            }
        }

        DisplayAttackCallout(deity);

        //// Display Laurinus' Cursed Garden Callout only if at least one Player Unit has been hit.
        //if (totalDamage > 0)
        //{
        //}
    }

    private void DisplayAttackCallout(Deity deity)
    {
        Unit deityUnit = deity.GetComponent<Unit>();
        OnUsedCursedGarden(deityUnit);
    }

    private void AttackEnemyUnits(Deity deity, float enmity)
    {
        float scaledDamage = deity.deitySpecialAttackPower + (enmity * 0.5f);

        GameObject[] enemyUnits = BattleManager.Instance.enemiesOnBattlefield;

        foreach (var enemyUnit in enemyUnits)
        {
            if (enemyUnit == null) continue;

            Unit unit = enemyUnit.GetComponent<Unit>();
            if (unit == null || unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead) continue;

            TileController tile = unit.ownedTile;
            if (tile == null) continue;

            if (tile.currentTileCurseStatus == TileCurseStatus.cursed)
            {
                unit.TakeDamage(scaledDamage);
                unit.OnTakenDamage.Invoke(scaledDamage);
            }
        }
        Debug.Log("King Laurinus used its special attack on Enemies");
    }

    private void MoveObelisk(Deity deity)
    {
        // Logic.
        MoveDeityToRandomTile(deity);
        GameObject deitySpawnerGameObject = GameObject.FindGameObjectWithTag("DeitySpawner");
        // Only the Obelisk conduit moves on the Battlefield.
        DeitySpawner deitySpawner = deitySpawnerGameObject.GetComponent<DeitySpawner>();
        // Physically move the Obelisk.        
        deitySpawner.MoveObeliskOnGridMap();
        DOVirtual.DelayedCall(1f, () => BattleInterface.Instance.SetDeityNotification($"Deity {deityName} moved its Altar."));
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