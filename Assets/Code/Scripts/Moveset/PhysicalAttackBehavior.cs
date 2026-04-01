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

        bool canKnockback = IsKnockbackPossible(activePlayerUnit, targetUnit.ownedTile) && targetUnit.unitType != Unit.UnitType.Deity;

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
        if (defender.unitType == Unit.UnitType.Deity)
            return;

        bool modifierIsActive = true;
        HitTarget(attacker, defender, modifierIsActive);
        ExecuteKnockback(attacker, defender);
        
        if (OnKnockbackFired != null)
            OnKnockbackFired();

        // 3D Voxel Coordinate Handling
        Vector3Int defenderPos = defender.ownedTile.gridPosition;
        Vector3Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

        newGridPos = ClampGridPosition(newGridPos);

        TileController projectedTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y, newGridPos.z);
        if (projectedTile == null)
            return;
        if (projectedTile.detectedUnit != null)
            return;

        // Unit.MoveUnit deve ricevere le coord X e Z (se non l'hai convertito al Vector3Int pieno)
        if (defender.MoveUnit(newGridPos.x, newGridPos.z, true) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

            TileController destinationTile = projectedTile;
            MoveUnitToTile(defender, destinationTile);
            
            destinationTile.tileShaderController.EnemyTileFeedback();
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
        int deltaZ = attackerPos.z - defenderPos.z; // Z al posto della Y!

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

    protected void HitTarget(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damage = CalculateDamage(attacker, defender, modifierIsActive);
        defender.TakeDamage(damage);
        BroadcastAttackNotification($"{attacker.unitTemplate.unitName} used Melee Attack");
    }

    private float CalculateDamage(Unit attacker, Unit defender, bool modifierIsActive)
    {
        float damageOutput = attacker.unitAttackPower * attacker.unitMeleeAttackBaseDamage;
        if (modifierIsActive)
            damageOutput += 2;
        return damageOutput;
    }
}