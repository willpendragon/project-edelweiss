using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileController : MonoBehaviour
{
    public enum ProfileOwner
    {
        empty,
        enemyUnit,
        activePlayerUnit,
        playerUnit
    }

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

    [Header("Slider UI Elements")]

    public Slider activeCharacterHealthPointsSlider;
    public Slider activeCharacterManaPointsSlider;
    public Slider activeCharacterShieldPointsSlider;
    public Slider activeCharacterOpportunityPointsSlider;

    public ProfileOwner currentProfileOwner;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;

    private void OnEnable()
    {
        SelectUnitPlayerAction.OnClickedTileWithUnit += UpdateUnitProfile;
        EnemyInfoPanelController.OnHoverMouseOnEnemy += UpdateUnitProfile;
    }
    private void OnDisable()
    {
        SelectUnitPlayerAction.OnClickedTileWithUnit -= UpdateUnitProfile;
        EnemyInfoPanelController.OnHoverMouseOnEnemy -= UpdateUnitProfile;
    }
    public void UpdateUnitProfile(GameObject detectedUnit)
    {
        switch (detectedUnit.tag)
        {
            case "Enemy":
                if (currentProfileOwner != ProfileOwner.playerUnit && currentProfileOwner != ProfileOwner.activePlayerUnit)
                {
                    ApplyProfileChanges(detectedUnit, ProfileOwner.enemyUnit);
                    Debug.Log("Updated Unit Profile with Enemy Info");

                }
                break;
            case "Player":
                if (currentProfileOwner != ProfileOwner.enemyUnit && GridManager.Instance.currentPlayerUnit == null)
                {
                    ApplyProfileChanges(detectedUnit, ProfileOwner.playerUnit);
                    UpdateLinkedDeityIcon(detectedUnit);
                    Debug.Log("Updated Unit Profile with Player Info");

                }
                break;
            case "ActivePlayerUnit":
            case "TargetedEnemyUnitProfile":
                if (currentProfileOwner != ProfileOwner.enemyUnit)
                {
                    ApplyProfileChanges(detectedUnit, ProfileOwner.activePlayerUnit);
                    UpdateLinkedDeityIcon(detectedUnit);
                    Debug.Log("Updated Active Player Unit Profile");
                }
                break;
            default:
                Debug.Log("Unknown unit tag: " + detectedUnit.tag);
                break;
        }
    }
    public void ApplyProfileChanges(GameObject detectedUnit, ProfileOwner profileOwner)
    {
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

        // Update Profile Owner.
        currentProfileOwner = profileOwner;

        Debug.Log("Updated" + detectedUnit + "Profile" + "with Current Info");

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