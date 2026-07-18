using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Physic Attack Behavior", menuName = "Moveset/PhysicAttackBehavior")]
public class PhysicalAttackBehavior : ScriptableObject
{
    public delegate void UsedPhysicalAttack(string notification);

    public static event UsedPhysicalAttack OnUsedPhysicalAttack;

    public delegate void PhysicalAttackMissed(string notification);

    public static event PhysicalAttackMissed OnPhysicalAttackMissed;

    public delegate void MeleeCriticalHit();

    public static event MeleeCriticalHit OnMeleeCriticalHit;

    public int baseDamage;

    [Header("Melee Accuracy")]
    [Range(0f, 1f)]
    [Tooltip("Base accuracy of melee attacks (0 = always misses, 1 = always hits)")]
    public float baseAccuracy = 0.90f;

    [Header("Melee Critical Strike")]
    [Range(0f, 1f)]
    [Tooltip("Base critical hit chance for melee attacks (0 = never, 1 = always)")]
    public float baseCriticalChance = 0.15f;

    private bool _isCriticalHit;

    // Usiamo Vector3Int
    private Vector3Int knockbackDirection;
    public int knockbackStrength = 2;
    public int selectionLimiter = 1;
    public int meleeRange = 2;

    public delegate void KnockbackFired();

    public static event KnockbackFired OnKnockbackFired;

    public delegate void KnockbackResolved();

