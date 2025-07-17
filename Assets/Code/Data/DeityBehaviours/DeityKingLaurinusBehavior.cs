using System.Linq;
using System;
using UnityEngine;
using static Deity;

[CreateAssetMenu(fileName = "KingLaurinusBehavior", menuName = "DeityBehavior/KingLaurinus")]
public class DeityKingLaurinusBehavior : DeityBehavior
{
    public delegate void UsedSpecialAttack(string moveName, string attackerName);
    public static event UsedSpecialAttack OnUsedSpecialAttack;

    public int attackExecutionThreshold = 2;
    private int lastAttackTurn = -1;

    public delegate void CheckPlayer();
    public static event CheckPlayer OnCheckPlayer;
    public override void ExecuteBehavior(Deity deity)
    {
        TileController[] gridTiles = ExtractRandomTiles();

        deity.deityCry.Play();
        foreach (var tile in gridTiles)
        {
            tile.currentTileCurseStatus = TileCurseStatus.cursed;
            Instantiate(Resources.Load("KingLaurinusOccupiedTileEffect"), tile.transform);
            BattleInterface.Instance.SetDeityNotification("King Laurinus's Curse spreads");
        }

        TurnController turnController = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>();

        if ((turnController.turnCounter - lastAttackTurn) >= attackExecutionThreshold)
        {
            lastAttackTurn = turnController.turnCounter;

            Debug.Log("Reached Attack Execution Turn. King Laurinus attacks the Player Units on the cursed tiles");
            deity.deityCry.Play();
            AttackPlayerUnits(deity);
            AttackEnemyUnits(deity);
            BattleInterface.Instance.SetSpellNameOnNotificationPanel("Cursed Garden", "King Laurinus");
            OnUsedSpecialAttack?.Invoke("Cursed Garden", "King Laurinus");
        }
    }


    // Extracts a number of random tiles. Laurinus will curse these random tiles.
    private TileController[] ExtractRandomTiles()
    {
        int randomCursedTileminRange = 20;
        int maxCursedTileRangeMaxRange = 30;

        int cursedTileNumber = UnityEngine.Random.Range(randomCursedTileminRange, maxCursedTileRangeMaxRange);

        // Ensure the GridManager instance and tiles array are properly initialized
        if (GridManager.Instance == null || GridManager.Instance.gridTileControllers == null)
        {
            throw new InvalidOperationException("GridManager instance or gridTileControllers array is not initialized.");
        }

        // Ensure there are at least 5 tiles to extract
        if (GridManager.Instance.gridTileControllers.Length < cursedTileNumber)
        {
            throw new InvalidOperationException("Not enough tiles to extract 5 random ones.");
        }

        // Use LINQ to randomly order the tiles and take the first 5
        return GridManager.Instance.gridTileControllers
            .OrderBy(t => Guid.NewGuid())
            .Take(cursedTileNumber)
            .ToArray();
    }
    private void AttackPlayerUnits(Deity deity)
    {
        float enmity = BattleManager.Instance.deity.enmity;
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
    private void AttackEnemyUnits(Deity deity)
    {
        float enmity = BattleManager.Instance.deity.enmity;
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