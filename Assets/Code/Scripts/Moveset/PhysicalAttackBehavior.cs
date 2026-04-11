using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "New Physic Attack Behavior", menuName = "Moveset/PhysicAttackBehavior")]

public class PhysicalAttackBehavior : ScriptableObject
{
    public delegate void UsedPhysicalAttack(string notification);
    public static event UsedPhysicalAttack OnUsedPhysicalAttack;

    public int baseDamage;
    // Usiamo Vector3Int
    private Vector3Int knockbackDirection;
    public int knockbackStrength = 2;
    public int selectionLimiter = 1;
    public int meleeRange = 2;

    public delegate void KnockbackFired();
    public static event KnockbackFired OnKnockbackFired;

    public virtual void AttackSequence(Unit targetUnit, TileController targetTile, Unit activePlayerUnit)
    {
        if (targetTile.tileType == TileType.Obstacle)
        {
            // Subtract Opportunity Points 
            int opportunityPointsCost = 1;
            activePlayerUnit.unitOpportunityPoints -= opportunityPointsCost;

            Beacon beacon = targetTile.detectedUnit.GetComponent<Beacon>();
            beacon.OnHitByUnit();

            // Trigger Character Animation
            activePlayerUnit.battleFeedbackController.PlayMeleeAttackAnimation(activePlayerUnit, targetTile.detectedUnit.GetComponent<Unit>());
            return;
        }

        // Safely disallow knockbacking static grid items like Chests or towering Deity monoliths
        bool canKnockback = IsKnockbackPossible(activePlayerUnit, targetUnit.ownedTile) 
            && targetUnit.unitType != Unit.UnitType.Deity 
            && !targetUnit.gameObject.CompareTag("Chest");

        if (canKnockback)
        {
            // Attacco con knockback: solo animazione base e applicazione dell'effetto
            Animator activePlayerUnitAnimator = activePlayerUnit.gameObject.GetComponentInChildren<Animator>();
            if (activePlayerUnitAnimator != null)
            {
                activePlayerUnitAnimator.SetTrigger("Attack");
            }
            if (activePlayerUnit.battleFeedbackController.PlayMeleeAttackSFX != null)
            {
                activePlayerUnit.battleFeedbackController.PlayMeleeAttackSFX.Invoke();
            }

            AttemptKnockback(activePlayerUnit, targetUnit);
        }
        else
        {
            // Attacco normale: teletrasporto e HitTarget standard
            activePlayerUnit.battleFeedbackController.PlayMeleeAttackAnimation(activePlayerUnit, targetUnit);
            HitTarget(activePlayerUnit, targetUnit, false);
        }
    }

    public void AttemptKnockback(Unit attacker, Unit defender)
    {
        if (!IsKnockbackPossible(attacker, defender.ownedTile))
            return;
        if (defender.unitType == Unit.UnitType.Deity || defender.gameObject.CompareTag("Chest"))
            return;

        ExecuteKnockback(attacker, defender);

        Vector3Int defenderPos = defender.ownedTile.gridPosition;
        Vector3Int validGridPos = defenderPos;
        TileController finalDestinationTile = null;
        bool isWallKnockback = false;

        // Step-by-step path check to find true walls and prevent jumping through solid terrain columns
        for (int i = 1; i <= knockbackStrength; i++)
        {
            Vector3Int stepPos = defenderPos + (knockbackDirection * i);
            stepPos = ClampGridPosition(stepPos);

            // Always get the highest visible surface block at this X/Z column, ignoring underground blocks!
            TileController stepTile = GridManager.Instance.GetTileControllerInstance(stepPos.x, stepPos.z);

            // If there's no map tile here, or the surface is strictly higher than our current height, we hit a wall/barrier!
            if (stepTile == null || stepTile.gridPosition.y > defenderPos.y)
            {
                isWallKnockback = true;
                break; // Stop pushing
            }

            // If another unit is standing on this step tile, we also stop pushing and do not occupy that space
            if (stepTile.detectedUnit != null)
            {
                break;
            }

            // It's a valid empty tile (either same height or a cliff drop-down). Store it as our max push distance so far.
            validGridPos = stepPos;
            finalDestinationTile = stepTile;
        }

        // --- Apply Damage and Modifiers ---
        bool modifierIsActive = true;
        
        HitTarget(attacker, defender, modifierIsActive, isWallKnockback);

        if (OnKnockbackFired != null)
            OnKnockbackFired();

        if (isWallKnockback)
        {
            Debug.Log($"{defender.unitTemplate.unitName} was slammed into a wall!");
        }

        // --- Execute valid movement ---
        // If we found a valid empty tile before hitting the wall (ex: knocked 1 tile, then hit a wall on the 2nd)
        if (finalDestinationTile != null && validGridPos != defenderPos)
        {
            if (defender.MoveUnit(validGridPos.x, validGridPos.z, true) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                defender.ownedTile.detectedUnit = null;
                defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

                MoveUnitToTile(defender, finalDestinationTile);
                
                finalDestinationTile.tileShaderController.EnemyTileFeedback();
            }
        }

        var defenderAgent = defender.gameObject.GetComponent<EnemyAgent>();
        if (defenderAgent != null)
            defenderAgent.RemoveElementalBuff(defenderAgent);

        RemoveInvulnerableMask(defender);
    }

