using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileController : MonoBehaviour
{
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

    public void OnEnable()
    {
        UnitSelectionController.OnActiveCharacterSelected += UpdatePlayerProfile;
        UnitSelectionController.OnActiveCharacterSelected += InitializePlayerProfileSliders;
        UnitSelectionController.OnActiveCharacterDeselected += ResetPlayerProfile;
        SpellcastingController.OnCastedSpell += UpdatePlayerProfile;
        SpellcastingController.OnCastedSpell += UpdatePlayerProfileSliders;
    }
    public void OnDisable()
    {
        UnitSelectionController.OnActiveCharacterSelected -= UpdatePlayerProfile;
        UnitSelectionController.OnActiveCharacterSelected -= InitializePlayerProfileSliders;
        UnitSelectionController.OnActiveCharacterDeselected -= ResetPlayerProfile;
        SpellcastingController.OnCastedSpell -= UpdatePlayerProfile;
        SpellcastingController.OnCastedSpell -= UpdatePlayerProfileSliders;

    }
    public void UpdatePlayerProfile()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        activeCharacterPortrait.GetComponent<Image>().overrideSprite = activePlayerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
        activeCharacterName.text = activePlayerUnit.GetComponent<Unit>().unitTemplate.unitName;
        activeCharacterHealthPoints.text = activePlayerUnit.GetComponent<Unit>().unitHealthPoints.ToString();
        activeCharacterManaPoints.text = activePlayerUnit.GetComponent<Unit>().unitManaPoints.ToString();
        activeCharacterShieldPoints.text = activePlayerUnit.GetComponent<Unit>().unitShieldPoints.ToString();
        activeCharacterOpportunityPoints.text = activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
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
        activeCharacterPortrait.GetComponent<Image>().overrideSprite = null;
        activeCharacterName.text = "";
        activeCharacterHealthPoints.text = "";
        activeCharacterManaPoints.text = "";
        activeCharacterShieldPoints.text = "";
        activeCharacterOpportunityPoints.text = "";
    }
}
