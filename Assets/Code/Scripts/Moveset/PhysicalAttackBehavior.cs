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
    private Vector2Int knockbackDirection;
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
        // Zoom Camera
        if (OnKnockbackFired != null) 
            OnKnockbackFired();

        Vector2Int defenderPos = defender.GetGridPosition();
        Vector2Int newGridPos = defenderPos + (knockbackDirection * knockbackStrength);

        newGridPos = ClampGridPosition(newGridPos);

        TileController projectedTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);
        if (projectedTile == null)
            return;
        if (projectedTile.detectedUnit != null)
            return;

        if (defender.MoveUnit(newGridPos.x, newGridPos.y, true) && defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
        {
            defender.ownedTile.detectedUnit = null;
            defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
            defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

            TileController destinationTile = GridManager.Instance.GetTileControllerInstance(newGridPos.x, newGridPos.y);
            MoveUnitToTile(defender, destinationTile);
            // If the Enemy it's still alive, the Enemy Tile Feedback (Red Tile) should still be present.
            destinationTile.tileShaderController.EnemyTileFeedback();
        }
        var defenderAgent = defender.gameObject.GetComponent<EnemyAgent>();
        if (defenderAgent != null)
            defenderAgent.RemoveElementalBuff(defenderAgent);

        RemoveInvulnerableMask(defender);

        //ResetTileColours();
    }

    private void ExecuteKnockback(Unit attacker, Unit defender)
    {
        Vector2Int attackerPos = attacker.GetGridPosition();
        Vector2Int defenderPos = defender.GetGridPosition();

        int deltaX = attackerPos.x - defenderPos.x;
        int deltaY = attackerPos.y - defenderPos.y;

        knockbackDirection = Vector2Int.zero;
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            knockbackDirection.x = -(int)Mathf.Sign(deltaX);
        else
            knockbackDirection.y = -(int)Mathf.Sign(deltaY);

        knockbackStrength = Mathf.Clamp(knockbackStrength, 1, 3);

        Vector2Int previewGridPos = defenderPos + (knockbackDirection * knockbackStrength);
        previewGridPos = ClampGridPosition(previewGridPos);

        TileController previewTile = GridManager.Instance.GetTileControllerInstance(previewGridPos.x, previewGridPos.y);
    }

    private bool IsKnockbackPossible(Unit activePlayerUnit, TileController targetTile)
    {
        DistanceController distanceController = GridManager.Instance.GetComponentInChildren<DistanceController>();
        return distanceController.CheckDistance(activePlayerUnit.ownedTile, targetTile);
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

    private Vector2Int ClampGridPosition(Vector2Int pos)
    {
        var grid = GridManager.Instance;
        pos.x = Mathf.Clamp(pos.x, 0, grid.gridHorizontalSize - 1);
        pos.y = Mathf.Clamp(pos.y, 0, grid.gridVerticalSize - 1);
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

}