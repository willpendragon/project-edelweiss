using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test_Knockback : MonoBehaviour
{
    public GameObject attacker;
    public GameObject defender;

    public void OnButtonClick()
    {
        ApplyKnockback(attacker, defender, 1);
    }
    public void ApplyKnockback(GameObject attacker, GameObject defender, int knockbackStrength)
    {
        Vector2Int attackerPos = attacker.GetComponent<Unit>().GetGridPosition();
        Vector2Int defenderPos = defender.GetComponent<Unit>().GetGridPosition();

        // Calculate the difference in positions
        int deltaX = attackerPos.x - defenderPos.x;
        int deltaY = attackerPos.y - defenderPos.y;

        // Determine the direction of the knockback
        Vector2Int knockbackDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
        {
            knockbackDirection.x = -(int)Mathf.Sign(deltaX);
        }
        else
        {
            knockbackDirection.y = -(int)Mathf.Sign(deltaY);
        }

        // Check the immediate next tile in the knockback direction
        Vector2Int immediateNextTile = defenderPos + knockbackDirection;

        // If the immediate next tile is occupied, do not apply knockback
        TileController immediateTileController = GridManager.Instance.GetTileControllerInstance(immediateNextTile.x, immediateNextTile.y);
        if (immediateTileController == null || immediateTileController.currentSingleTileCondition == SingleTileCondition.occupied)
        {
            Debug.Log("Immediate tile in knockback path is occupied. Knockback canceled.");
            return;
        }

        // Apply the knockback strength within the limit of 1, 2, or 3 tiles
        knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

        // Calculate the new grid position with the possibly adjusted knockback strength
        Vector2Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

        // Clamp the new position to the grid bounds
        newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
        newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

        // Move the defender to the new grid position
        defender.GetComponent<Unit>().SetPosition(newGridPos.x, newGridPos.y);
    }
}
