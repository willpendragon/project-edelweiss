using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnController;
using static Unit;

public class UnitSetupController : MonoBehaviour
{
    private void Start()
    {
        SetUnitsInitialPositionOnGrid();
    }
    public void SetUnitsInitialPositionOnGrid()
    {
        foreach (var playerUnitGO in TurnController.Instance.playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();

            // Ensure that the starting coordinates are within the grid boundaries
            if (!IsWithinGridBounds(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate))
            {
                Debug.LogError($"Player Unit {playerUnit.name} has invalid starting coordinates: ({playerUnit.startingXCoordinate}, {playerUnit.startingYCoordinate})");
                continue; // Skip this unit to prevent errors
            }

            // Get the tile at the starting position
            TileController startingTile = GridManager.Instance.GetTileControllerInstance(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
            if (startingTile != null)
            {
                // Move the unit to its starting tile and update the tile's state
                playerUnit.SetPosition(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
                startingTile.currentSingleTileCondition = SingleTileCondition.occupied;
                startingTile.detectedUnit = playerUnit.gameObject;
                playerUnit.ownedTile = startingTile;

                Debug.Log($"Player Unit {playerUnit.name} placed at ({playerUnit.startingXCoordinate}, {playerUnit.startingYCoordinate})");
            }
            else
            {
                Debug.LogError($"Could not find a valid tile at ({playerUnit.startingXCoordinate}, {playerUnit.startingYCoordinate}) for {playerUnit.name}");
            }
        }

        foreach (var enemyUnitGO in TurnController.Instance.enemyUnitsOnBattlefield)
        {
            Unit enemyUnit = enemyUnitGO.GetComponent<Unit>();

            // Ensure that the starting coordinates are within the grid boundaries
            if (!IsWithinGridBounds(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate))
            {
                Debug.LogError($"Enemy Unit {enemyUnit.name} has invalid starting coordinates: ({enemyUnit.startingXCoordinate}, {enemyUnit.startingYCoordinate})");
                continue; // Skip this unit to prevent errors
            }

            // Get the tile at the starting position
            TileController startingTile = GridManager.Instance.GetTileControllerInstance(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);
            if (startingTile != null)
            {
                // Move the unit to its starting tile and update the tile's state
                enemyUnit.SetPosition(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);
                startingTile.currentSingleTileCondition = SingleTileCondition.occupied;
                startingTile.detectedUnit = enemyUnit.gameObject;
                enemyUnit.ownedTile = startingTile;

                Debug.Log($"Enemy Unit {enemyUnit.name} placed at ({enemyUnit.startingXCoordinate}, {enemyUnit.startingYCoordinate})");
            }
            else
            {
                Debug.LogError($"Could not find a valid tile at ({enemyUnit.startingXCoordinate}, {enemyUnit.startingYCoordinate}) for {enemyUnit.name}");
            }
        }

        RestorePlayerUnitsStatus();
    }
    private bool IsWithinGridBounds(int x, int y)
    {
        return x >= 0 && x < GridManager.Instance.gridHorizontalSize && y >= 0 && y < GridManager.Instance.gridVerticalSize;
    }

    public void RestorePlayerUnitsStatus()
    {
        foreach (var playerUnitGO in TurnController.Instance.playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();
            playerUnit.GetComponent<UnitSelectionController>().currentUnitSelectionStatus = UnitSelectionController.UnitSelectionStatus.unitDeselected;
            if (playerUnit.currentUnitLifeCondition == UnitLifeCondition.unitDead)
            {
                playerUnit.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            }

            // Removes all ailments from previous battles.
            playerUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.basic;
        }
    }

}
