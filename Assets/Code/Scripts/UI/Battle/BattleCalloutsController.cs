using DG.Tweening;
using ProjectEdelweiss.Utils;
using UnityEngine;
using UnityEngine.Rendering;

public class BattleCalloutsController : MonoBehaviour
{
    [SerializeField] private GameObject _angeredDeityCallout;
    public void OnEnable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit += ShowCriticalHitCallout;
        AOESpellPlayerAction.OnDeityAngered += ShowDeityAngeredCallout;
        DeityKingLaurinusBehavior.OnUsedCursedGarden += ShowDeityAttackCallout;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit -= ShowCriticalHitCallout;
        AOESpellPlayerAction.OnDeityAngered -= ShowDeityAngeredCallout;
        DeityKingLaurinusBehavior.OnUsedCursedGarden -= ShowDeityAttackCallout;
    }

    [SerializeField] Volume _globalVolume;
    [SerializeField] Canvas _calloutCanvas;

    public void ShowCriticalHitCallout()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit)?.GetComponent<Unit>();
        if (activePlayerUnit == null)
            return;
        if (activePlayerUnit.unitTemplate.unitCalloutPortrait == null)
            return;

        GameObject battleCalloutInstance = Instantiate(activePlayerUnit.unitTemplate.unitCalloutPortrait, _calloutCanvas.transform);
        battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(battleCalloutInstance, battleCalloutDuration);
        DarkenScreen();
        SFXHelper(activePlayerUnit);
    }

    public void ShowDeityAttackCallout(Unit deityUnit)
    {
        if (deityUnit == null)
            return;
        if (deityUnit.unitTemplate.unitCalloutPortrait == null)
            return;

        GameObject battleCalloutInstance = Instantiate(deityUnit.unitTemplate.unitCalloutPortrait, _calloutCanvas.transform);
        battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(battleCalloutInstance, battleCalloutDuration);
        DarkenScreen();
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

    public void ShowDeityAngeredCallout()
    {
        if (_angeredDeityCallout == null)
            return;

        GameObject angeredDeityCalloutInstance = Instantiate(_angeredDeityCallout, _calloutCanvas.transform);
        angeredDeityCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(angeredDeityCalloutInstance, battleCalloutDuration);
        DarkenScreen();
    }

    private void DarkenScreen()
    {
        Sequence volumeSequence = DOTween.Sequence();

        volumeSequence.Append(DOTween.To(
            () => _globalVolume.weight,
            x => _globalVolume.weight = x,
            0.1f,
            0.5f
        ).SetEase(Ease.InOutSine));

        volumeSequence.Append(DOTween.To(
            () => _globalVolume.weight,
            x => _globalVolume.weight = x,
            0f,
            0.5f
        ).SetEase(Ease.InOutSine));
    }
}