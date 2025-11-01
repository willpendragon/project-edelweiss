using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using static DeityKingLaurinusBehavior;

[CreateAssetMenu(fileName = "KingLaurinusBehavior", menuName = "DeityBehavior/KingLaurinus")]
public class DeityKingLaurinusBehavior : DeityBehavior
{
    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;

    public delegate void UsedCursedGarden(Deity deity);
    public static event UsedCursedGarden OnUsedCursedGarden;

    private string deityName = "King Laurinus";

    public override void ExecuteBehavior(Deity deity)
    {
        deity.deityCry.Play();
        DOVirtual.DelayedCall(1.5f, () => SpreadCurse());
        DOVirtual.DelayedCall(3f, () => Attack(deity));

        Debug.Assert(deity == BattleManager.Instance.deity, "Mismatch: Passed-in deity is not the one in BattleManager");
    }

    private void Attack(Deity deity)
    {
        float enmity = BattleManager.Instance.deity.enmity;
        if (enmity >= BattleManager.Instance.deity._maxEnmity)

        {
            AttackPlayerUnits(deity, enmity);
            AttackEnemyUnits(deity, enmity);
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName} activated Cursed Garden");
            // Reset Enmity
            BattleManager.Instance.deity.enmity = 0;
            BattleManager.Instance.deity.UpdateDeityEnmitySlider();
            // Show Cursed Garden Callout
            OnUsedCursedGarden(deity);
        }
    }

    private void SpreadCurse()
    {
        var allTiles = GridManager.Instance.gridTileControllers;

        // Check if all tiles are already cursed
        bool allCursed = allTiles.All(t => t.currentTileCurseStatus == TileCurseStatus.cursed);

        if (allCursed)
        {
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName}'s Curse is complete");
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
            }
        }
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
}