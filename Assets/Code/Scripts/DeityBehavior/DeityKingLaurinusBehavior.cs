using System.Linq;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "KingLaurinusBehavior", menuName = "DeityBehavior/KingLaurinus")]
public class DeityKingLaurinusBehavior : DeityBehavior
{
    private float deityAttackPower = 100;
    private int attackExecutionThreshold = 2;
    public override void ExecuteBehavior(Deity deity)
    {
        TileController[] gridTiles = ExtractRandomTiles();
        foreach (var tile in gridTiles)
        {
            //tile.gameObject.GetComponentInChildren<SpriteRenderer>().material.color = Color.gray;
            tile.currentTileCurseStatus = TileCurseStatus.cursed;
            Instantiate(Resources.Load("KingLaurinusOccupiedTileEffect"), tile.transform);

        }
        if (GameObject.FindGameObjectWithTag("BattleManager").GetComponent<TurnController>().turnCounter >= attackExecutionThreshold)
        {
            Debug.Log("Reached Turn 3. King Laurinus attacks the Player Units on the cursed tiles");
            foreach (var playerUnit in GameObject.FindGameObjectWithTag("PlayerPartyController").GetComponent<PlayerPartyController>().playerUnitsOnBattlefield)
            {
                if (playerUnit.GetComponent<Unit>().ownedTile.currentTileCurseStatus == TileCurseStatus.cursed)
                {
                    playerUnit.GetComponent<Unit>().HealthPoints -= deityAttackPower;
                    //Hard coded damage. Remember to create it as a variable that I can change on the King Laurinus Behaviour Scriptable Object.
                    Debug.Log("King Laurinus provokes massive Damage on the Unit");
                }
            }
        }
        Debug.Log("King Laurinus Executes Cursed Garden attack");
        // King Laurinus' specific behavior implementation goes here.
        // For example, checking the enmity meter and unleashing an attack.
    }

    void Start()
    {
        try
        {
            TileController[] randomTiles = ExtractRandomTiles();
            // Use the randomTiles array as needed, for example, to display them or trigger specific logic
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
        }
    }

    private TileController[] ExtractRandomTiles()
    {
        int cursedTileNumber = UnityEngine.Random.Range(20, 30);
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