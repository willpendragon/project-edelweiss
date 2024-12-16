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
    public string playerId;

    public void PopulatePlayerUnitProfile(Unit playerUnit)
    {
        playerUnitName.text = playerUnit.unitTemplate.unitName;

        if (playerUnitHPTextValue.text != null) playerUnitHPTextValue.text = playerUnit.unitHealthPoints.ToString();
        if (playerUnitAttackPowerTextValue.text != null) playerUnitAttackPowerTextValue.text = playerUnit.unitAttackPower.ToString();
        if (playerUnitMagicPowerTextValue.text != null) playerUnitMagicPowerTextValue.text = playerUnit.unitMagicPower.ToString();

        playerId = playerUnit.Id;

        if (playerUnitHPSlider != null) playerUnitHPSlider.maxValue = playerUnit.unitTemplate.unitMaxHealthPoints;
        if (playerUnitAttackPowerSlider != null) playerUnitAttackPowerSlider.maxValue = playerUnit.unitTemplate.meleeAttackPower;
        if (playerUnitMagicPowerSlider != null) playerUnitMagicPowerSlider.maxValue = playerUnit.unitTemplate.unitMagicPower;

        if (playerUnitHPSlider != null) playerUnitHPSlider.value = playerUnit.unitTemplate.unitMaxHealthPoints;
        if (playerUnitAttackPowerSlider != null) playerUnitAttackPowerSlider.value = playerUnit.unitTemplate.meleeAttackPower;
        if (playerUnitMagicPowerSlider != null) playerUnitMagicPowerSlider.value = playerUnit.unitTemplate.unitMagicPower;

        playerUnitMiniPortrait.sprite = playerUnit.unitTemplate.unitMiniPortrait;
        if (playerUnit.linkedDeity != null)
        {
            linkedDeityName.text = playerUnit.linkedDeity.GetComponent<Unit>().unitTemplate.unitName.ToString();
        }

        selectedPlayerUnit = playerUnit;

        selectPlayerButton.onClick.AddListener(SelectPlayerUnit);
    }

    public void UpdatePlayerUnitProfile(Unit playerUnit)
    {
        if (playerUnitHPTextValue.text != null) playerUnitHPTextValue.text = playerUnit.unitHealthPoints.ToString();
        if (playerUnitAttackPowerTextValue.text != null) playerUnitAttackPowerTextValue.text = playerUnit.unitAttackPower.ToString();
        if (playerUnitMagicPowerTextValue.text != null) playerUnitMagicPowerTextValue.text = playerUnit.unitMagicPower.ToString();

        if (playerUnitHPSlider != null) playerUnitHPSlider.maxValue = playerUnit.unitMaxHealthPoints;
        if (playerUnitAttackPowerSlider != null) playerUnitAttackPowerSlider.maxValue = playerUnit.unitAttackPower;
        if (playerUnitMagicPowerSlider != null) playerUnitMagicPowerSlider.maxValue = playerUnit.unitMagicPower;

        if (playerUnitHPSlider != null) playerUnitHPSlider.value = playerUnit.unitMaxHealthPoints;
        if (playerUnitAttackPowerSlider != null) playerUnitAttackPowerSlider.value = playerUnit.unitAttackPower;
        if (playerUnitMagicPowerSlider != null) playerUnitMagicPowerSlider.value = playerUnit.unitMagicPower;
    }

    public void SelectPlayerUnit()
    {
        Debug.Log("SelectedPlayerUnit");
        DeityAltarController deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
        deityAltarController.SetCurrentSelectedUnit(selectedPlayerUnit, this.gameObject);
    }
}
