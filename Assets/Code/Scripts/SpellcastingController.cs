using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellcastingController : MonoBehaviour
{
    public delegate void CastingSpell();
    public static event CastingSpell OnCastingSpell;

    public delegate void CastedSpell();
    public static event CastedSpell OnCastedSpell;

    public delegate void CastedSpellTextNotification(string spellName);
    public static event CastedSpellTextNotification OnCastedSpellTextNotification;


    public Unit currentTargetedUnit;
    public Spell currentSelectedSpell;
    public void OnEnable()
    {
        GridTargetingController.OnTargetedUnit += SetTargetedUnit;
        TileController.OnTileConfirmedAttackMode += UseCurrentSpellOnCurrentTarget;
    }
    public void OnDisable()
    {
        GridTargetingController.OnTargetedUnit -= SetTargetedUnit;
        TileController.OnTileConfirmedAttackMode -= UseCurrentSpellOnCurrentTarget;

    }
    public void CastSpell(Spell castedSpell)
    {
        //Sends an event delegate that activates the Attack Selection Mode on the Grid Targeting Controller.
        //Sets the current Target Enemy and the current Spell
        OnCastingSpell();
        currentSelectedSpell = castedSpell;
        Debug.Log("Now Casting" + castedSpell);
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
            //Spell damage calculation logic
            currentTargetedUnit.HealthPoints -= GridManager.Instance.currentPlayerUnit.GetComponent<SpellcastingController>().currentSelectedSpell.damage;
            currentTargetedUnit.gameObject.GetComponent<EnemyAgent>().EnemyTakingDamage.Invoke();
            SpendPlayerManaPoints();
            SpendPlayerUnitOpportunityPoints();
            OnCastedSpell();
            OnCastedSpellTextNotification(GridManager.Instance.currentPlayerUnit.GetComponent<SpellcastingController>().currentSelectedSpell.name);
            Debug.Log("Using Spell on Enemy");
        }
        else
        {
            Debug.Log("Active Player Unit does not have enough Opportunity Points");
            //TileController targetUnitTileController = GridManager.Instance.GetTileControllerInstance(currentTargetedUnit.GetComponent<Unit>().currentXCoordinate, currentTargetedUnit.GetComponent<Unit>().currentYCoordinate);
            //targetUnitTileController.DeselectTile();
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
        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitManaPoints -= GridManager.Instance.currentPlayerUnit.GetComponent<SpellcastingController>().currentSelectedSpell.manaPointsCost;
    }
    public void SpendPlayerUnitOpportunityPoints()
    {
        GridManager.Instance.currentPlayerUnit.GetComponent<Unit>().unitOpportunityPoints -= GridManager.Instance.currentPlayerUnit.GetComponent<SpellcastingController>().currentSelectedSpell.opportunityPointsCost;
    }
}
