using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSetupController : MonoBehaviour
{
    [SerializeField] FaithController faithController;
    private void Start()
    {
        SetUnitsInitialPositionOnGrid();
    }
    public void SetUnitsInitialPositionOnGrid()
    {
        foreach (var playerUnitGO in TurnController.Instance.playerUnitsOnBattlefield)
        {
            Unit playerUnit = playerUnitGO.GetComponent<Unit>();

            // INSTEAD OF MATH BOUNDS -> Check if the actual tile exists!
            TileController startingTile = GridManager.Instance.GetTileControllerInstance(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);

            if (startingTile != null)
            {
                // Move the unit to its starting tile and update the tile's state.
                playerUnit.SetPosition(playerUnit.startingXCoordinate, playerUnit.startingYCoordinate);
                startingTile.currentSingleTileCondition = SingleTileCondition.occupied;
                startingTile.detectedUnit = playerUnit.gameObject;
                playerUnit.ownedTile = startingTile;

                Debug.Log($"Player Unit {playerUnit.name} placed at ({playerUnit.startingXCoordinate}, {playerUnit.startingYCoordinate})");
            }
            else
            {
                Debug.LogWarning($"UnitSetupController: Could not find a floor tile at ({playerUnit.startingXCoordinate}, {playerUnit.startingYCoordinate}) for {playerUnit.name}! Did you paint them in mid-air?");
            }
        }

        foreach (var enemyUnitGO in TurnController.Instance.enemyUnitsOnBattlefield)
        {
            Unit enemyUnit = enemyUnitGO.GetComponent<Unit>();

            // INSTEAD OF MATH BOUNDS -> Check if the actual tile exists!
            TileController startingTile = GridManager.Instance.GetTileControllerInstance(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);

            if (startingTile != null && enemyUnit.unitType != Unit.UnitType.Deity)
            {
                // Move the unit to its starting tile and update the tile's state.
                enemyUnit.SetPosition(enemyUnit.startingXCoordinate, enemyUnit.startingYCoordinate);
                startingTile.currentSingleTileCondition = SingleTileCondition.occupied;
                startingTile.detectedUnit = enemyUnit.gameObject;
                enemyUnit.ownedTile = startingTile;

                Debug.Log($"Enemy Unit {enemyUnit.name} placed at ({enemyUnit.startingXCoordinate}, {enemyUnit.startingYCoordinate})");
            }
            else
            {
                Debug.LogWarning($"UnitSetupController: Could not find a floor tile at ({enemyUnit.startingXCoordinate}, {enemyUnit.startingYCoordinate}) for {enemyUnit.name}! Did you paint them in mid-air?");
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

            // Removes all ailments from previous battles
            playerUnit.GetComponentInChildren<UnitStatusController>().unitCurrentStatus = UnitStatus.basic;

            // NEW: Clear any lingering stun icons from previous battles
            BattleFeedbackController battleFeedbackController = playerUnitGO.GetComponent<BattleFeedbackController>();
            if (battleFeedbackController != null && battleFeedbackController.stunIcon != null)
            {
                Destroy(battleFeedbackController.stunIcon);
                battleFeedbackController.stunIcon = null;
            }

            if (playerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            {
                playerUnitGO.GetComponent<Unit>().characterAnimator.SetTrigger("Die");
            }

            if (faithController != null)
            {
                faithController.CheckFaithPoints();
            }
        }

        // NEW: Clear the statusIcons list from previous battles
        if (GridManager.Instance != null)
        {
            GridManager.Instance.statusIcons.Clear();
        }
    }
}
