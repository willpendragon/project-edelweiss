using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarDeityUnitProfileController : MonoBehaviour
{

    [Header("Deity Character Details")]
    [SerializeField] TextMeshProUGUI deityName;
    [SerializeField] Image deityUnitPortrait;

    [Header("Deity Character UI")]
    [SerializeField] TextMeshProUGUI buffType;
    [SerializeField] Slider buffAmountSlider;
    [SerializeField] Button selectDeityButton;

    private Deity selectedDeity;

    public void PopulateDeityUnitProfile(Unit deityUnit, Deity deity)
    {
        deityName.text = deityUnit.unitTemplate.unitName;
        buffAmountSlider.maxValue = deity.deityPrayerBuff.buffAmount;
        buffType.text = deity.deityPrayerBuff.currentAffectedStat.ToString();

        deityUnitPortrait.sprite = deityUnit.unitTemplate.unitPortrait;

        selectedDeity = deity;
        selectDeityButton.onClick.AddListener(SelectDeityUnit);
    }

    public void SelectDeityUnit()
    {
        Debug.Log("SelectedDeityUnit");
        DeityAltarController deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
        deityAltarController.AssignDeityToUnit(selectedDeity);
    }
}
