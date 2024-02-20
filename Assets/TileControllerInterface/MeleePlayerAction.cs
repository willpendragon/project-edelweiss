using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;
using static TileController;
using UnityEngine.UI;


public class MeleePlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (selectedTile != null && selectionLimiter == 1)
        {
            currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();

            if (currentTarget.gameObject.tag == "Enemy")
            {
                selectedTile.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                //Switch Selected tile to WaitingForConfirmationStatus
                Debug.Log("Melee Select Logic");
                activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
                //Warning: magic number
                selectionLimiter--;
            }
        }
        else
        {
            Debug.Log("Can't Select Unit");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;
        savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
        savedSelectedTile = null;
    }
    public void Execute()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0)
        {
            DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();
            int attackPower = 2;
            int knockbackStrength = 2;
            //Warning: remove magic number later

            if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, savedSelectedTile))
            {
                attackPower = attackPower * 2;
                knockbackStrength = knockbackStrength * 2;
                Debug.Log("Applying distance and knockback multiplier");
            }
            ApplyKnockback(activePlayerUnit, currentTarget, knockbackStrength);
            currentTarget.HealthPoints -= attackPower;
            activePlayerUnit.unitOpportunityPoints--;
            Debug.Log("Melee Execution Logic");
        }
        else
        {
            Debug.Log("Not enough Opportunity Points on Active Player Unit");
        }

    }

    public void ApplyKnockback(Unit attacker, Unit defender, int knockbackStrength)
    {
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

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
        defender.ownedTile.detectedUnit = null;
        defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y);
        defender.GetComponent<Unit>().SetPosition(newGridPos.x, newGridPos.y);
    }
}
