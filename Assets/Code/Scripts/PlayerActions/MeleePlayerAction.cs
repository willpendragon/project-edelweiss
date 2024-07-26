using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TileController;
using UnityEngine.UI;
using UnityEditor.Experimental.GraphView;

public class MeleePlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    //public TileController savedPreviewDestinationTile;

    public delegate void UsedMeleeAction(string moveName, string attackerName);
    public static event UsedMeleeAction OnUsedMeleeAction;

    public void Select(TileController selectedTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (selectedTile != null && selectionLimiter == 1 && selectedTile.detectedUnit != null)
        {
            currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();

            if (currentTarget.gameObject.tag == "Enemy")
            {
                selectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.cyan;
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                savedSelectedTile = selectedTile;
                //Switch Selected tile to WaitingForConfirmationStatus
                Debug.Log("Melee Select Logic");

                activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
                //Warning: magic number
                selectionLimiter--;

                CheckKnockback(activePlayerUnit, selectedTile.detectedUnit.GetComponent<Unit>());
            }
        }
        else
        {
            // Play error SFX
            Debug.Log("Can't Select Unit");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;
        ResetTileColours();
    }

    public void CheckKnockback(Unit attacker, Unit defender)
    {
        int knockbackStrength = 2;

        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

        if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, savedSelectedTile))
        {
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
            float attackPower = activePlayerUnit.unitTemplate.meleeAttackPower;
            attackPower = attackPower * 2;
            knockbackStrength = knockbackStrength * 2;
            //Warning: remove magic numbers later

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

            // Apply the knockback strength within the limit of 1, 2, or 3 tiles
            knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

            // Calculate the new grid position with the possibly adjusted knockback strength
            Vector2Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

            // Clamp the new position to the grid bounds
            newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
            newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

            TileController destinationTile = GridManager.Instance.GetTileControllerInstance((int)newGridPos.x, (int)newGridPos.y);

            destinationTile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.magenta);
            Debug.Log("Enemy knocked back");

        }
    }

    public void Execute()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && currentTarget.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            float attackPower = activePlayerUnit.unitTemplate.meleeAttackPower;

            //Warning: remove magic number later
            int knockbackStrength = 2;

            DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

            if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, savedSelectedTile))
            {
                attackPower = attackPower * 2;
                knockbackStrength = knockbackStrength * 2;
                //Warning: remove magic numbers later
                ApplyKnockback(activePlayerUnit, currentTarget, knockbackStrength);
            }
            else
            {
                currentTarget.TakeDamage(activePlayerUnit.unitAttackPower * activePlayerUnit.unitMeleeAttackBaseDamage);
            }
            activePlayerUnit.unitOpportunityPoints--;

            UpdateActivePlayerUnitProfile(activePlayerUnit);

            activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, currentTarget);
            OnUsedMeleeAction("Melee Attack", activePlayerUnit.unitTemplate.unitName);

            Debug.Log("Melee Execution Logic");
        }
        else
        {
            Debug.Log("Not enough Opportunity Points on Active Player Unit or Unit has already died");
        }
    }

    public void ApplyKnockback(Unit attacker, Unit defender, int knockbackStrength)
    {
        currentTarget.TakeDamage(attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage);

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
            currentTarget.TakeDamage(attacker.unitTemplate.meleeAttackPower);
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
        if (defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y);

            TileController destinationTile = GridManager.Instance.GetTileControllerInstance((int)newGridPos.x, (int)newGridPos.y);

            destinationTile.detectedUnit = defender.gameObject;
            defender.ownedTile = destinationTile;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;
            Debug.Log("Enemy knocked back");
            destinationTile.tileShaderController.ResetTileFadeHeightAnimation(destinationTile);
        }
        else
        {
            Debug.Log("Can't knockback Enemy Unit");
        }
        ResetTileColours();

    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }

    public void ResetTileColours()
    {
        if (savedSelectedTile != null)
        {
            savedSelectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            savedSelectedTile = null;
            Debug.Log("Deselecting Currently Selected Tile");
        }
    }
}
