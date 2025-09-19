using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitProfileController : MonoBehaviour
{
    [Header("Gameplay Logic")]
    public GameObject activeCharacterPortrait;

    [Header("Text Mesh Pro Elements - Visual")]
    public TextMeshProUGUI activeCharacterName;

    [Header("Visual Elements")]
    public GameObject linkedDeityMiniPortrait;

    [Header("Text Mesh Pro Elements - Gameplay Stats")]
    public TextMeshProUGUI activeCharacterHealthPoints;
    public TextMeshProUGUI activeCharacterManaPoints;
    public TextMeshProUGUI activeCharacterShieldPoints;
    public TextMeshProUGUI activeCharacterOpportunityPoints;

    [Header("Text Mesh Pro Elements - Character Stats")]
    public TextMeshProUGUI activeCharacterAttackPower;
    public TextMeshProUGUI activeCharacterMagicPower;
    public TextMeshProUGUI activeCharacterFaithPower;

    [Header("Slider UI Elements")]

    public Slider activeCharacterHealthPointsSlider;
    public Slider activeCharacterManaPointsSlider;
    public Slider activeCharacterShieldPointsSlider;
    public Slider activeCharacterOpportunityPointsSlider;

    private void OnEnable()
    {
        EnemyInfoPanelController.OnHoverMouseOnEnemy += ApplyProfileChanges;

    }
    private void OnDisable()
    {
        EnemyInfoPanelController.OnHoverMouseOnEnemy -= ApplyProfileChanges;
    }
    public void ApplyProfileChanges(GameObject detectedUnit)
    {
        if (detectedUnit == null)
            return;
        // Update Unit Profile Portrait and Name.
        activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
        activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;

        // Update numeric gameplay values.
        activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
        activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
        activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
        activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();

        //Update numeric Unit stats.
        activeCharacterAttackPower.text = detectedUnit.GetComponent<Unit>().unitAttackPower.ToString();
        activeCharacterMagicPower.text = detectedUnit.GetComponent<Unit>().unitMagicPower.ToString();

        if (detectedUnit.tag == "ActivePlayerUnit")
        {
            activeCharacterFaithPower.text = detectedUnit.GetComponent<Unit>().unitFaithPoints.ToString();
        }

        // Update Stats Slider's Max Value.
        activeCharacterHealthPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitTemplate.unitHealthPoints;
        activeCharacterManaPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitTemplate.unitManaPoints;
        activeCharacterShieldPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitTemplate.unitShieldPoints;
        activeCharacterOpportunityPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitTemplate.unitOpportunityPoints;

        // Update Stats Slider's Value.
        activeCharacterHealthPointsSlider.value = detectedUnit.GetComponent<Unit>().unitHealthPoints;
        activeCharacterManaPointsSlider.value = detectedUnit.GetComponent<Unit>().unitManaPoints;
        activeCharacterShieldPointsSlider.value = detectedUnit.GetComponent<Unit>().unitShieldPoints;
        activeCharacterOpportunityPointsSlider.value = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
        Debug.Log($"Updated {detectedUnit} profile");
    }

    public void UpdateActivePlayerProfile(Unit activePlayerUnit)
    {
        activeCharacterManaPoints.text = activePlayerUnit.unitManaPoints.ToString();
        activeCharacterOpportunityPoints.text = activePlayerUnit.unitOpportunityPoints.ToString();
        activeCharacterManaPointsSlider.value = activePlayerUnit.unitManaPoints;
        activeCharacterOpportunityPointsSlider.value = activePlayerUnit.unitOpportunityPoints;
    }
    public void UpdateTargetedUnitProfile(Unit targetedUnit)
    {
        activeCharacterHealthPoints.text = targetedUnit.unitHealthPoints.ToString();
        activeCharacterHealthPointsSlider.value = targetedUnit.unitHealthPoints;
    }

    public void UpdateLinkedDeityIcon(GameObject unitLinkedToDeity)
    {
        if (unitLinkedToDeity.GetComponent<Unit>().linkedDeity != null)
        {
            Debug.Log("Trying to add Linked Deity Mini Portrait to Active Player Unit profile");
            linkedDeityMiniPortrait.GetComponent<Image>().overrideSprite = unitLinkedToDeity.GetComponent<Unit>().linkedDeity.GetComponent<Unit>().unitTemplate.unitMiniPortrait;
            Debug.Log("Added Linked Deity Mini Portrait to Active Player Unit profile");
        }
    }
}