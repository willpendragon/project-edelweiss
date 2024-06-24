using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarDeityUnitProfileController : MonoBehaviour
{

    [Header("Deity Character Details")]
    [SerializeField] TextMeshProUGUI deityName;
    [SerializeField] Image deityUnitPortrait;

    [SerializeField] Slider buffAmountSlider;
    [SerializeField] TextMeshProUGUI buffType;

    public void PopulatePlayerUnitProfile(Unit deityUnit, Deity deity)
    {
        deityName.text = deityUnit.unitTemplate.unitName;
        buffAmountSlider.maxValue = deity.deityPrayerBuff.buffAmount;
        buffType.text = deity.deityPrayerBuff.currentAffectedStat.ToString();

        deityUnitPortrait.sprite = deityUnit.unitTemplate.unitPortrait;
    }
}
