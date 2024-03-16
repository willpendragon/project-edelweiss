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

    public void Select(TileController selectedTile)
    {
        spellCastingController = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>();

        if (selectedTile != null && selectionLimiter == 1)
        {
            savedSelectedTile = selectedTile;

            if (spellCastingController.currentSelectedSpell.spellType == SpellType.aoe)
            {
                foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
                {
                    tile.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                    selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                    selectionLimiter--;
                }
                Debug.Log("Selected AOE Spell Range");
            }
            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.singleTarget)
            {
                savedSelectedTile = selectedTile;

                savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;
                if (selectedTile.detectedUnit != null)
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

        if (activePlayerUnit.unitManaPoints > 0)
        {
            if (spellCastingController.currentSelectedSpell.spellType == SpellType.aoe)
            {
                foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
                {
                    Debug.Log("Using AOE Spell on Multiple Targets");
                    tile.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                    if (tile.detectedUnit == null)
                    {
                        Debug.Log("No Unit found. Can't apply damage");
                    }
                    else if (tile.detectedUnit.tag == "Enemy")
                    {
                        tile.detectedUnit.GetComponent<Unit>().TakeDamage(GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage);
                        activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                        activePlayerUnit.unitOpportunityPoints--;
                        UpdateActivePlayerUnitMana(activePlayerUnit);
                        Debug.Log("Applied damage on Enemy Units affected by the AOE Spell");
                        DeityEnmityCheck();
                        PlayVFX(spellCastingController.currentSelectedSpell.spellVFX, tile, spellCastingController.currentSelectedSpell.spellVFXOffset);
                        OnUsedSpell(spellCastingController.currentSelectedSpell.spellName, activePlayerUnit.unitTemplate.unitName);
                    }
                }
            }

            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.singleTarget)
            {
                savedSelectedTile.detectedUnit.GetComponent<Unit>().TakeDamage(spellCastingController.currentSelectedSpell.damage);
                savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                activePlayerUnit.unitOpportunityPoints--;
                UpdateActivePlayerUnitMana(activePlayerUnit);
                DeityEnmityCheck();
                PlayVFX(spellCastingController.currentSelectedSpell.spellVFX, savedSelectedTile, spellCastingController.currentSelectedSpell.spellVFXOffset);
                OnUsedSpell(spellCastingController.currentSelectedSpell.spellName, activePlayerUnit.unitTemplate.unitName);

            }
        }
        else
        {
            Debug.Log("Active Player Unit has not enough Mana Points.");
        }

    }
    public void Deselect()
    {
        selectionLimiter++;
        if (savedSelectedTile == null)
        {
            GridManager.Instance.currentPlayerUnit.GetComponent<UnitSelectionController>().StopUnitAction();
        }
        else
        {
            foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
            {
                tile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;

            }

            savedSelectedTile = null;
            Debug.Log("Selecting AOE Range");
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
