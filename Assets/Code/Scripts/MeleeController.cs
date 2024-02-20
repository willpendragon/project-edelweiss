using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;

public class MeleeController : MonoBehaviour
{
    /*
    public delegate void MeleeAttack();
    public static event MeleeAttack OnMeleeAttack;

    public Unit currentMeleeTargetedUnit;
    //Magic number, fix later 12022024
    public float attackPower = 2;
    public DistanceController distanceController;

    public GameObject targetUnit;

    public void OnEnable()
    {
        GridTargetingController.OnMeleeTargetedUnit += SetTargetedUnit;
        TileController.OnTileConfirmedMeleeMode += ExecuteMeleeAttack;
    }
    public void OnDisable()
    {
        GridTargetingController.OnMeleeTargetedUnit -= SetTargetedUnit;
        TileController.OnTileConfirmedMeleeMode -= ExecuteMeleeAttack;
    }

    public void StartMeleeAttack()
    {
        OnMeleeAttack();
    }

    public void SetTargetedUnit(Unit targetedUnit)
    {
        //Sets the currently Targeted Enemy.
        currentMeleeTargetedUnit = targetedUnit;
    }

    public void ExecuteMeleeAttack()
    {
        if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, currentMeleeTargetedUnit.ownedTile))
        {
            attackPower = attackPower * 2;
            //Warning: Magic Number
            Debug.Log("Applying distance multiplier");
        }
        currentMeleeTargetedUnit.TakeDamage((int)attackPower);
        ApplyKnockback(GameObject.FindGameObjectWithTag("ActivePlayerUnit"), currentMeleeTargetedUnit.gameObject, 2);
        Debug.Log("Attacking Unit using Melee Move");

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
    */
}