    public static event KnockbackResolved OnKnockbackResolved;

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
            activePlayerUnit.battleFeedbackController.PlayMeleeAttackAnimation(activePlayerUnit,
                targetTile.detectedUnit.GetComponent<Unit>());
            return;
        }

        // Safely disallow knockbacking static grid items like Chests or towering Deity monoliths
        bool canKnockback = IsKnockbackPossible(activePlayerUnit, targetUnit.ownedTile)
                            && targetUnit.unitType != Unit.UnitType.Deity
                            && !targetUnit.gameObject.CompareTag("Chest")
                            && targetUnit.unitType != Unit.UnitType.DeityShard;

        if (canKnockback)
        {
            // Attacco con knockback: solo animazione base e applicazione dell'effetto
            Animator activePlayerUnitAnimator = activePlayerUnit.gameObject.GetComponentInChildren<Animator>();
            if (activePlayerUnitAnimator != null)
            {
                activePlayerUnitAnimator.SetTrigger("Attack");
            }

            // Play Sword SFX
            BattleSFXManager.PlaySound(SoundType.SWORDATTACKKNOCKBACK);

            AttemptKnockback(activePlayerUnit, targetUnit);
        }
        else
        {
            // Attacco normale: teletrasporto e HitTarget standard
            activePlayerUnit.battleFeedbackController.PlayMeleeAttackAnimation(activePlayerUnit, targetUnit);
            HitTarget(activePlayerUnit, targetUnit, false);
            BattleSFXManager.PlaySound(SoundType.SWORDATTACK);
        }
    }

    public void AttemptKnockback(Unit attacker, Unit defender)
    {
        if (!IsKnockbackPossible(attacker, defender.ownedTile))
        {
            NotifyKnockbackResolved();
            return;
        }
        if (defender.unitType == Unit.UnitType.Deity || defender.gameObject.CompareTag("Chest"))
        {
            NotifyKnockbackResolved();
            return;
        }

        // Check if the knockback attack hits
        if (!AccuracyChecker.CheckMeleeAccuracy(attacker, defender, baseAccuracy))
        {
            OnPhysicalAttackMissed?.Invoke($"{attacker.unitTemplate.unitName}'s attack missed!");
            NotifyKnockbackResolved();
            return;
        }

        ExecuteKnockback(attacker, defender);

        Vector3Int defenderPos = defender.ownedTile.gridPosition;
        Vector3Int validGridPos = defenderPos;
        TileController finalDestinationTile = null;
        bool isWallKnockback = false;
        bool fellIntoVoid = false;
        int distanceToVoid = 0;

        for (int i = 1; i <= knockbackStrength; i++)
        {
            Vector3Int stepPos = defenderPos + (knockbackDirection * i);
            TileController stepTile = GridManager.Instance.GetTileControllerInstance(stepPos.x, stepPos.z);

            // 1. NULL CHECK: Tile is missing entirely (off-grid or a hole in the map) -> YEET
            if (stepTile == null)
            {
                fellIntoVoid = true;
                distanceToVoid = i;
                break;
            }

            // 2. HIGHER ELEVATION CHECK: Stepping up -> WALL SLAM
            // (This automatically catches decorations built as walls/pillars, since their Y is > baseline)
            if (stepTile.gridPosition.y > defenderPos.y)
            {
                isWallKnockback = true;
                break;
            }

            // 3. LOWER ELEVATION CHECK: Stepping down a cliff -> YEET
            if (stepTile.gridPosition.y < defenderPos.y)
            {
                fellIntoVoid = true;
                distanceToVoid = i;
                break;
            }

            // --- At this point, the step is at the EXACT SAME elevation as the defender ---

            // 4. SAME-ELEVATION DECORATION CHECK: A decoration at floor level -> YEET
            if (stepTile.CompareTag("DecorationEnvironment"))
            {
                fellIntoVoid = true;
                distanceToVoid = i;
                break;
            }

            // 5. PRIZE CHECK: Stop before prize, but not a wall slam
            if (stepTile.tileCurrentFieldPrize != null)
            {
                break;
            }

            // 6. OBSTACLE/UNIT CHECK: Slamming into a hard structural object or unit -> WALL SLAM
            if (stepTile.tileType == TileType.Obstacle || stepTile.tileType == TileType.Environment ||
                stepTile.detectedUnit != null)
            {
                isWallKnockback = true;
                break;
            }

            // 7. VALID FLOOR: It's a standard empty, walkable tile.
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
            Debug.Log($"{defender.unitTemplate.unitName} was slammed into a wall or environment object!");
        }
        else if (fellIntoVoid)
        {
            if (defender.currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
            {
                defender.FallIntoVoid(new Vector2Int(knockbackDirection.x, knockbackDirection.z), distanceToVoid);
            }

            var defenderAgent2 = defender.gameObject.GetComponent<EnemyAgent>();
            if (defenderAgent2 != null)
                defenderAgent2.RemoveElementalBuff(defenderAgent2);

            RemoveInvulnerableMask(defender);
            NotifyKnockbackResolved();
            return; // Early return prevents standard movement logic below
        }

        // --- Execute valid movement ---
        if (finalDestinationTile != null && validGridPos != defenderPos)
        {
            if (defender.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
            {
                finalDestinationTile.tileShaderController.ResetEnemyTileFeedback();
            }
            else if (defender.MoveUnit(validGridPos.x, validGridPos.z, true))
            {
                defender.ownedTile.detectedUnit = null;
                defender.ownedTile.currentSingleTileCondition = SingleTileCondition.free;
                defender.ownedTile.tileShaderController.ResetEnemyTileFeedback();

                MoveUnitToTile(defender, finalDestinationTile);

                finalDestinationTile.tileShaderController.EnemyTileFeedback();
            }
        }

        NotifyKnockbackResolved();
    }

    private void NotifyKnockbackResolved()
    {
        OnKnockbackResolved?.Invoke();
    }

    private void ExecuteKnockback(Unit attacker, Unit defender)
    {
        Vector3Int attackerPos = attacker.ownedTile.gridPosition;
        Vector3Int defenderPos = defender.ownedTile.gridPosition;

        int deltaX = attackerPos.x - defenderPos.x;
        int deltaZ = attackerPos.z - defenderPos.z;

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
        GameObject.FindGameObjectWithTag("CameraDistanceController").GetComponent<CameraDistanceController>()
            .SortUnits();
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
        
        // Apply Faith-based damage modifier
        damageOutput = FaithModifierCalculator.ApplyFaithDamageModifier(damageOutput, attacker);
        
        if (modifierIsActive)
            damageOutput += 2;

        // Extra punitive damage for hitting the wall!
        if (isWallKnockback)
            damageOutput += 3;

        // Apply critical hit multiplier if this is a critical hit
        if (IsCritical(attacker))
        {
            damageOutput *= 1 + Mathf.FloorToInt(attacker.unitAttackPower / 100f);
        }

        // Flatten damage to remove excessive decimal places
        return DamageCalculationUtility.FlattenDamage(damageOutput);
    }

    private bool IsCritical(Unit attacker)
    {
        // Apply Faith modifier to critical hit chance
        float adjustedCritChance = FaithModifierCalculator.ApplyFaithCriticalModifier(baseCriticalChance, attacker);
        
        // Determine if this is a critical hit
        if (Random.value < adjustedCritChance)
        {
            _isCriticalHit = true;
            OnMeleeCriticalHit?.Invoke();
            return true;
        }
        else
        {
            _isCriticalHit = false;
            return false;
        }
    }

    public bool GetIsCriticalHit()
    {
        return _isCriticalHit;
    }
}