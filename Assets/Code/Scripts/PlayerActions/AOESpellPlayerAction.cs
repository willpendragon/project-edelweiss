using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TileController;
using UnityEngine.UI;
using UnityEngine.Events;

public class AOESpellPlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    SpellcastingController spellCastingController;
    public Deity unboundDeity;

    private int aoeRange = 1;


    public delegate void UsedSpell(string spellName, string casterName);
    public static event UsedSpell OnUsedSpell;

    public delegate void UsedSingleTargetSpell();
    public static event UsedSingleTargetSpell OnUsedSingleTargetSpell;

    public delegate void SpellCriticalHit();
    public static event SpellCriticalHit OnSpellCriticalHit;

    public UnityEvent playSpellVFX;
    public void Select(TileController selectedTile)
    {
        spellCastingController = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>();

        if (selectedTile != null && selectionLimiter > 0)
        {
            if (spellCastingController.currentSelectedSpell.spellType == SpellType.AOE)
            {
                foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(selectedTile, aoeRange))
                {
                    tile.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
                    tile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.magenta);
                    selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                }
                savedSelectedTile = selectedTile;
                selectionLimiter--;
                Debug.Log("Selected AOE Spell Range");
            }

            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.SingleTarget)
            {
                savedSelectedTile = selectedTile;

                savedSelectedTile.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
                if (selectedTile.detectedUnit != null && selectedTile.detectedUnit.tag != "Player" && selectedTile.detectedUnit.tag != "ActivePlayerUnit")
                {
                    currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();
                    selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                    Debug.Log("Selected Single Target Spell Range");
                    selectionLimiter--;
                }

            }

        }

    }

    public void Execute()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

        if (activePlayerUnit.unitManaPoints > 0 && activePlayerUnit.unitOpportunityPoints > 0)
        {
            if (unitManaDoesNotGoBelowZeroAfterUsage(activePlayerUnit.unitManaPoints, spellCastingController.currentSelectedSpell.manaPointsCost))
            {
                Spell currentSpell = spellCastingController.currentSelectedSpell;
                // Determine if this is a critical hit
                bool isCritical = Random.value < currentSpell.criticalHitChance;
                // Base damage calculation now includes attacker's attack power
                int baseDamage = currentSpell.damage + (int)(activePlayerUnit.unitMagicPower * 0.5);
                // Critical hit damage calculation
                int damageToApply = baseDamage * (isCritical ? 1 + Mathf.FloorToInt(activePlayerUnit.unitMagicPower / 100) : 1);

                if (currentSpell.spellType == SpellType.AOE)
                {
                    activePlayerUnit.unitOpportunityPoints--;
                    activePlayerUnit.SpendManaPoints(currentSpell.manaPointsCost);
                    UpdateActivePlayerUnitMana(activePlayerUnit);

                    foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, aoeRange))
                    {
                        Debug.Log("Using AOE Spell on Multiple Targets");
                        tile.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                        tile.tileShaderController.AnimateFadeHeight(0, 0.5f, Color.white);

                        if (tile.detectedUnit == null || tile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
                        {
                            Debug.Log("No Unit found or found Unit has died. Can't apply damage");
                        }
                        else if (tile.detectedUnit.tag == "Enemy")
                        {
                            PlayVFX(currentSpell.spellVFX, tile, currentSpell.spellVFXOffset);
                            activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();

                            // Used Spell notification appears on the Battle Interface
                            OnUsedSpell(currentSpell.spellName, activePlayerUnit.unitTemplate.unitName);

                            // If the Spell is a Critical Hit, sends an event to display the Battle Callout
                            if (isCritical)
                            {
                                OnSpellCriticalHit();
                            }

                            tile.detectedUnit.GetComponent<Unit>().TakeDamage(damageToApply);

                            Debug.Log("Applied " + (isCritical ? "critical " : "") + "damage on Enemy Units affected by the AOE Spell");

                            DeityEnmityCheck();
                        }
                    }
                }
                else if (currentSpell.spellType == SpellType.SingleTarget)
                {
                    if (savedSelectedTile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                    {
                        PlayVFX(currentSpell.spellVFX, savedSelectedTile, currentSpell.spellVFXOffset);
                        activePlayerUnit.GetComponent<BattleFeedbackController>().PlaySpellSFX.Invoke();

                        // Used Spell notification appears on the Battle Interface
                        OnUsedSpell(currentSpell.spellName, activePlayerUnit.unitTemplate.unitName);
                        if (isCritical)
                        {
                            OnSpellCriticalHit();
                        }

                        savedSelectedTile.detectedUnit.GetComponent<Unit>().TakeDamage(damageToApply);
                        activePlayerUnit.SpendManaPoints(currentSpell.manaPointsCost);
                        activePlayerUnit.unitOpportunityPoints--;
                        UpdateActivePlayerUnitMana(activePlayerUnit);
                        OnUsedSingleTargetSpell();
                        DeityEnmityCheck();
                        Debug.Log("Applied " + (isCritical ? "critical " : "") + "damage to the target unit.");
                    }
                }
            }
            else
            {
                Debug.Log("Active Player Unit has not enough Mana Points or enough Opportunity Points.");
            }


        }

    }
    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile != null)
        {
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile, aoeRange))
            {
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
                tile.GetComponentInChildren<SpriteRenderer>().color = Color.white;

                tile.tileShaderController.ResetTileFadeHeightAnimation(tile);
                Debug.Log("Deselecting AOE Range");
            }
        }
    }

    public void DeityEnmityCheck()
    {
        if (GameObject.FindGameObjectWithTag("BattleManager").GetComponent<EnemyTurnManager>().deity == null)
        {
            Debug.Log("No Deity Found");
        }
        else
        {
            unboundDeity = GameObject.FindGameObjectWithTag("BattleManager").GetComponentInChildren<EnemyTurnManager>().deity.GetComponent<Deity>();

            //Looks for the Unbound Deity on the Battlefield
            SpellAlignment spellAlignment = spellCastingController.currentSelectedSpell.alignment;
            //Checks if the alignment of the casted spell is between the list of the Deity's Hated Spell Alignments
            if (unboundDeity.hatedSpellAlignments.Contains(spellAlignment))
            {
                float enmityIncrease = 2.5f;
                //Beware: Magic Numbers
                unboundDeity.enmity += enmityIncrease;
                unboundDeity.deityEnmityTracker.GetComponent<DeityEnmityTrackerController>().UpdateDeityEnmityTracker();
                //Updates the current level of Enmity between the Deity and the Player Unit.
                Debug.Log("Hated Alignment. Deity becomes angrier");
            }
            else
            {
                Debug.Log("Not Hated Alignment. Nothing happens to Deity");
            }
        }
    }

    public void UpdateActivePlayerUnitMana(Unit activePlayerUnit)
    {
        //Misleading method name, as this updates all of the Active Player Profile Unit parameters, not just the manas
        activePlayerUnit.unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(activePlayerUnit);
    }

    public void PlayVFX(GameObject spellVFX, TileController enemyOccupiedTile, Vector3 spellVFXOffset)
    {
        GameObject spellVFXInstance = Instantiate(spellVFX, enemyOccupiedTile.detectedUnit.transform.position, Quaternion.identity);
        spellVFXInstance.transform.localPosition += spellVFXOffset;
        //Beware: Magic numbers
        Debug.Log("Instantiating VFX");
        Destroy(spellVFXInstance, 0.5f);
    }

    public bool unitManaDoesNotGoBelowZeroAfterUsage(float unitManaPoints, float spellPrice)
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
}
