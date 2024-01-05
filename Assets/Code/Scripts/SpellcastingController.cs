using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellcastingController : MonoBehaviour
{
    public delegate void CastingSpell();
    public static event CastingSpell OnCastingSpell;
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
        currentTargetedUnit = targetedUnit;
    }
    public void UseCurrentSpellOnCurrentTarget()
    {
        //Spell damage calculation Logic
        currentTargetedUnit.unitHealth -= currentSelectedSpell.damage;
        currentTargetedUnit.gameObject.GetComponent<Enemy>().EnemyTakingDamage.Invoke();
    }
}
