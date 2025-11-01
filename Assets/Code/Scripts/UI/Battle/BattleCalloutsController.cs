using ProjectEdelweiss.Utils;
using System;
using UnityEngine;

public class BattleCalloutsController : MonoBehaviour
{
    public void OnEnable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit += ShowCriticalHitCallout;
        DeityKingLaurinusBehavior.OnUsedCursedGarden += ShowDeityAttackCallout;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit -= ShowCriticalHitCallout;
        DeityKingLaurinusBehavior.OnUsedCursedGarden -= ShowDeityAttackCallout;
    }

    public void ShowCriticalHitCallout()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit)?.GetComponent<Unit>();
        if (activePlayerUnit == null)
            return;

        if (activePlayerUnit.unitTemplate.unitCalloutPortrait == null)
            return;

        GameObject battleCalloutInstance = Instantiate(activePlayerUnit.unitTemplate.unitCalloutPortrait, this.gameObject.transform);
        battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(battleCalloutInstance, battleCalloutDuration);
        SFXHelper(activePlayerUnit);
    }

    public void ShowDeityAttackCallout(Unit deityUnit)
    {
        if (deityUnit == null)
            return;
        if (deityUnit.unitTemplate.unitCalloutPortrait == null)
            return;
        GameObject battleCalloutInstance = Instantiate(deityUnit.unitTemplate.unitCalloutPortrait, this.gameObject.transform);
        battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(battleCalloutInstance, battleCalloutDuration);
        SFXHelper(deityUnit);
    }

    private void SFXHelper(Unit unit)
    {
        // Add a dedicated SFX for the Deity in Inspector, currently using placeholder.
        if (unit.unitTemplate.unitCriticalHitVoice == null)
            return;
        GameObject criticalHitVoiceSFX = Instantiate(unit.unitTemplate.unitCriticalHitVoice, Camera.main.transform);
        Destroy(criticalHitVoiceSFX, unit.unitTemplate.unitCriticalHitVoice.GetComponent<AudioSource>().clip.length);
    }
}