using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPartyProfileHelper : MonoBehaviour
{
    [SerializeField] private Image unitPortrait;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;
    //[SerializeField] private Slider SPSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI availableMovesText;

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
        availableMovesText.text = ($"{unit.unitOpportunityPoints}/{unit.unitTemplate.unitOpportunityPoints}");
        Debug.Log("Update remaining moves details");
    }

    public void UpdateHP(Unit unit)
    {
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        hpSlider.value = unit.unitHealthPoints;
    }
}
