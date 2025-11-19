using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
        hpSlider.maxValue = unit.unitMaxHealthPoints;
        mpSlider.maxValue = unit.unitMaxManaPoints;
        availableMovesText.text = unit.unitOpportunityPoints.ToString();
        hpText.text = ($"{unit.unitHealthPoints}/{unit.unitMaxHealthPoints}");
        mpText.text = ($"{unit.unitManaPoints}/{unit.unitMaxManaPoints}");
        Debug.Log("Filling Player Details");
    }

    public void UpdateRemainingMovesDisplay(Unit unit)
    {
        availableMovesText.text = ($"{unit.unitOpportunityPoints}/{unit.unitTemplate.unitOpportunityPoints}");
        Debug.Log("Update remaining moves details");
    }
}
