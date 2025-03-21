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
        if (turnController.turnCounter >= attackExecutionThreshold)
        {
            Debug.Log("Reached Attack Execution Turn. King Laurinus attacks the Player Units on the cursed tiles");

            deity.deityCry.Play();

            foreach (var playerUnit in GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield)
            {
                if (playerUnit.GetComponent<Unit>().ownedTile.currentTileCurseStatus == TileCurseStatus.cursed)
                {
                    playerUnit.GetComponent<Unit>().TakeDamage(deity.deitySpecialAttackPower);

                    playerUnit.GetComponent<Unit>().OnTakenDamage.Invoke(deity.deitySpecialAttackPower);

                    BattleInterface.Instance.SetSpellNameOnNotificationPanel("Cursed Garden", "King Laurinus");

                    Debug.Log("King Laurinus executes its Special Attack.");
                }
            }
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
}