using JetBrains.Annotations;
using System;
using Unity.VisualScripting;
using UnityEditor;
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

    public delegate void UsedMagnet();
    public static event UsedMagnet OnUsedMagnet;

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

        if (savedSelectedTile != null)
        {
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
                tile.tileShaderController.ResetTileFadeHeightAnimation(tile);
                Debug.Log("Deselected Melee Attack.");
            }
        }
        //UnitProfilesController.Instance.DestroyEnemyUnitPanel();

        if (savedSelectedTile == null)
        {
            // If no Enemy Unit is selected, deactivates the Spell Player Actions and allows the Player to choose other actions.
            foreach (var tile in GridManager.Instance.gridTileControllers)
            {
                tile.currentPlayerAction = new SelectUnitPlayerAction();
                tile.tileShaderController.AnimateFadeHeight(0, 0.2f, Color.white);
            }
            BattleInterface.Instance.DeactivateActionInfoPanel();
            Debug.Log("Deselected Melee Attack.");
        }
    }

    // Magnet attack logic (used when Magnet is equipped). Note: Hookshot is legacy name for the Magnet.
    public void ExecuteHookshot(Unit attacker, Unit defender)
    {
        if (LookUpDeityComponent(defender) == true)
        {
            return;
        }
        int hookshotRange = 3; // Maximum range of the Magnet.

        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        // Calculate the Manhattan distance (abs(dx) + abs(dy)).
        int distance = Mathf.Abs(defenderPos.x - attackerPos.x) + Mathf.Abs(defenderPos.y - attackerPos.y);

        // Check if the distance is within the hookshot range.
        if (distance > hookshotRange)
        {
            Debug.Log("Enemy is out of hookshot range.");
            return;
        }
        // Calculate the difference in positions.
        int deltaX = defenderPos.x - attackerPos.x;
        int deltaY = defenderPos.y - attackerPos.y;

        // Determine the direction of the pull (opposite of knockback logic).
        Vector2Int pullDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
        {
            pullDirection.x = (int)Mathf.Sign(deltaX); // Pull in X-axis.
        }
        else
        {
            pullDirection.y = (int)Mathf.Sign(deltaY); // Pull in Y-axis.
        }
        // Check if the Enemy has an Invulnerable Mask Buff. If yes, the Magnet disables the Buff.
        if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
        {
            defender.currentUnitBuff = Unit.UnitBuff.Basic;
            // Play mask deactivation feedback on the Enemy.
            defender.gameObject.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
        }
        AnimateConveyorTiles(attackerPos, defenderPos, pullDirection, attacker);

        // Calculate the target tile directly in front of the player based on direction.
        Vector2Int newGridPos = attackerPos + pullDirection;

        // Clamp the position to the grid bounds.
        newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GridManager.Instance.gridHorizontalSize - 1);
        newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GridManager.Instance.gridVerticalSize - 1);

        TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);

        // Ensure the destination tile is not occupied.
        if (destinationTile != null && destinationTile.currentSingleTileCondition != SingleTileCondition.occupied)
        {
            // Move the defender to the new tile in front of the player.
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;

            defender.GetComponent<Unit>().MoveUnit(newGridPos.x, newGridPos.y, true);

            destinationTile.detectedUnit = defender.gameObject;
            defender.ownedTile = destinationTile;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.occupied;

            Debug.Log("Enemy pulled in with hookshot");
            destinationTile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.cyan);

            // Trigger a feedback event for the Magnet usage
            OnUsedMeleeAction?.Invoke("Magnet", attacker.unitTemplate.unitName);
        }
        else
        {
            Debug.Log("No valid position for Magnet pull.");
        }
        OnUsedMagnet?.Invoke();
    }
    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (activePlayerUnit.unitOpportunityPoints <= 0)
            return;
        if (activePlayerUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            return;
        if (activePlayerUnit.unitStatusController.unitCurrentStatus != UnitStatus.basic)
            return;
        if (IsEnemyReachable(activePlayerUnit, targetTile) == false)
            return;
        GameObject enemyObject = targetTile.detectedUnit;
        UseMagnet(activePlayerUnit);
        AttemptKnockback(activePlayerUnit, enemyObject.GetComponent<Unit>());
        HitTarget(activePlayerUnit, enemyObject.GetComponent<Unit>(), targetTile);
        // UnitProfilesController.Instance.UpdateEnemyUnitPanel(targetTile.detectedUnit.gameObject);
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);
        OnUsedMeleeAction("Melee Attack", activePlayerUnit.unitTemplate.unitName);
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlayMeleeAttackAnimation(activePlayerUnit, enemyObject.GetComponent<Unit>());
    }

    private bool IsKnockbackPossible(Unit activePlayerUnit, TileController targetTile)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();

        if (distanceController.CheckDistance(GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>().ownedTile, targetTile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsEnemyReachable(Unit activePlayerUnit, TileController targetTile)
    {
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, targetTile);
        if (distance > meleeRange)
        {
            targetTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
            return false;
        }
        else
        {
            return true;
        }
    }
    private void UseMagnet(Unit activePlayerUnit)
    {
        if (activePlayerUnit.hasHookshot == false)
            return;
        {
            // Execute the Magnet attack instead of the standard Melee attack.
            ExecuteHookshot(activePlayerUnit, currentTarget);

            // Reduce the opportunity points after the attack.
            activePlayerUnit.unitOpportunityPoints--;

            UpdateActivePlayerUnitProfile(activePlayerUnit);
        }
    }
    public void AttemptKnockback(Unit attacker, Unit defender)
    {
        if (!IsKnockbackPossible(attacker, defender.ownedTile))
            return;
        bool modifierIsActive = true;
        HitTarget(attacker, defender, modifierIsActive);
        ExecuteKnockback(attacker, defender);
        Vector2Int defenderPos = defender.GetGridPosition();
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
        if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
        {
            defender.currentUnitBuff = Unit.UnitBuff.Basic;
            // Play mask deactivation feedback on the Enemy.
            defender.gameObject.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
        }

        ResetTileColours();
    }

    private void ExecuteKnockback(Unit attacker, Unit defender)
    {
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        // Calculate the difference in positions.
        int deltaX = attackerPos.x - defenderPos.x;
        int deltaY = attackerPos.y - defenderPos.y;

        // Determine the direction of the knockback.
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

    private void HitTarget(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damage = CalculateDamage(attacker, defender, modifierIsActive);
        defender.TakeDamage(damage);
    }
    private float CalculateDamage(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damageOutput = attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage;
        if (modifierIsActive)
        {
            damageOutput = damageOutput += 2; // Beware, magic number;
        }
        return damageOutput;
    }


    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        //activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
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