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

    public delegate void ClickedOnSpellButton(Spell clickedSpell);
    public static event ClickedOnSpellButton OnClickedOnSpellButton;

    public DistanceController distanceController;
    public void CastSpell(Spell castedSpell)
    {
        // Sends an event delegate that activates the Attack Selection Mode on the Grid Targeting Controller.
        // Sets the current Target Enemy and the current Spell
        if (castedSpell != null && castedSpell.spellType == SpellType.SingleTarget)
        {
            OnCastingSpell();
            currentSelectedSpell = castedSpell;
            Debug.Log("Single Target - Now Casting" + castedSpell);
        }
        else if (castedSpell != null && castedSpell.spellType == SpellType.AOE)
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

    public void SetCurrentSpell(Spell currentSpell)
    {
        currentSelectedSpell = currentSpell;
        OnClickedOnSpellButton(currentSelectedSpell);
    }
}
