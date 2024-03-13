using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    public GameObject activeCharacterPortrait;
    public TextMeshProUGUI activeCharacterName;
    public TextMeshProUGUI activeCharacterHealthPoints;
    public TextMeshProUGUI activeCharacterManaPoints;
    public TextMeshProUGUI activeCharacterShieldPoints;
    public TextMeshProUGUI activeCharacterOpportunityPoints;
    public Slider activeCharacterHealthPointsSlider;
    public Slider activeCharacterManaPointsSlider;
    public Slider activeCharacterShieldPointsSlider;
    public Slider activeCharacterOpportunityPointsSlider;

    public ProfileOwner currentProfileOwner;

    public delegate void ClickedTileWithUnit(GameObject detectedUnit);
    public static event ClickedTileWithUnit OnClickedTileWithUnit;

    public void OnEnable()
    {
        //TileController.OnClickedTileWithUnit += UpdateUnitProfile;
        //TileController.OnUpdateEnemyTargetUnitProfile += UpdateUnitProfile;
        SelectUnitPlayerAction.OnClickedTileWithUnit += UpdateUnitProfile;


    }
    public void OnDisable()
    {
        SelectUnitPlayerAction.OnClickedTileWithUnit -= UpdateUnitProfile;

        //TileController.OnClickedTileWithUnit -= UpdateUnitProfile;
        //TileController.OnUpdateEnemyTargetUnitProfile -= UpdateUnitProfile;
    }

    private void Start()
    {
    }
    public void UpdateUnitProfile(GameObject detectedUnit)
    {
        if (detectedUnit.tag == "Enemy" && currentProfileOwner != ProfileOwner.playerUnit && currentProfileOwner != ProfileOwner.activePlayerUnit)
        {
            Debug.Log("Updating Unit Profile with Enemy Info");
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
            activeCharacterHealthPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterHealthPointsSlider.value = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.value = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.value = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.value = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            currentProfileOwner = ProfileOwner.enemyUnit;
        }
        else if (detectedUnit.tag == "Player" && currentProfileOwner != ProfileOwner.enemyUnit)
        {
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
            activeCharacterHealthPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterHealthPointsSlider.value = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.value = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.value = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.value = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            currentProfileOwner = ProfileOwner.playerUnit;
        }
        else if (detectedUnit.tag == "ActivePlayerUnit" && currentProfileOwner != ProfileOwner.enemyUnit)
        {
            Debug.Log("Updating Player Profile");
            activeCharacterHealthPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterHealthPointsSlider.value = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.value = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.value = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.value = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
            currentProfileOwner = ProfileOwner.activePlayerUnit;
        }

        else if (detectedUnit.tag == "TargetedEnemyUnitProfile" && currentProfileOwner != ProfileOwner.enemyUnit)
        {
            activeCharacterHealthPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.maxValue = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterHealthPointsSlider.value = detectedUnit.GetComponent<Unit>().unitHealthPoints;
            activeCharacterManaPointsSlider.value = detectedUnit.GetComponent<Unit>().unitManaPoints;
            activeCharacterShieldPointsSlider.value = detectedUnit.GetComponent<Unit>().unitShieldPoints;
            activeCharacterOpportunityPointsSlider.value = detectedUnit.GetComponent<Unit>().unitOpportunityPoints;
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
            currentProfileOwner = ProfileOwner.activePlayerUnit;
        }
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

}
