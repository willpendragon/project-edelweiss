using UnityEngine;
using UnityEngine.Events;

public class AOESpellPlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    SpellcastingController spellCastingController;
    public Deity unboundDeity;

    private int aoeRange = 1;

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
    public void Select(TileController selectedTile)
    {
        spellCastingController = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>();
        OnSelectedSpell();
        if (selectedTile != null && selectionLimiter > 0)
        {
            GridMovementController gridMovementController = GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>();
            Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit").GetComponent<Unit>();

            int distance = gridMovementController.GetDistance(activePlayerUnit.ownedTile, selectedTile);
            Debug.Log($"Calculated distance: {distance}");

            int spellRange = spellCastingController.currentSelectedSpell.spellRange;

            if (spellCastingController.currentSelectedSpell.spellType == SpellType.AOE)
            {
                if (distance > spellRange)
                {
                    // Play the error animation for the invalid selection
                    selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
                    Debug.Log("Unable to select tile - Out of range");
                    return; // Exit the method early, preventing further execution
                }
                else
                {
                    // Valid selection: highlight and set the state
                    foreach (var tile in gridMovementController.GetMultipleTiles(selectedTile, aoeRange))
                    {
                        tile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.magenta);
                    }

                    selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                    savedSelectedTile = selectedTile; // Save the valid tile
                    selectionLimiter--;
                    Debug.Log("Selected AOE Spell Range");
                }
            }
            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.SingleTarget)
            {
                // Check if the selected tile is within range for single-target spells
                if (distance > spellRange)
                {
                    // Play the error animation for the invalid selection
                    selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
                    Debug.Log("Unable to select tile - Out of range");
                    return; // Exit the method early, preventing further execution
                }
                else
                {
                    savedSelectedTile = selectedTile;
                    savedSelectedTile.tileShaderController.AnimateFadeHeight(2.75f, 0.5f, Color.blue);

                    if (selectedTile.detectedUnit != null && selectedTile.detectedUnit.tag != "Player" && selectedTile.detectedUnit.tag != "ActivePlayerUnit")
                    {
                        currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();
                        UnitProfilesController.Instance.CreateEnemyUnitPanel(currentTarget.gameObject);
                        selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                        Debug.Log("Selected Single Target Spell Range");
                        selectionLimiter--;
                    }
                    else
                    {
                        // No valid unit found on the selected tile, play an error animation
                        selectedTile.tileShaderController.AnimateFadeHeightError(2.75f, 0.5f, Color.red);
                        Debug.Log("No valid unit found for single-target spell selection");
                    }
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
                        Unit spellTarget = savedSelectedTile.detectedUnit.GetComponent<Unit>();
                        spellTarget.TakeDamage(damageToApply);
                        UnitProfilesController.Instance.UpdateEnemyUnitPanel(currentTarget.gameObject);
                        if (spellTarget.unitStatusController != null && spellTarget.unitStatusController.unitCurrentStatus != UnitStatus.stun)
                        {
                            spellTarget.unitStatusController.unitCurrentStatus = UnitStatus.stun;
                            PlayFrozenFeedback(spellTarget);
                            //Actually this is the Stun behaviour but with a different icon
                        }
                        Debug.Log("The Target is now Stun and unable to move");
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
                tile.tileShaderController.ResetTileFadeHeightAnimation(tile);
                Debug.Log("Deselecting AOE Range");
            }
            OnDeselectedSpell();
        }
        UnitProfilesController.Instance.DestroyEnemyUnitPanel();
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
