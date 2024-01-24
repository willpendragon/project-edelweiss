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

    public void OnEnable()
    {
        //UnitSelectionController.OnActiveCharacterSelected += UpdatePlayerProfile;
        //UnitSelectionController.OnActiveCharacterSelected += InitializePlayerProfileSliders;
        //UnitSelectionController.OnActiveCharacterDeselected += ResetPlayerProfile;
        //SpellcastingController.OnCastedSpell += UpdatePlayerProfile;
        TileController.OnClickedTileWithUnit += UpdatePlayerProfile;
        TileController.OnDeselectedTileWithUnit += ResetPlayerProfile;
        SpellcastingController.OnCastedSpell += UpdatePlayerProfileSliders;
    }
    public void OnDisable()
    {
        //UnitSelectionController.OnActiveCharacterSelected -= UpdatePlayerProfile;
        //UnitSelectionController.OnActiveCharacterSelected -= InitializePlayerProfileSliders;
        //UnitSelectionController.OnActiveCharacterDeselected -= ResetPlayerProfile;
        //SpellcastingController.OnCastedSpell -= UpdatePlayerProfile;
        TileController.OnClickedTileWithUnit -= UpdatePlayerProfile;
        TileController.OnDeselectedTileWithUnit -= ResetPlayerProfile;
        SpellcastingController.OnCastedSpell -= UpdatePlayerProfileSliders;
    }
    public void UpdatePlayerProfile(GameObject detectedUnit)
    {
        if (detectedUnit.tag == "Enemy" && currentProfileOwner != ProfileOwner.playerUnit && currentProfileOwner != ProfileOwner.activePlayerUnit)
        {
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
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
            currentProfileOwner = ProfileOwner.playerUnit;

        }
        else if (detectedUnit.tag == "ActivePlayerUnit" && currentProfileOwner != ProfileOwner.enemyUnit)
        {
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
            activeCharacterPortrait.GetComponent<Image>().overrideSprite = detectedUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            activeCharacterName.text = detectedUnit.GetComponent<Unit>().unitTemplate.unitName;
            activeCharacterHealthPoints.text = detectedUnit.GetComponent<Unit>().unitHealthPoints.ToString();
            activeCharacterManaPoints.text = detectedUnit.GetComponent<Unit>().unitManaPoints.ToString();
            activeCharacterShieldPoints.text = detectedUnit.GetComponent<Unit>().unitShieldPoints.ToString();
            activeCharacterOpportunityPoints.text = detectedUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
            currentProfileOwner = ProfileOwner.activePlayerUnit;

        }
        //GameObject activePlayerUnit = detectedUnit;//GameObject.FindGameObjectWithTag("ActivePlayerUnit");
    }

    public void InitializePlayerProfileSliders()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        activeCharacterHealthPointsSlider.maxValue = activePlayerUnit.GetComponent<Unit>().unitHealthPoints;
        activeCharacterManaPointsSlider.maxValue = activePlayerUnit.GetComponent<Unit>().unitManaPoints;
        activeCharacterShieldPointsSlider.maxValue = activePlayerUnit.GetComponent<Unit>().unitShieldPoints;
        activeCharacterOpportunityPointsSlider.maxValue = activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints;
        activeCharacterHealthPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitHealthPoints;
        activeCharacterManaPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitManaPoints;
        activeCharacterShieldPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitShieldPoints;
        activeCharacterOpportunityPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints;
    }

    public void UpdatePlayerProfileSliders()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        activeCharacterHealthPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitHealthPoints;
        activeCharacterManaPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitManaPoints;
        activeCharacterShieldPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitShieldPoints;
        activeCharacterOpportunityPointsSlider.value = activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints;
    }

    void ResetPlayerProfile()
    {
        //GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        /*this.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        activeCharacterPortrait.GetComponent<Image>().overrideSprite = null;
        activeCharacterName.text = "";
        activeCharacterHealthPoints.text = "";
        activeCharacterManaPoints.text = "";
        activeCharacterShieldPoints.text = "";
        activeCharacterOpportunityPoints.text = "";*/
    }
}
