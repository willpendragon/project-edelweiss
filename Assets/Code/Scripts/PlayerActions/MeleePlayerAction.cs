using UnityEngine;
using static TileController;

public class MeleePlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    private int meleeRange = 2;

    public Vector2Int knockbackDirection; // Stores knockback direction
    public int knockbackStrength = 2; // Stores knockback strength

    public delegate void UsedMeleeAction(string moveName, string attackerName);
    public static event UsedMeleeAction OnUsedMeleeAction;

    public void Select(TileController selectedTile)
    {
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, selectedTile);
        Debug.Log($"Calculated distance: {distance}");

        if (distance > meleeRange)
        {
            // Play the error animation for the invalid selection.
            selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            return;
        }

        if (selectedTile.detectedUnit == null)
        {
            // No enemy is present on the selected tile.
            selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            return;
        }

        currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();

        if (currentTarget != null && currentTarget.CompareTag("Enemy"))
        {
            savedSelectedTile = selectedTile;
            selectionLimiter--;

            // Reset previous enemy panel before creating a new one
            UnitProfilesController.Instance.DestroyEnemyUnitPanel();
            UnitProfilesController.Instance.CreateEnemyUnitPanel(currentTarget.gameObject);

            selectedTile.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.cyan;
            selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;

            if (activePlayerUnit.hasHookshot)
            {
                ActivateMagnet(activePlayerUnit, currentTarget);
                Debug.Log("Hookshot selected, waiting for confirmation...");
            }
            else
            {
                CheckKnockback(activePlayerUnit, currentTarget);
            }
        }
        else
        {
            Debug.Log("No Enemy Found");
        }
    }

    public void Deselect()
    {
        selectionLimiter++;
        ResetTileColours();
        MoveInfoController.Instance.HideActionInfoPanel();

        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit != null && activePlayerUnit.hasHookshot == true)
        {
            MagnetHelper magnetHelper = activePlayerUnit.gameObject.GetComponentInChildren<MagnetHelper>();
            magnetHelper.DestroyMagnet();
        }
        UnitProfilesController.Instance.DestroyEnemyUnitPanel();
    }

    // Method for knockback logic (used for normal melee attacks)
    public void CheckKnockback(Unit attacker, Unit defender)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

        if (distanceController.CheckDistance(attacker.ownedTile, savedSelectedTile) && LookUpDeityComponent(defender) != true)
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
        if (LookUpDeityComponent(defender) == true)
        {
            return;
        }
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

        AnimateConveyorTiles(attackerPos, defenderPos, pullDirection, attacker);

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
            OnUsedMeleeAction?.Invoke("Magnet", attacker.unitTemplate.unitName);
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

                // Reduce the opportunity points after the attack
                activePlayerUnit.unitOpportunityPoints--;

                UpdateActivePlayerUnitProfile(activePlayerUnit);
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
                UnitProfilesController.Instance.UpdateEnemyUnitPanel(currentTarget.gameObject);
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

    private void ActivateMagnet(Unit attacker, Unit defender)
    {
        attacker.gameObject.GetComponentInChildren<MagnetHelper>().OrientMagnet(attacker, defender);
    }
    private void AnimateConveyorTiles(Vector2Int attackerPos, Vector2Int defenderPos, Vector2Int pullDirection, Unit attacker)
    {
        // Calculate the number of tiles between the attacker and the defender
        int distance = Mathf.Abs(defenderPos.x - attackerPos.x) + Mathf.Abs(defenderPos.y - attackerPos.y);

        Vector2Int currentPos = attackerPos;

        // Loop through each tile along the path from attacker to defender
        for (int i = 0; i < distance; i++)
        {
            currentPos += pullDirection; // Move to the next tile in the direction of the pull

            // Get the tile controller for the current tile
            TileController currentTile = GridManager.Instance.GetTileControllerInstance(currentPos.x, currentPos.y);

            if (currentTile != null)
            {
                // Retrieve the conveyor belt plane or mesh
                GameObject conveyorPlane = currentTile.GetComponentInChildren<ConveyorBeltHelper>().gameObject;

                if (conveyorPlane != null)
                {
                    // Calculate the direction vector based on the pull direction
                    Vector3 direction = new Vector3(pullDirection.x, 0, pullDirection.y);

                    // Set the rotation using Quaternion.LookRotation to face the direction
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    conveyorPlane.transform.rotation = rotation;

                    // Optional: Manage the conveyor belt behavior if needed
                    ConveyorBeltHelper conveyorBeltHelper = conveyorPlane.GetComponent<ConveyorBeltHelper>();
                    if (conveyorBeltHelper != null)
                    {
                        conveyorBeltHelper.ManageConveyorBelt(1);
                    }
                }
            }
        }
    }

    private bool LookUpDeityComponent(Unit defenderUnit)
    {
        // The purpose of this method is to check if the target Unit has a Deity component on it.
        if (defenderUnit.gameObject.GetComponent<Deity>() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}