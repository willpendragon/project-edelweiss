using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static GridTargetingController;
using static TileController;
using UnityEngine.UI;
using Unity.VisualScripting;

public class AOESpellPlayerAction : MonoBehaviour, IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    SpellcastingController spellCastingController;
    public Deity unboundDeity;

    public delegate void UsedSpell(string spellName, string casterName);
    public static event UsedSpell OnUsedSpell;

    public delegate void UsedSingleTargetSpell();
    public static event UsedSingleTargetSpell OnUsedSingleTargetSpell;

    public void Select(TileController selectedTile)
    {
        spellCastingController = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>();

        if (selectedTile != null && selectionLimiter > 0)
        {
            if (spellCastingController.currentSelectedSpell.spellType == SpellType.aoe)
            {
                foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(selectedTile))
                {
                    tile.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
                    selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                }
                savedSelectedTile = selectedTile;
                selectionLimiter--;
                Debug.Log("Selected AOE Spell Range");
            }

            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.singleTarget)
            {
                savedSelectedTile = selectedTile;

                savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.cyan;
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
            if (spellCastingController.currentSelectedSpell.spellType == SpellType.aoe)
            {
                foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
                {
                    Debug.Log("Using AOE Spell on Multiple Targets");
                    tile.GetComponentInChildren<SpriteRenderer>().material.color = Color.red;
                    if (tile.detectedUnit == null || tile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
                    {
                        Debug.Log("No Unit found or found Unit has died. Can't apply damage");
                    }
                    else if (tile.detectedUnit.tag == "Enemy")
                    {
                        PlayVFX(spellCastingController.currentSelectedSpell.spellVFX, tile, spellCastingController.currentSelectedSpell.spellVFXOffset);
                        OnUsedSpell(spellCastingController.currentSelectedSpell.spellName, activePlayerUnit.unitTemplate.unitName);
                        tile.detectedUnit.GetComponent<Unit>().TakeDamage(GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage);
                        activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                        activePlayerUnit.unitOpportunityPoints--;
                        UpdateActivePlayerUnitMana(activePlayerUnit);
                        Debug.Log("Applied damage on Enemy Units affected by the AOE Spell");
                        DeityEnmityCheck();

                    }
                }
            }

            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.singleTarget)
            {
                if (savedSelectedTile.detectedUnit.GetComponent<Unit>().currentUnitLifeCondition != Unit.UnitLifeCondition.unitDead)
                {
                    PlayVFX(spellCastingController.currentSelectedSpell.spellVFX, savedSelectedTile, spellCastingController.currentSelectedSpell.spellVFXOffset);
                    OnUsedSpell(spellCastingController.currentSelectedSpell.spellName, activePlayerUnit.unitTemplate.unitName);
                    savedSelectedTile.detectedUnit.GetComponent<Unit>().TakeDamage(spellCastingController.currentSelectedSpell.damage);
                    savedSelectedTile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
                    activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                    activePlayerUnit.unitOpportunityPoints--;
                    UpdateActivePlayerUnitMana(activePlayerUnit);
                    OnUsedSingleTargetSpell();
                    DeityEnmityCheck();

                }
            }
        }
        else
        {
            Debug.Log("Active Player Unit has not enough Mana Points or enough Opportunity Points.");
        }

    }
    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile != null)
        {
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
            {
                tile.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
                tile.currentSingleTileStatus = SingleTileStatus.selectionMode;
            }
            Debug.Log("Deselecting AOE Range");
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
}
