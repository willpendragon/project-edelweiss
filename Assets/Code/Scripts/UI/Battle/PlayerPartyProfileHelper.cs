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
    //[SerializeField] private Slider SPSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI availableMovesText;
    [SerializeField] private CanvasGroup _partyProfileGroup;

    public void FillPlayerDetails(Unit unit) // Avoid refreshing portrait and max values every single time.
    {
        if (unit == null)
            return;
        unitPortrait.sprite = unit.unitTemplate.unitMiniPortrait;

        availableMovesText.text = unit.unitOpportunityPoints.ToString();
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        mpText.text = ($"{unit.unitManaPoints}/{unit.unitMaxManaPoints}");
        Debug.Log("Filling Player Details");
        UpdateSliders(unit);
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

        seq.Append(rt.DOScale(1.25f, 0.12f).SetEase(Ease.OutQuad))   // pulse up
           .Append(rt.DOScale(1f, 0.15f).SetEase(Ease.InQuad))       // return
           .Join(rt.DOShakePosition(0.20f, strength: 5f, vibrato: 15, randomness: 50, snapping: false));
        // Small shake.

        // Update text.
        availableMovesText.text = $"{unit.unitOpportunityPoints}/{unit.unitTemplate.unitOpportunityPoints}";

        if (unit.unitOpportunityPoints <= 0)
        {
            SetProfileAlpha(0.16f);
        }
        else
        {
            SetProfileAlpha(1f);
        }
        Debug.Log("Update remaining moves details");
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
}
