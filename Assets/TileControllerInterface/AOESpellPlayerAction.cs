using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;
using static TileController;
using UnityEngine.UI;


public class AOESpellPlayerAction : IPlayerAction
{
    public Unit currentTarget;
    public TileController savedSelectedTile;
    public int selectionLimiter = 1;
    SpellcastingController spellCastingController;

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
                currentTarget = selectedTile.detectedUnit.GetComponent<Unit>();
                selectedTile.currentSingleTileStatus = SingleTileStatus.waitingForConfirmationMode;
                Debug.Log("Selected Single Target Spell Range");
                selectionLimiter--;
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
                    activePlayerUnit.unitOpportunityPoints--;
                    if (tile.detectedUnit == null)
                    {
                        Debug.Log("No Unit found. Can't apply damage");
                    }
                    else if (tile.detectedUnit.tag == "Enemy")
                    {
                        tile.detectedUnit.GetComponent<Unit>().TakeDamage(GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage);
                        activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                        activePlayerUnit.unitOpportunityPoints--;
                        Debug.Log("Applied damage on Enemy Units affected by the AOE Spell");
                    }
                }
            }

            else if (spellCastingController.currentSelectedSpell.spellType == SpellType.singleTarget)
            {
                savedSelectedTile.detectedUnit.GetComponent<Unit>().TakeDamage(spellCastingController.currentSelectedSpell.damage);
                savedSelectedTile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                activePlayerUnit.SpendManaPoints(spellCastingController.currentSelectedSpell.manaPointsCost);
                activePlayerUnit.unitOpportunityPoints--;
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

        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(savedSelectedTile))
        {
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            savedSelectedTile.currentSingleTileStatus = SingleTileStatus.selectionMode;

        }
        savedSelectedTile = null;
        Debug.Log("Selecting AOE Range");
    }
}
