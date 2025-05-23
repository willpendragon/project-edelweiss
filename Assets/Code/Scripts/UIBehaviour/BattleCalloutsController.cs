using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCalloutsController : MonoBehaviour
{
    public void OnEnable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit += ShowCriticalHitCallout;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit -= ShowCriticalHitCallout;
    }

    public void ShowCriticalHitCallout()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit")?.GetComponent<Unit>();
        if (activePlayerUnit != null)
        {
            GameObject battleCalloutInstance = Instantiate(activePlayerUnit.unitTemplate.unitCalloutPortrait, this.gameObject.transform);
            battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger("ShowUnitCallout");

            GameObject criticalHitVoiceSFX = Instantiate(activePlayerUnit.unitTemplate.unitCriticalHitVoice, Camera.main.transform);

            float battleCalloutDuration = 1f;

            Destroy(battleCalloutInstance, battleCalloutDuration);
            Destroy(battleCalloutInstance, activePlayerUnit.unitTemplate.unitCriticalHitVoice.GetComponent<AudioSource>().clip.length);
        }
    }
}
