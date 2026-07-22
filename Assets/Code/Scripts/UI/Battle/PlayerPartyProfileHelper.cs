using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using ProjectEdelweiss.Utils;


public class PlayerPartyProfileHelper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image unitPortrait;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Outline _outline;

    [SerializeField] private Slider mpSlider;
    [SerializeField] private Slider fpSlider;
    [SerializeField] private Slider _deityMoveSlider;
    private int _deityMoveCooldown;
    private int _currentCooldownTimer = 0;

    //[SerializeField] private Slider SPSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI fpText;
    [SerializeField] private TextMeshProUGUI availableMovesText;
    [SerializeField] private CanvasGroup _partyProfileGroup;
    [SerializeField] private GameObject _deityMoveObj;
    [SerializeField] private GameObject _deityMoveButton;
    [SerializeField] private bool _isInOverworldMap;

    public event Action<Unit> OnProfileClicked;
    private Unit _linkedUnit;

    void Start()
    {
        _outline.enabled = false;
    }

    public void SetOutlineHighlight(bool isHighlighted)
    {
        _outline.enabled = isHighlighted;
    }

    public void Initialize(Unit unit)
    {
        _linkedUnit = unit;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_linkedUnit == null)
            return;

        OnProfileClicked?.Invoke(_linkedUnit);
    }

    public void FillPlayerDetails(Unit unit) // Avoid refreshing portrait and max values every single time.
    {
        if (unit == null)
            return;
        unitPortrait.sprite = unit.unitTemplate.unitBattlePortrait;

        availableMovesText.text = unit.unitOpportunityPoints.ToString();
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        mpText.text = ($"{unit.unitManaPoints}/{unit.unitMaxManaPoints}");
        if (fpText != null) // This logic applies only in the OverworldMap at the moment.
        {
            fpText.text = ($"{unit.unitFaithPoints}/{unit.unitFaithPoints}");
        }

        Debug.Log("Filling Player Details");
        UpdateSliders(unit);
        AddDeityIcon(unit);
    }

    private void AddDeityIcon(Unit unit)
    {
        _deityMoveObj.SetActive(false);
        if (unit.linkedDeity == null)
            return;
        _deityMoveObj.SetActive(true);
        DeityPowerController _deityPowerController = BattleManager.Instance?.DeityPowerController;            
        _deityMoveObj.GetComponentInChildren<Image>().sprite = unit.linkedDeity.deityPortrait;

        var scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        if (sceneName == GameTags.OVERWORLD_MAP)
        {
            _deityMoveSlider.gameObject.SetActive(false);
            _deityMoveButton.gameObject.SetActive(false);
            return;
        }

        // Clear old listeners before adding the new one
        Button deityButton = _deityMoveObj.GetComponentInChildren<Button>();
        deityButton.onClick.RemoveAllListeners();
        deityButton.onClick.AddListener(() => _deityPowerController.UseDeityMove());
        // This logic applies only to Anguana, needs to be extended to all of the other Deities.
        if (unit.linkedDeity.summoningBehaviour is DeityAnguanaSummoningBehavior anguanaBehavior)
        {
            _deityMoveSlider.maxValue = anguanaBehavior.moveCooldown;
            _deityMoveSlider.value = anguanaBehavior.moveCooldown;
            _deityMoveCooldown = anguanaBehavior.moveCooldown;
        }

        _partyProfileGroup.interactable = true;
        _partyProfileGroup.blocksRaycasts = true;
    }

    public bool IsDeityMoveReady()
    {
        return _currentCooldownTimer <= 0;
    }

    public void StartCooldown()
    {
        _currentCooldownTimer = _deityMoveCooldown;
        UpdateDeityMoveSlider();
    }

    public void UpdateDeityMoveSlider()
    {
        if (_deityMoveSlider != null)
            _deityMoveSlider.value = _deityMoveCooldown - _currentCooldownTimer;
    }

    public void FillCountdown()
    {
        if (_currentCooldownTimer > 0)
        {
            _currentCooldownTimer--;
            UpdateDeityMoveSlider();
        }
    }

    private void UpdateSliders(Unit unit)
    {
        hpSlider.maxValue = unit.unitTemplate.unitMaxHealthPoints;
        mpSlider.maxValue = unit.unitTemplate.unitManaPoints;
        hpSlider.value = unit.unitHealthPoints;
        mpSlider.value = unit.unitManaPoints;
        if (_isInOverworldMap)
        {
            fpSlider.maxValue = unit.unitTemplate.unitFaithPoints;
            fpSlider.value = unit.unitFaithPoints;
        }
    }


    public void UpdateRemainingMovesDisplay(Unit unit)
    {
        if (_isInOverworldMap == true)
            return;
        // Prevents refresh of the Remaining Moves display in the OverworldMap scene.
        RectTransform rt = availableMovesText.rectTransform;

        rt.DOKill();
        availableMovesText.DOKill();
        rt.localScale = Vector3.one;

        // Pulse and shake.
        Sequence seq = DOTween.Sequence();

        seq.Append(rt.DOScale(1.25f, 0.12f).SetEase(Ease.OutQuad)) // pulse up
            .Append(rt.DOScale(1f, 0.15f).SetEase(Ease.InQuad)) // return
            .Join(rt.DOShakePosition(0.20f, strength: 5f, vibrato: 15, randomness: 50, snapping: false));
        // Small shake.


        if (unit.unitOpportunityPoints <= 0 || unit.currentUnitLifeCondition == Unit.UnitLifeCondition.unitDead)
        {
            SetProfileAlpha(0.16f);
            availableMovesText.text = $"{unit.unitOpportunityPoints}/{unit.unitTemplate.unitOpportunityPoints}";
        }
        else
        {
            // Update text.
            availableMovesText.text = $"{unit.unitOpportunityPoints}/{unit.unitTemplate.unitOpportunityPoints}";
            SetProfileAlpha(1f);
        }

        Debug.Log("Update remaining moves details");
    }

    public void DeactivateProfile()
    {
        SetProfileAlpha(0.16f);
        availableMovesText.text = "-/-";
    }

    public void SetProfileAlpha(float alpha)
    {
        _partyProfileGroup.alpha = alpha;
    }

    public void UpdateHP(Unit unit)
    {
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        hpSlider.value = unit.unitHealthPoints;
    }

    public void CollectUpgradeFeedback()
    {
        Debug.Log("Displaying collect upgrade feedback");
    }

    public void ResetAvailableMovesCounter()
    {
        availableMovesText.text = $"-/-";
    }
}