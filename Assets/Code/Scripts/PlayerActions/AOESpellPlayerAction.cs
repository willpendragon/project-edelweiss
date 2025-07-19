using UnityEngine;
using UnityEngine.Events;


public class AOESpellPlayerAction : MonoBehaviour, IPlayerAction
{
    private enum SpellMode
    {
        AOE,
        Formation,
        SingleTarget
    }

    public Unit currentTarget;
    //public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    SpellcastingController spellCastingController;
    public Deity unboundDeity;
    private SpellMode spellMode;

    private int aoeRange = 1;
    private bool _criticalHit;
    private TileController _savedSelectedTile;


    public delegate void SelectedSpell();
    public static event SelectedSpell OnSelectedSpell;

    public delegate void DeselectedSpell();
    public static event DeselectedSpell OnDeselectedSpell;

    public delegate void UsedSpell(string spellName, string casterName);
    public static event UsedSpell OnUsedSpell;

    public delegate void UsedSingleTargetSpell();
    public static event UsedSingleTargetSpell OnUsedSingleTargetSpell;

    public delegate void SpellCriticalHit();
    public static event SpellCriticalHit OnSpellCriticalHit;


    public UnityEvent playSpellVFX;
    public void Execute(TileController targetTile)
    {
        Debug.Log("Executing Spell");
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (CheckTargetTileValidity(targetTile) == false)
            return;

        if (activePlayerUnit.unitManaPoints <= 0 || activePlayerUnit.unitOpportunityPoints <= 0)
            return;

        // BEWARE: Hard-coded logic, retrieves first spell only.
        Spell spell = activePlayerUnit.unitTemplate.spellsList[0];
        SetSpellType(spell);
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

        if (manaPointsAvailable(activePlayerUnit.unitManaPoints, spell.manaPointsCost) == false)
            return;

        spellTarget.TakeDamage(CalculateSpellDamage(spell));
        TriggerSecondaryEffect(spellTarget);
        PlaySpellFeedback(activePlayerUnit, spellTarget, spell);
        SpendResources(activePlayerUnit, spell);

        OnUsedSingleTargetSpell();
        DeityEnmityCheck();
    }

    private void SpendResources(Unit activePlayerUnit, Spell spell)
    {
        activePlayerUnit.SpendManaPoints(spell.manaPointsCost);
        activePlayerUnit.unitOpportunityPoints--;
        UpdateActivePlayerUnitMana(activePlayerUnit);
    }

    private void TriggerSecondaryEffect(Unit spellTarget)
    {
        if (spellTarget.unitStatusController == null)
            return;
        if (spellTarget.unitStatusController.unitCurrentStatus == UnitStatus.stun)
            return;
        spellTarget.unitStatusController.unitCurrentStatus = UnitStatus.stun;
        // Currently this EFFECT works just like the Stun behaviour, but with a different icon.
        // Should retrieve the secondary effect dynamically from the Spell properties.
        PlayFrozenFeedback(spellTarget);
        Debug.Log("The Target is now Stun and unable to move");
    }

