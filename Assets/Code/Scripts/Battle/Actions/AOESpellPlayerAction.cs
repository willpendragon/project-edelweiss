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


    public UnityEvent playSpellVFX;
    public void Execute(TileController targetTile)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (!CheckTargetTileValidity(targetTile))
            return;

        if (activePlayerUnit.unitManaPoints <= 0)
        {
            OnNotEnoughMana("Not enough Mana...");
            return;
        }

        Spell spell = activePlayerUnit.unitTemplate.spellsList[0];
        SetSpellType(spell);

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

        // Play feedback on Deity Obelisk when applicable.
        if (targetTile.detectedUnit.GetComponent<Unit>().unitType == Unit.UnitType.Deity)
        {
            activePlayerUnit.GetComponent<BattleFeedbackController>().DisplaySpellObeliskDamageFeedback(activePlayerUnit);
        }

        int damageToApply = CalculateSpellDamage(spell);
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

        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        List<TileController> affectedTiles = gridMovementController.GetMultipleTiles(targetTile, aoeRange);

        // Play feedback on Deity Obelisk when applicable.
        if (targetTile.detectedUnit.GetComponent<Unit>().unitType == Unit.UnitType.Deity)
        {
            activePlayerUnit.GetComponent<BattleFeedbackController>().DisplaySpellObeliskDamageFeedback(activePlayerUnit);
        }

        SpendResources(activePlayerUnit, spell);

        OnUsedSpell?.Invoke($"{activePlayerUnit.unitTemplate.unitName} used {spell.spellName}");

        foreach (var tile in affectedTiles)
        {
            if (tile.detectedUnit == null || (tile.detectedUnit.tag != "Enemy" && tile.detectedUnit.tag != "Chest"))
                continue;

            Unit targetUnit = tile.detectedUnit.GetComponent<Unit>();
            if (targetUnit == null || targetUnit.currentUnitLifeCondition != Unit.UnitLifeCondition.unitAlive)
                continue;

            int damageToApply = CalculateSpellDamage(spell);
            targetUnit.TakeDamage(damageToApply);

            // Avoid attempting to Stun a lifeless Chest
            if (spell.spellSecundaryEffect == SpellSecundaryEffect.Stun && !tile.detectedUnit.CompareTag("Chest"))
                TriggerSecondaryEffect(targetUnit);

            PlaySpellFeedback(activePlayerUnit, targetUnit, spell);
            DeityEnmityCheck(spell.alignment);
            PlayVFX(spell.spellVFX, tile, spell.spellVFXOffset);
        }
    }


    private void SpendResources(Unit activePlayerUnit, Spell spell)
    {
        activePlayerUnit.SpendManaPoints(spell.manaPointsCost);
        activePlayerUnit.unitOpportunityPoints--;
        // Update on the UI.
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
        // Currently this EFFECT works just like the Stun behaviour, but with a different icon.
        // Should retrieve the secondary effect dynamically from the Spell properties.
        PlayFrozenFeedback(spellTarget);
    }

    private void PlaySpellFeedback(Unit activePlayerUnit, Unit spellTarget, Spell spell)
    {
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();
        // Used Spell notification appears on the Battle Interface
        OnUsedSpell($"{activePlayerUnit.unitTemplate.unitName} used {spell.spellName}");

        if (_criticalHit == true)
        {
            OnSpellCriticalHit();
        }

        PlayVFX(spell.spellVFX, spellTarget.ownedTile, spell.spellVFXOffset);
    }

    private int CalculateSpellDamage(Spell spell)
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();
        // Base damage calculation retrieves the Attack Power from the Attacker's statistics.
        int baseDamage = spell.damage + (int)(activePlayerUnit.unitMagicPower * 0.5);
        // Critical hit damage calculation.
        int damageToApply = baseDamage * (IsCritical(spell) ? 1 + Mathf.FloorToInt(activePlayerUnit.unitMagicPower / 100) : 1);
        return damageToApply;
    }

    private bool IsCritical(Spell spell)
    {
        // Determine if this is a critical hit
        if (Random.value < spell.criticalHitChance)
        {
            _criticalHit = true;
            return true;
        }
        else
        {
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
        // Treat both proper enemies and breakable chests as attackable elements
        if (detectedUnit.gameObject.CompareTag("Enemy") || detectedUnit.gameObject.CompareTag("Chest"))
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
        var enemyTurnManager = GameObject.FindGameObjectWithTag(GameTags.ENEMY_TURN_MANAGER).GetComponent<EnemyTurnManager>();
        if (enemyTurnManager.deity == null)
            return;

        // Look for the Unbound Deity on the Battlefield.
        unboundDeity = enemyTurnManager.deity.GetComponent<Deity>();

        // Checks if the alignment of the casted spell is between the list of the Deity's Hated Spell Alignments.
        if (unboundDeity.hatedSpellAlignments.Contains(spellAlignment))
        {
            // This number should be retrieved dynamically instead.
            float enmityIncrease = 2.5f;
            unboundDeity.enmity += enmityIncrease;
            unboundDeity.UpdateDeityEnmitySlider();
            TriggeredFeedback();
        }
    }

    private void TriggeredFeedback()
    {
        // Display Triggered Deity feedback
        if (unboundDeity.enmity >= unboundDeity._maxEnmity)
        {
            OnDeityAngered();
        }
    }

    public void UpdateActivePlayerUnitProfile(Unit activePlayerUnit)
    {
        //activePlayerUnit.unitProfilePanel.GetComponent<UnitProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
        // Use the centralized logic.
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateProfile(activePlayerUnit.unitTemplate.unitName);
        BattleInterface.Instance.PlayerPartyProfilesUIManager.UpdateRemainingMoves(activePlayerUnit.unitTemplate.unitName);
    }

    public void PlayVFX(GameObject spellVFX, TileController enemyOccupiedTile, Vector3 spellVFXOffset)
    {
        GameObject spellVFXInstance = Instantiate(spellVFX, enemyOccupiedTile.transform.position, Quaternion.identity);
        spellVFXInstance.transform.localPosition += spellVFXOffset;
        //Beware: Magic numbers
        Debug.Log("Instantiating VFX");
        Destroy(spellVFXInstance, 0.5f);
    }
    public bool manaPointsAvailable(float unitManaPoints, float spellPrice)
    {
        if (unitManaPoints - spellPrice >= 0)
        {
            OnNotEnoughMana("Not enough Mana...");
            return true;
        }
        else
        {
            return false;
        }
    }
    private void PlayFrozenFeedback(Unit targetUnit)
    {
        // Define the Y offset for the VFX spawn position
        float yOffset = 1.0f;

        // Calculate the new spawn position with the Y offset
        Vector3 stunVFXSpawnPosition = targetUnit.transform.position + new Vector3(0, yOffset, 0);

        // Instantiate the VFX at the new position
        GameObject stunVFX = Instantiate(Resources.Load<GameObject>("StunAttackVFX"), stunVFXSpawnPosition, Quaternion.identity);
        float stunVFXDestroyCountdown = 1.5f;
        Destroy(stunVFX, stunVFXDestroyCountdown);

        if (targetUnit.characterAnimator != null)
        {
            targetUnit.characterAnimator.SetTrigger("Frozen");
        }

        // Spawn Frozen Cube
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
