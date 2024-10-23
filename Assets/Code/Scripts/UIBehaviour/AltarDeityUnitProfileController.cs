using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AltarDeityUnitProfileController : MonoBehaviour
{

    [Header("Deity Character Details")]
    [SerializeField] TextMeshProUGUI deityName;
    [SerializeField] Image deityUnitPortrait;
    [SerializeField] Image linkedUnitPortrait;

    [Header("Deity Character UI")]
    [SerializeField] TextMeshProUGUI buffType;
    [SerializeField] TextMeshProUGUI buffAmountCounter;
    [SerializeField] Slider buffAmountSlider;
    [SerializeField] Button selectDeityButton;

    private Deity selectedDeity;

    public void PopulateDeityUnitProfile(Unit deityUnit, Deity deity)
    {
        AnguanaHealingBehavior healingBehavior = deity.summoningBehaviour as AnguanaHealingBehavior;


        deityName.text = deityUnit.unitTemplate.unitName;

        // Sets the parameter of the Deity Prayer Buff on the menu

        buffAmountSlider.maxValue = healingBehavior.bubbleBuffShieldPointsIncreaseAmount;
        buffAmountSlider.value = healingBehavior.bubbleBuffShieldPointsIncreaseAmount;
        buffType.text = healingBehavior.deityBuffName;

        buffAmountCounter.text = healingBehavior.bubbleBuffShieldPointsIncreaseAmount.ToString();

        //if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.AttackPower)
        //{
        //    buffType.text = "Attack Buff";
        //}
        //else if (deity.deityPrayerBuff.currentAffectedStat == DeityPrayerBuff.AffectedStat.MagicPower)
        //{
        //    buffType.text = "Magic Buff";
        //}

        deityUnitPortrait.sprite = deityUnit.unitTemplate.unitPortrait;
        linkedUnitPortrait.sprite = RetrieveLinkedUnitSmallPortrait(deity);
        selectedDeity = deity;
        selectDeityButton.onClick.AddListener(SelectDeityUnit);
    }

    public void SelectDeityUnit()
    {
        Debug.Log("SelectedDeityUnit");
        DeityAltarController deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
        deityAltarController.AssignDeityToUnit(selectedDeity);
        linkedUnitPortrait.sprite = RetrieveLinkedUnitSmallPortrait(selectedDeity);
    }

    public Sprite RetrieveLinkedUnitSmallPortrait(Deity deity)
    {
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            if (playerUnit.LinkedDeityId == deity.Id)
            {
                Debug.Log("Found Linked Unit");
                return playerUnit.GetComponent<Unit>().unitTemplate.unitMiniPortrait;
            }
        }
        Debug.Log("No Linked Unit Found");

        return null;
    }
}
