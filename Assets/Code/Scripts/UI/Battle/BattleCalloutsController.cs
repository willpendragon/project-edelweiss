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
        DeityAnguanaSummoningBehavior.OnUsedFrozenPunishment += ShowAnguanaSummonCutin;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnSpellCriticalHit -= ShowCriticalHitCallout;
        AOESpellPlayerAction.OnDeityAngered -= ShowDeityAngeredCallout;
        DeityKingLaurinusBehavior.OnUsedCursedGarden -= ShowDeityAttackCallout;
        DeityAnguanaSummoningBehavior.OnUsedFrozenPunishment -= ShowAnguanaSummonCutin;
    }

    [SerializeField] Volume _globalVolume;
    [SerializeField] Canvas _calloutCanvas;

    private Sequence _activeCutinSequence;

    public void ShowCriticalHitCallout()
    {
        Unit activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit)?.GetComponent<Unit>();
        if (activePlayerUnit == null)
            return;
        if (activePlayerUnit.unitTemplate.unitCalloutPortrait == null)
            return;

        GameObject battleCalloutInstance =
            Instantiate(activePlayerUnit.unitTemplate.unitCalloutPortrait, _calloutCanvas.transform);
        battleCalloutInstance.GetComponentInChildren<Animator>().SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        float battleCalloutDuration = 1f;
        Destroy(battleCalloutInstance, battleCalloutDuration);
        DarkenScreen();
        SFXHelper(activePlayerUnit);
        // Play Critical Hit SFX
        BattleSFXManager.PlaySound(SoundType.CRITICALHIT);
    }

    public void ShowDeityAttackCallout(Unit deityUnit)
    {
        if (deityUnit == null)
            return;
        if (deityUnit.unitTemplate.unitCalloutPortrait == null)
            return;

        GameObject battleCalloutInstance =
            Instantiate(deityUnit.unitTemplate.unitCalloutPortrait, _calloutCanvas.transform);
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

    private void ShowAnguanaSummonCutin(DeityCutinConfig config, System.Action onMoveComplete)
    {
        // Kill any active cutin to prevent overlaps
        if (_activeCutinSequence != null && _activeCutinSequence.IsActive())
        {
            _activeCutinSequence.Kill();
            _activeCutinSequence = null;
        }

        _activeCutinSequence = ShowDeitySummonCutin(config, onMoveComplete);
    }

    public Sequence ShowDeitySummonCutin(DeityCutinConfig config, System.Action onComplete)
    {
        if (config == null)
        {
            Debug.LogWarning("DeityCutinConfig is null, executing action immediately.");
            onComplete?.Invoke();
            return null;
        }

        if (config.CutinPrefab == null)
        {
            Debug.LogWarning("Cutin prefab is null in config, executing action immediately.");
            onComplete?.Invoke();
            return null;
        }

        // Instantiate cutin prefab
        GameObject cutinInstance = Instantiate(config.CutinPrefab, _calloutCanvas.transform);
        RectTransform cutinRect = cutinInstance.GetComponent<RectTransform>();

        if (cutinRect == null)
        {
            Debug.LogError("Cutin prefab doesn't have a RectTransform component!");
            Destroy(cutinInstance);
            onComplete?.Invoke();
            return null;
        }

        // Calculate positions for slide animation
        float canvasWidth = _calloutCanvas.GetComponent<RectTransform>().rect.width;
        float cutinWidth = cutinRect.rect.width;
        Vector2 startPosition = new Vector2(canvasWidth / 2 + cutinWidth, 0); // Off-screen right
        Vector2 centerPosition = Vector2.zero; // Center of screen
        Vector2 endPosition = new Vector2(-canvasWidth / 2 - cutinWidth, 0); // Off-screen left

        // Set initial position
        cutinRect.anchoredPosition = startPosition;

        // Create DOTween sequence
        Sequence cutinSequence = DOTween.Sequence();

        // Slide in from right
        cutinSequence.Append(cutinRect.DOAnchorPos(centerPosition, config.SlideInDuration)
            .SetEase(config.SlideInEase));

        // Hold at center
        cutinSequence.AppendInterval(config.HoldDuration);

        // Slide out to left
        cutinSequence.Append(cutinRect.DOAnchorPos(endPosition, config.SlideOutDuration)
            .SetEase(config.SlideOutEase));

        // Destroy cutin GameObject
        cutinSequence.AppendCallback(() =>
        {
            if (cutinInstance != null)
                Destroy(cutinInstance);
        });

        // Delay before action
        cutinSequence.AppendInterval(config.DelayBeforeAction);

        // Execute the actual move
        cutinSequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            _activeCutinSequence = null;
        });

        // Optional: Apply screen darkening effect
        if (config.ShouldDarkenScreen)
        {
            DarkenScreen();
        }

        // Optional: Trigger animator if present
        Animator cutinAnimator = cutinInstance.GetComponentInChildren<Animator>();
        if (cutinAnimator != null)
        {
            cutinAnimator.SetTrigger(GameTags.SHOW_UNIT_CALLOUT);
        }

        return cutinSequence;
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