    private void PlaySpellFeedback(Unit activePlayerUnit, Unit spellTarget, Spell spell)
    {
        activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();
        // Used Spell notification appears on the Battle Interface
        OnUsedSpell(spell.spellName, activePlayerUnit.unitTemplate.unitName);
        if (_criticalHit == true)
        {
            OnSpellCriticalHit();
        }
        //UnitProfilesController.Instance.UpdateEnemyUnitPanel(spellTarget.gameObject);
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

    //private void CastAOESpell(Spell spell)
    //{
    //    Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

    //    activePlayerUnit.unitOpportunityPoints--;
    //    activePlayerUnit.SpendManaPoints(spell.manaPointsCost);
    //    UpdateActivePlayerUnitMana(activePlayerUnit);

    //    if (CheckTargetTileValidity(savedSelectedTile) == false)
    //        return;

    //    // Used Spell notification appears on the Battle Interface
    //    OnUsedSpell?.Invoke(spell.spellName, activePlayerUnit.unitTemplate.unitName);

    //    foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, aoeRange))
    //    {
    //        if (tile.detectedUnit == null || tile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
    //        {
    //            Debug.Log("No Unit found or found Unit has died. Can't apply damage");
    //        }
    //        else if (tile.detectedUnit.tag == "Enemy")
    //        {
    //            PlayVFX(spell.spellVFX, tile, spell.spellVFXOffset);
    //            activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();

    //            // If the Spell is a Critical Hit, sends an event to display the Battle Callout
    //            if (IsCritical(spell))
    //            {
    //                OnSpellCriticalHit();
    //            }

    //            //tile.detectedUnit.GetComponent<Unit>().TakeDamage(damageToApply);

    //            DeityEnmityCheck();
    //        }
    //    }

    //    PlayVFX(spell.spellVFX, savedSelectedTile, spell.spellVFXOffset);
    //}
    //private void CastFormationSpell(Spell spell)

    //{
    //    if (savedSelectedTile.detectedUnit == null && savedSelectedTile.currentSingleTileCondition == SingleTileCondition.free)
    //    {
    //        // Imbue the Tile with Sacred Triad Power.
    //        savedSelectedTile.tileType = TileType.Triad;
    //        savedSelectedTile.tileShaderController.AnimateFadeHeight(3, 0.1f, Color.cyan);
    //        Debug.Log(savedSelectedTile + "imbued with Sacred Triad Power");
    //    }
    //}

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
        if (IsEnemy(targetTile.detectedUnit) && EnemyIsAlive(targetUnit))
            return true;
        else
            return false;
    }

    public bool IsEnemy(GameObject detectedUnit)
    {
        if (detectedUnit.gameObject.tag == "Enemy")
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
        selectionLimiter++;
        GridManager.Instance.AOESelectionPermitted = true;
        if (_savedSelectedTile == null)
            return;
        GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
        foreach (var tile in gridMovementController.GetMultipleTiles(_savedSelectedTile, aoeRange))
        {
            tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            tile.tileShaderController.ResetTileFadeHeightAnimation(tile);
            Debug.Log("Deselecting AOE Range");
        }
        OnDeselectedSpell();
        UnitProfilesController.Instance.DestroyEnemyUnitPanel();
    }
    public void DeityEnmityCheck()
    {
        if (GameObject.FindGameObjectWithTag("BattleManager").GetComponent<EnemyTurnManager>().deity == null)
            return;
        // Look for the Unbound Deity on the Battlefield.
        unboundDeity = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<EnemyTurnManager>().deity.GetComponent<Deity>();
        // Checks if the alignment of the casted spell is between the list of the Deity's Hated Spell Alignments.
        SpellAlignment spellAlignment = spellCastingController.currentSelectedSpell.alignment;

        if (unboundDeity.hatedSpellAlignments.Contains(spellAlignment))
        {
            // This number should be retrieved dynamically instead.
            float enmityIncrease = 2.5f;
            unboundDeity.enmity += enmityIncrease;
            unboundDeity.deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
            Debug.Log("Hated Alignment. Deity becomes angrier");
        }
    }
    public void UpdateActivePlayerUnitMana(Unit activePlayerUnit)
    {
        //Misleading method name, as this updates all of the Active Player Profile Unit parameters, not just the manas
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }

    public void PlayVFX(GameObject spellVFX, TileController enemyOccupiedTile, Vector3 spellVFXOffset)
    {
        if (enemyOccupiedTile.detectedUnit != null)
        {
            GameObject spellVFXInstance = Instantiate(spellVFX, enemyOccupiedTile.detectedUnit.transform.position, Quaternion.identity);
            spellVFXInstance.transform.localPosition += spellVFXOffset;
            //Beware: Magic numbers
            Debug.Log("Instantiating VFX");
            Destroy(spellVFXInstance, 0.5f);
        }
    }
    public bool manaPointsAvailable(float unitManaPoints, float spellPrice)
    {
        if (unitManaPoints - spellPrice >= 0)
        {
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

        if (targetUnit.GetComponentInChildren<SpriteRenderer>() != null)
        {
            targetUnit.GetComponentInChildren<SpriteRenderer>().material.color = Color.blue;
        }
    }
}