    private void ExecuteKnockback(Unit attacker, Unit defender)
    {
        Vector3Int attackerPos = attacker.ownedTile.gridPosition;
        Vector3Int defenderPos = defender.ownedTile.gridPosition;

        int deltaX = attackerPos.x - defenderPos.x;
        int deltaZ = attackerPos.z - defenderPos.z; // Use Z instead of Y!

        knockbackDirection = Vector3Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaZ))
            knockbackDirection.x = -(int)Mathf.Sign(deltaX);
        else
            knockbackDirection.z = -(int)Mathf.Sign(deltaZ);

        knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);
    }

    private bool IsKnockbackPossible(Unit activePlayerUnit, TileController targetTile)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();
        // Utilizziamo l'algoritmo puro in DistanceController
        return distanceController.CheckDistance(activePlayerUnit.ownedTile, targetTile);
    }

    private Vector3Int ClampGridPosition(Vector3Int pos)
    {
        var grid = GridManager.Instance;
        pos.x = Mathf.Clamp(pos.x, 0, grid.gridHorizontalSize - 1);
        pos.z = Mathf.Clamp(pos.z, 0, grid.gridVerticalSize - 1);
        return pos;
    }

    private void RemoveInvulnerableMask(Unit defender)
    {
        if (defender.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
        {
            defender.currentUnitBuff = Unit.UnitBuff.Basic;
            defender.GetComponentInChildren<MaskFeedbackHelper>()?.DeactivateMask();
        }
    }

    public virtual void MoveUnitToTile(Unit unit, TileController destinationTile)
    {
        if (unit == null || destinationTile == null)
            return;

        // Update previous tile
        if (unit.ownedTile != null)
        {
            unit.ownedTile.detectedUnit = null;
            unit.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
        }

        // Update new tile
        destinationTile.detectedUnit = unit.gameObject;
        destinationTile.currentSingleTileCondition = SingleTileCondition.occupied;

        // Update unit's reference
        unit.ownedTile = destinationTile;
        GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>().SortUnits();
    }

    public virtual void BroadcastAttackNotification(string message)
    {
        OnUsedPhysicalAttack(message);
    }

    public virtual int GetAttackRange()
    {
        return meleeRange;
    }

    protected void HitTarget(Unit attacker, Unit defender, bool modifierIsActive, bool isWallKnockback = false)
    {
        float damage = CalculateDamage(attacker, defender, modifierIsActive, isWallKnockback);
        defender.TakeDamage(damage);
        
        string wallMessage = isWallKnockback ? " (Wall Slam!)" : "";
        BroadcastAttackNotification($"{attacker.unitTemplate.unitName} used Melee Attack" + wallMessage);
    }

    private float CalculateDamage(Unit attacker, Unit defender, bool modifierIsActive, bool isWallKnockback = false)
    {
        float damageOutput = attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage;
        if (modifierIsActive)
            damageOutput += 2;
            
        // Extra punitive damage for hitting the wall!
        if (isWallKnockback)
            damageOutput += 3;
            
        return damageOutput;
    }
}