using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerPartyProfileHelper : MonoBehaviour
{
    [SerializeField] private Image unitPortrait;
    [SerializeField] private Slider hpSlider;

    [SerializeField] private Slider mpSlider;
    [SerializeField] private Slider _deityMoveSlider;
    private int _deityMoveCooldown;
    private int _currentCooldownTimer = 0;

    //[SerializeField] private Slider SPSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI availableMovesText;
    [SerializeField] private CanvasGroup _partyProfileGroup;
    [SerializeField] private GameObject _deityMoveObj;

    public void FillPlayerDetails(Unit unit) // Avoid refreshing portrait and max values every single time.
    {
        if (unit == null)
            return;
        unitPortrait.sprite = unit.unitTemplate.unitBattlePortrait;

        availableMovesText.text = unit.unitOpportunityPoints.ToString();
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        mpText.text = ($"{unit.unitManaPoints}/{unit.unitMaxManaPoints}");
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
        DeityPowerController _deityPowerController = BattleManager.Instance.DeityPowerController;
        _deityMoveObj.GetComponentInChildren<Image>().sprite = unit.linkedDeity.deityPortrait;
        _deityMoveObj.GetComponentInChildren<Button>().onClick.AddListener(() => _deityPowerController.UseDeityMove());
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
    }


    public void UpdateRemainingMovesDisplay(Unit unit)
    {
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
}