using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TileController;
using UnityEngine.UI;

public class MeleePlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;

    public Vector2Int knockbackDirection; // Stores knockback direction
    public int knockbackStrength = 2; // Stores knockback strength

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
                selectionLimiter--;

                // Check if hookshot is equipped and handle selection logic
                if (activePlayerUnit.hasHookshot)
                {
                    Debug.Log("Hookshot selected, waiting for confirmation...");
                }
                else
                {
                    // For normal melee attack, check knockback after selection
                    CheckKnockback(activePlayerUnit, selectedTile.detectedUnit.GetComponent<Unit>());
                }
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
        ResetTileColours();
    }

    // Method for knockback logic (used for normal melee attacks)
    public void CheckKnockback(Unit attacker, Unit defender)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

        if (distanceController.CheckDistance(attacker.ownedTile, savedSelectedTile))
        {
            Vector2Int attackerPos = attacker.GetGridPosition();
            Vector2Int defenderPos = defender.GetGridPosition();

            // Calculate the difference in positions
            int deltaX = attackerPos.x - defenderPos.x;
            int deltaY = attackerPos.y - defenderPos.y;

            // Determine the direction of the knockback
            knockbackDirection = Vector2Int.zero;
            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                knockbackDirection.x = -(int)Mathf.Sign(deltaX);
            }
            else
            {
                knockbackDirection.y = -(int)Mathf.Sign(deltaY);
            }

            // Clamp knockback strength between 1 and 3 tiles
            knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

            // Calculate preview knockback position
            Vector2Int previewGridPos = defenderPos + (knockbackDirection * knockbackStrength);

            // Clamp the preview position to the grid bounds
            previewGridPos.x = Mathf.Clamp(previewGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
            previewGridPos.y = Mathf.Clamp(previewGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

            TileController previewTile = GridManager.Instance.GetTileControllerInstance(previewGridPos.x, previewGridPos.y);

            if (previewTile != null && previewTile.currentSingleTileCondition != SingleTileCondition.occupied)
            {
                // Show the knockback preview (e.g., change tile color, animation)
                previewTile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.magenta);
            }
            else
            {
                Debug.Log("Can't preview knockback, invalid destination.");
            }
        }
    }

    // Hookshot attack logic (used when hookshot is equipped)
    public void ExecuteHookshot(Unit attacker, Unit defender)
    {
        int hookshotRange = 3; // Maximum range of the hookshot

        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        // Calculate the Manhattan distance (abs(dx) + abs(dy))
        int distance = Mathf.Abs(defenderPos.x - attackerPos.x) + Mathf.Abs(defenderPos.y - attackerPos.y);

        // Check if the distance is within the hookshot range
        if (distance > hookshotRange)
        {
            Debug.Log("Enemy is out of hookshot range.");
            return;
        }

        // Calculate the difference in positions
        int deltaX = defenderPos.x - attackerPos.x;
        int deltaY = defenderPos.y - attackerPos.y;

        // Determine the direction of the pull (opposite of knockback logic)
        Vector2Int pullDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
        {
            pullDirection.x = (int)Mathf.Sign(deltaX); // Pull in X-axis
        }
        else
        {
            pullDirection.y = (int)Mathf.Sign(deltaY); // Pull in Y-axis
        }

        // Calculate the target tile directly in front of the player based on direction
        Vector2Int newGridPos = attackerPos + pullDirection;

        // Clamp the position to the grid bounds
        newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
        newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

        TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);

        // Ensure the destination tile is not occupied
        if (destinationTile != null && destinationTile.currentSingleTileCondition != SingleTileCondition.occupied)
        {
            // Move the defender to the new tile in front of the player
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;

            defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y, true);

            destinationTile.detectedUnit = defender.gameObject;
            defender.ownedTile = destinationTile;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;

            Debug.Log("Enemy pulled in with hookshot");

            // Optional: Apply visual or sound effects to show the hookshot pull
            destinationTile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.cyan);

            // Trigger a feedback event for the hookshot usage
            OnUsedMeleeAction?.Invoke("Hookshot", attacker.unitTemplate.unitName);
        }
        else
        {
            Debug.Log("No valid position for hookshot pull.");
        }
    }

    public void Execute()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitOpportunityPoints > 0 && currentTarget.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            // Check if the player has a hookshot equipped
            if (activePlayerUnit.hasHookshot)
            {
                // Execute the hookshot attack instead of the melee attack
                ExecuteHookshot(activePlayerUnit, currentTarget);
            }
            else
            {
                // Standard melee attack execution with knockback
                float attackPower = activePlayerUnit.unitTemplate.meleeAttackPower;

                DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

                if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, savedSelectedTile))
                {
                    attackPower = attackPower * 2;
                    ApplyKnockback(activePlayerUnit, currentTarget);
                    Debug.Log($"Defending Unit receives {attackPower} damage points");
                }
                else
                {
                    currentTarget.TakeDamage(activePlayerUnit.unitAttackPower * activePlayerUnit.unitMeleeAttackBaseDamage);
                }

                // Reduce the opportunity points after the attack
                activePlayerUnit.unitOpportunityPoints--;

                UpdateActivePlayerUnitProfile(activePlayerUnit);

                activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, currentTarget);
                OnUsedMeleeAction("Melee Attack", activePlayerUnit.unitTemplate.unitName);

                Debug.Log("Melee Execution Logic");
            }
        }
        else
        {
            Debug.Log("Not enough Opportunity Points on Active Player Unit or Unit has already died");
        }
    }

    public void ApplyKnockback(Unit attacker, Unit defender)
    {
        currentTarget.TakeDamage(attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage);

        Vector2Int defenderPos = defender.GetGridPosition();

        // Reuse the knockbackDirection and knockbackStrength calculated during selection
        Vector2Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

        // Clamp the new position to the grid bounds
        newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
        newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

        // Move the defender to the new grid position
        if (defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y, true) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y, true);

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
