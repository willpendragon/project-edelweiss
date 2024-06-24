using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AltarPlayerUnitProfileController : MonoBehaviour
{

    [Header("Stats Details")]
    [SerializeField] Slider playerUnitHPSlider;
    [SerializeField] Slider playerUnitAttackPowerSlider;
    [SerializeField] Slider playerUnitMagicPowerSlider;

    [Header("Playable Character Details")]
    [SerializeField] TextMeshProUGUI playerUnitName;
    [SerializeField] TextMeshProUGUI linkedDeityName;
    [SerializeField] Image playerUnitMiniPortrait;

    public void PopulatePlayerUnitProfile(Unit playerUnit)
    {
        playerUnitName.text = playerUnit.unitTemplate.unitName;
        playerUnitHPSlider.maxValue = playerUnit.unitTemplate.unitMaxHealthPoints;
        playerUnitAttackPowerSlider.maxValue = playerUnit.unitTemplate.meleeAttackPower;

        playerUnitMiniPortrait.sprite = playerUnit.unitTemplate.unitMiniPortrait;
        if (playerUnit.linkedDeity != null)
        {
            linkedDeityName.text = playerUnit.linkedDeity.GetComponent<Unit>().unitTemplate.unitName.ToString();
        }
    }
}
