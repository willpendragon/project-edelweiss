using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellcastingController : MonoBehaviour
{
    public delegate void CastingSpell();
    public static event CastingSpell OnCastingSpell;

    public delegate void CastingSpellAOE();
    public static event CastingSpellAOE OnCastingSpellAOE;

    public delegate void CastedSpellTextNotification(string spellName);
    public static event CastedSpellTextNotification OnCastedSpellTextNotification;

    public Unit currentTargetedUnit;
    public Spell currentSelectedSpell;
    public TileController aoeSpellEpicenterTile;

    public delegate void CastedSpellTypeHatedbyDeity();
    public static event CastedSpellTypeHatedbyDeity OnCastedSpellTypeHatedbyDeity;
    public void OnEnable()
    {
        GridTargetingController.OnTargetedUnit += SetTargetedUnit;
        TileController.OnTileConfirmedAttackMode += UseCurrentSpellOnCurrentTarget;
        TileController.OnTileWaitingForConfirmationAOESpellMode += SetAOESpellEpicenter;
        TileController.OnTileConfirmedAOESpellMode += UseCurrentSpellOnCurrentTargets;
    }
    public void OnDisable()
    {
        GridTargetingController.OnTargetedUnit -= SetTargetedUnit;
        TileController.OnTileConfirmedAttackMode -= UseCurrentSpellOnCurrentTarget;
        TileController.OnTileWaitingForConfirmationAOESpellMode -= SetAOESpellEpicenter;
        TileController.OnTileConfirmedAOESpellMode -= UseCurrentSpellOnCurrentTargets;
    }
    public void CastSpell(Spell castedSpell)
    {
        //Sends an event delegate that activates the Attack Selection Mode on the Grid Targeting Controller.
        //Sets the current Target Enemy and the current Spell
        if (castedSpell != null && castedSpell.spellType == SpellType.singleTarget)
        {
            OnCastingSpell();
            currentSelectedSpell = castedSpell;
            Debug.Log("Single Target - Now Casting" + castedSpell);
        }
        else if (castedSpell != null && castedSpell.spellType == SpellType.aoe)
        {
            OnCastingSpellAOE();
            currentSelectedSpell = castedSpell;
            Debug.Log("AOE - Now Casting" + castedSpell);
        }

    }
    public void SetTargetedUnit(Unit targetedUnit)
    {
        //Sets the currently Targeted Enemy.
        currentTargetedUnit = targetedUnit;
    }
    public void UseCurrentSpellOnCurrentTarget()
    {
        if (CheckActivePlayerUnitOpportunityPoints())
        {
            if ((GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitManaPoints >= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.manaPointsCost))
            {
                //Spell damage calculation logic
                currentTargetedUnit.HealthPoints -= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage;
                //This is a legacy method that applies the damage on the Enemy and executes the Enemy damaged feedback
                //currentTargetedUnit.gameObject.GetComponent<EnemyAgent>().EnemyTakingDamage.Invoke();
                SpendPlayerManaPoints();
                SpendPlayerUnitOpportunityPoints();
                GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitProfilePanel.GetComponent<PlayerProfileController>().UpdateActivePlayerProfile(GridManager.Instance.currentPlayerUnit.GetComponent<Unit>());
                //Updates the Active Player Unit Profile Panel
                currentTargetedUnit.GetComponent<Unit>().unitProfilePanel.GetComponent<PlayerProfileController>().UpdateTargetedUnitProfile(currentTargetedUnit.GetComponent<Unit>());
                //Updates the Enemy Unit Profile Panel
                OnCastedSpellTextNotification(GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.name);
                Debug.Log("Using Spell on Enemy");
                Deity unboundDeity = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().deity;
                if (unboundDeity != null)
                {
                    //Looks for the Unbound Deity on the Battlefield
                    SpellAlignment spellAlignment = GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.alignment;
                    //Checks if the alignment of the casted spell is between the list of the Deity's Hated Spell Alignments
                    if (unboundDeity.hatedSpellAlignments.Contains(spellAlignment))
                    {
                        float enmityIncrease = GameObject.FindGameObjectWithTag("Deity").GetComponent<Deity>().enmity++;
                        OnCastedSpellTypeHatedbyDeity();
                        Debug.Log("Hated Alignment. Deity becomes angrier");
                    }
                    else
                    {
                        Debug.Log("Not Hated Alignment. Nothing happens to Deity");
                    }
                }
            }
            else
            {
                OnCastedSpellTextNotification("Not enough Mana Points");
            }
        }
        else
        {
            Debug.Log("Active Player Unit does not have enough Opportunity Points");
            OnCastedSpellTextNotification("Not enough Opportunity Points");
            //TileController targetUnitTileController = GridManager.Instance.GetTileControllerInstance(currentTargetedUnit.GetComponent<Unit>().currentXCoordinate, currentTargetedUnit.GetComponent<Unit>().currentYCoordinate);
            //targetUnitTileController.DeselectTile();
        }
    }

    public void SetAOESpellEpicenter(TileController epicenterSpellTarget)
    {
        aoeSpellEpicenterTile = epicenterSpellTarget;
    }
    public void UseCurrentSpellOnCurrentTargets()
    {
        foreach (var tile in GameObject.FindGameObjectWithTag("GridMovementController").GetComponent<GridMovementController>().GetMultipleTiles(aoeSpellEpicenterTile))
        {
            Debug.Log("Using AOE Spell on Multiple Targets");
            tile.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            if (tile.detectedUnit == null)
            {
                Debug.Log("No Unit found. Can't apply damage");
            }
            else if (tile.detectedUnit.tag == "Enemy")
            {
                tile.detectedUnit.GetComponent<Unit>().HealthPoints -= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.damage;
                Debug.Log("Applied damage on Enemy Units affected by the AOE Spell");
            }
        }
    }
    public bool CheckActivePlayerUnitOpportunityPoints()
    {
        if (GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitOpportunityPoints > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SpendPlayerManaPoints()
    {
        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitManaPoints -= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.manaPointsCost;
    }
    public void SpendPlayerUnitOpportunityPoints()
    {
        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitOpportunityPoints -= GameObject.FindGameObjectWithTag("SpellcastingController").GetComponent<SpellcastingController>().currentSelectedSpell.opportunityPointsCost;
    }
}
