using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimildeBehavior", menuName = "DeityBehavior/Similde")]
public class DeitySimildeBehavior : DeityBehavior
{
    private string deityName = "Similde";

    public override void ExecuteBehavior(Deity deity)
    {
        Debug.Log("Deity is acting");
        // Scan the Grid
        var tileList = GridManager.Instance.gridTileControllers;
        // Roll two random numbers
        int randomTileIndex = Random.Range(1, tileList.Length);
        TileController randomTile = tileList[randomTileIndex];
        List<TileController> randomTiles = GridManager.Instance.gridMovementController.GetNeighbours(randomTile);
        List<TileController> finalList = new List<TileController>(randomTiles);
        finalList.Add(randomTile);

        foreach (var tile in finalList)
        {
            Instantiate(Resources.Load("SimildePossessedTile"), tile.transform);
            Debug.Log($"Randomly picked {tile.gameObject.name} at {tile.tileXCoordinate}, {tile.tileYCoordinate}");
            tile.tileElement = TileElement.Ice;
            BattleInterface.Instance.SetDeityNotification($"Deity {deityName} enchanted {finalList.Count} tiles");
        }

        // Search the neighbours for those tiles
        // Act on these tiles
    }

    public override void ExecuteBuffBehaviour(Deity deity, Unit unit)
    {

    }
}
