using Edelweiss.Core;
using ProjectEdelweiss.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class AOESpellPlayerAction : MonoBehaviour, IPlayerAction<TileController>
{
    private enum SpellMode
    {
        AOE,
        Formation,
        SingleTarget
    }

    public Unit currentTarget;
    public int selectionLimiter = 1;
    public Deity unboundDeity;
    private SpellMode spellMode;

    private int aoeRange = 1;
    private bool _criticalHit;

    public delegate void SelectedSpell();

    public static event SelectedSpell OnSelectedSpell;

    public delegate void DeselectedSpell();

    public static event DeselectedSpell OnDeselectedSpell;

    public delegate void UsedSpell(string notification);

    public static event UsedSpell OnUsedSpell;

    public delegate void NotEnoughMana(string notification);

    public static event NotEnoughMana OnNotEnoughMana;

    public delegate void UsedSingleTargetSpell();

    public static event UsedSingleTargetSpell OnUsedSingleTargetSpell;

    public delegate void SpellCriticalHit();

    public static event SpellCriticalHit OnSpellCriticalHit;

    public delegate void DeityAngered();

    public static event DeityAngered OnDeityAngered;

    public delegate void SpellMissed(string notification);

    public static event SpellMissed OnSpellMissed;

    public UnityEvent playSpellVFX;

    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        Spell spell = activePlayerUnit.unitTemplate.spellsList[0];
        SetSpellType(spell);

        // If it's single target, it MUST have a valid unit. 
        // If it's AOE, we allow clicking empty tiles so long as the tile itself exists!
        if (spellMode != SpellMode.AOE)
        {
            if (!CheckTargetTileValidity(targetTile))
                return;
        }
        else
        {
            if (targetTile == null) return; // Basic safety check
        }

        if (activePlayerUnit.unitManaPoints <= 0)
        {
            OnNotEnoughMana?.Invoke("Not enough Mana...");
            return;
        }

        if (spellMode == SpellMode.AOE)
            CastAOESpell(spell, targetTile);
        else
            CastSpell(spell, targetTile);
    }

    private void SetSpellType(Spell spell)
    {
        switch (spell.spellType)
        {
            case (SpellType.SingleTarget):
                spellMode = SpellMode.SingleTarget;
                break;
            case (SpellType.AOE):
                spellMode = SpellMode.AOE;
                break;
            case (SpellType.Formation):
                spellMode = SpellMode.Formation;
                break;
        }
    }

    private void CastSpell(Spell spell, TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        Unit spellTarget = targetTile.detectedUnit.GetComponent<Unit>();

        if (!manaPointsAvailable(activePlayerUnit.unitManaPoints, spell.manaPointsCost))
        {
            OnNotEnoughMana("Not enough Mana...");
            return;
        }

        // Check if the spell hits
        if (!AccuracyChecker.CheckSpellAccuracy(activePlayerUnit, spellTarget, spell))
        {
            OnSpellMissed?.Invoke($"{activePlayerUnit.unitTemplate.unitName}'s {spell.spellName} missed!");
            SpendResources(activePlayerUnit, spell);
            return;
        }

        // Play feedback on Deity Obelisk when applicable.
        if (targetTile.detectedUnit.GetComponent<Unit>().unitType == Unit.UnitType.Deity)
        {
            activePlayerUnit.GetComponent<BattleFeedbackController>()
                .DisplaySpellObeliskDamageFeedback(activePlayerUnit);
        }

        int damageToApply = CalculateSpellDamage(spell, activePlayerUnit);
        spellTarget.TakeDamage(damageToApply);

        // Only spawn the Frozen VFX if the Enemy has HP left after the attack
        if (spell.spellSecundaryEffect == SpellSecundaryEffect.Stun && spellTarget.unitHealthPoints > 0)
            TriggerSecondaryEffect(spellTarget);

        PlaySpellFeedback(activePlayerUnit, spellTarget, spell);
        SpendResources(activePlayerUnit, spell);

        OnUsedSingleTargetSpell?.Invoke();
        DeityEnmityCheck(spell.alignment);
    }

    private void CastAOESpell(Spell spell, TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        if (!manaPointsAvailable(activePlayerUnit.unitManaPoints, spell.manaPointsCost))
            return;

        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController")
            .GetComponent<GridMovementController>();
        List<TileController> affectedTiles = gridMovementController.GetMultipleTiles(targetTile, aoeRange);

        // Check accuracy once for the entire AOE spell
        if (!AccuracyChecker.CheckSpellAccuracy(activePlayerUnit, null, spell))
        {
            OnSpellMissed?.Invoke($"{activePlayerUnit.unitTemplate.unitName}'s {spell.spellName} missed!");
            SpendResources(activePlayerUnit, spell);
            return;
        }

        if (targetTile.detectedUnit != null && targetTile.detectedUnit.GetComponent<Unit>().unitType == Unit.UnitType.Deity)
        {
            activePlayerUnit.GetComponent<BattleFeedbackController>()
                .DisplaySpellObeliskDamageFeedback(activePlayerUnit);
        }

        SpendResources(activePlayerUnit, spell);

        OnUsedSpell?.Invoke($"{activePlayerUnit.unitTemplate.unitName} used {spell.spellName}");

        activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();

        int hitCount = 0;

        foreach (var tile in affectedTiles)
        {
            if (tile.detectedUnit == null || (tile.detectedUnit.tag != "Enemy" && tile.detectedUnit.tag != "Chest" &&
                                              tile.detectedUnit.tag != "DeityShard"))
                continue;

            Unit targetUnit = tile.detectedUnit.GetComponent<Unit>();
            if (targetUnit == null || targetUnit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitAlive)
                continue;

            hitCount++;

            int damageToApply = CalculateSpellDamage(spell, activePlayerUnit);
            targetUnit.TakeDamage(damageToApply);

            if (spell.spellSecundaryEffect == SpellSecundaryEffect.Stun && !tile.detectedUnit.CompareTag("Chest"))
                TriggerSecondaryEffect(targetUnit);

            // Handle per-target crits, enmity, and VFX individually:
            if (_criticalHit == true)
            {
                OnSpellCriticalHit?.Invoke();
            }

            DeityEnmityCheck(spell.alignment);
            PlayVFX(spell.spellVFX, tile, spell.spellVFXOffset);
        }
    }

    private void SpendResources(Unit activePlayerUnit, Spell spell)
    {
        activePlayerUnit.SpendManaPoints(spell.manaPointsCost);
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitProfile(activePlayerUnit);
    }

    private void TriggerSecondaryEffect(Unit spellTarget)
    {
        if (spellTarget.currentUnitBuff == Unit.UnitBuff.InvulnerableMask)
            return;
        if (spellTarget.unitStatusController == null)
            return;
        if (spellTarget.unitStatusController.unitCurrentStatus == UnitStatus.stun)
            return;
        spellTarget.unitStatusController.unitCurrentStatus = UnitStatus.stun;
        PlayFrozenFeedback(spellTarget);
    }

    private void PlaySpellFeedback(Unit activePlayerUnit, Unit spellTarget, Spell spell)
    {
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();
        OnUsedSpell($"{activePlayerUnit.unitTemplate.unitName} used {spell.spellName}");

        if (_criticalHit == true)
        {
            OnSpellCriticalHit();
        }

        PlayVFX(spell.spellVFX, spellTarget.ownedTile, spell.spellVFXOffset);
    }

    private int CalculateSpellDamage(Spell spell, Unit activePlayerUnit)
    {
        int baseDamage = spell.damage + (int)(activePlayerUnit.unitMagicPower * 0.5f);
        float faithModifiedDamage = FaithModifierCalculator.ApplyFaithDamageModifier(baseDamage, activePlayerUnit);
        
        int damageToApply = Mathf.RoundToInt(faithModifiedDamage *
                            (IsCritical(spell, activePlayerUnit) ? 1 + Mathf.FloorToInt(activePlayerUnit.unitMagicPower / 100f) : 1));
        return damageToApply;
    }

    private bool IsCritical(Spell spell, Unit activePlayerUnit)
    {
        float adjustedCritChance = FaithModifierCalculator.ApplyFaithCriticalModifier(spell.criticalHitChance, activePlayerUnit);
        
        if (Random.value < adjustedCritChance)
        {
            _criticalHit = true;
            return true;
        }
        else
        {
            _criticalHit = false;
            return false;
        }
    }

    public bool CheckTargetTileValidity(TileController targetTile)
    {
        if (targetTile.detectedUnit == null)
            return false;

        Unit targetUnit = targetTile.detectedUnit.GetComponent<Unit>();
        if (targetUnit == null)
            return false;

        if (IsAttackable(targetTile.detectedUnit) && EnemyIsAlive(targetUnit))
            return true;
        else
            return false;
    }

    public bool IsAttackable(GameObject detectedUnit)
    {
        if (detectedUnit.gameObject.CompareTag("Enemy") || detectedUnit.gameObject.CompareTag("Chest") ||
            detectedUnit.gameObject.CompareTag("DeityShard"))
            return true;
        else
            return false;
    }

    public bool EnemyIsAlive(Unit enemyUnit)
    {
        if (enemyUnit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitAlive)
            return true;
        else
            return false;
    }

    public void Deselect()
    {
    }

    public void DeityEnmityCheck(SpellAlignment spellAlignment)
    {
        var enemyTurnManager = GameObject.FindGameObjectWithTag(GameTags.ENEMY_TURN_MANAGER)
            .GetComponent<EnemyTurnManager>();
        if (enemyTurnManager.deity == null)
            return;

        unboundDeity = enemyTurnManager.deity.GetComponent<Deity>();

        if (unboundDeity.hatedSpellAlignments.Contains(spellAlignment))
        {
            float enmityIncrease = 2.5f;
            unboundDeity.enmity += enmityIncrease;
            unboundDeity.UpdateDeityEnmitySlider();
            TriggeredFeedback();
        }
    }

    private void TriggeredFeedback()
    {
        if (unboundDeity.enmity >= unboundDeity._maxEnmity)
        {
            OnDeityAngered();
        }
    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateProfile(activePlayerUnit.unitTemplate.unitName);
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate.unitName);
    }

    public void PlayVFX(GameObject spellVFX, TileController enemyOccupiedTile, Vector3 spellVFXOffset)
    {
        GameObject spellVFXInstance = Instantiate(spellVFX, enemyOccupiedTile.transform.position, Quaternion.identity);
        spellVFXInstance.transform.localPosition += spellVFXOffset;
        Debug.Log("Instantiating VFX");
        Destroy(spellVFXInstance, 0.5f);
    }

    public bool manaPointsAvailable(float unitManaPoints, float spellPrice)
    {
        if (unitManaPoints - spellPrice >= 0)
        {
            return true;
        }
        else
        {
            OnNotEnoughMana?.Invoke("Not enough Mana..."); 
            return false;
        }
    }

    private void PlayFrozenFeedback(Unit targetUnit)
    {
        float yOffset = 1.0f;
        Vector3 stunVFXSpawnPosition = targetUnit.transform.position + new Vector3(0, yOffset, 0);

        GameObject stunVFX = Instantiate(Resources.Load<GameObject>("StunAttackVFX"), stunVFXSpawnPosition,
            Quaternion.identity);
        float stunVFXDestroyCountdown = 1.5f;
        Destroy(stunVFX, stunVFXDestroyCountdown);

        if (targetUnit.characterAnimator != null)
        {
            targetUnit.characterAnimator.SetTrigger("Frozen");
        }

        GameObject frozenCubePrefab = Resources.Load<GameObject>("FrozenCube");
        if (frozenCubePrefab != null)
        {
            GameObject frozenCube = Instantiate(frozenCubePrefab, targetUnit.transform);
            frozenCube.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            Debug.LogWarning("FrozenCube prefab not found in Resources.");
        }
    }
}