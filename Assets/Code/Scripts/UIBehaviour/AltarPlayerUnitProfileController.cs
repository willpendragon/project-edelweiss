using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AltarPlayerUnitProfileController : MonoBehaviour
{

    [Header("Stats Details Slider")]
    [SerializeField] Slider playerUnitHPSlider;
    [SerializeField] Slider playerUnitAttackPowerSlider;
    [SerializeField] Slider playerUnitMagicPowerSlider;

    [Header("Stats Details Texts")]

    [SerializeField] TextMeshProUGUI playerUnitHPTextValue;
    [SerializeField] TextMeshProUGUI playerUnitAttackPowerTextValue;
    [SerializeField] TextMeshProUGUI playerUnitMagicPowerTextValue;


    [Header("Playable Character Details")]
    [SerializeField] TextMeshProUGUI playerUnitName;
    public TextMeshProUGUI linkedDeityName;
    [SerializeField] Image playerUnitMiniPortrait;

    [SerializeField] Button selectPlayerButton;

    private Unit selectedPlayerUnit;

    public void PopulatePlayerUnitProfile(Unit playerUnit)
    {
        playerUnitName.text = playerUnit.unitTemplate.unitName;

        playerUnitHPSlider.maxValue = playerUnit.unitTemplate.unitMaxHealthPoints;
        playerUnitAttackPowerSlider.maxValue = playerUnit.unitTemplate.meleeAttackPower;
        playerUnitMagicPowerSlider.maxValue = playerUnit.unitTemplate.unitMagicPower;

        playerUnitHPSlider.value = playerUnit.unitTemplate.unitMaxHealthPoints;
        playerUnitAttackPowerSlider.value = playerUnit.unitTemplate.meleeAttackPower;
        playerUnitMagicPowerSlider.value = playerUnit.unitTemplate.unitMagicPower;


        playerUnitMiniPortrait.sprite = playerUnit.unitTemplate.unitMiniPortrait;
        if (playerUnit.linkedDeity != null)
        {
            linkedDeityName.text = playerUnit.linkedDeity.GetComponent<Unit>().unitTemplate.unitName.ToString();
        }

        selectedPlayerUnit = playerUnit;

        selectPlayerButton.onClick.AddListener(SelectPlayerUnit);
    }

    public void SelectPlayerUnit()
    {
        Debug.Log("SelectedPlayerUnit");
        DeityAltarController deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
        deityAltarController.SetCurrentSelectedUnit(selectedPlayerUnit, this.gameObject);
    }
}
