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
    public void OnEnable()
    {
        UnitSelectionController.OnActiveCharacterSelected += PopulatePlayerProfile;
        UnitSelectionController.OnActiveCharacterDeselected += ResetPlayerProfile;

    }
    public void OnDisable()
    {
        UnitSelectionController.OnActiveCharacterSelected -= PopulatePlayerProfile;
        UnitSelectionController.OnActiveCharacterDeselected -= ResetPlayerProfile;

    }
    void PopulatePlayerProfile()
    {
        GameObject activePlayerUnit = GameObject.FindGameObjectWithTag("ActivePlayerUnit");
        activeCharacterPortrait.GetComponent<Image>().overrideSprite = activePlayerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
        activeCharacterName.text = activePlayerUnit.GetComponent<Unit>().unitTemplate.unitName;
        activeCharacterHealthPoints.text = activePlayerUnit.GetComponent<Unit>().unitHealthPoints.ToString();
        activeCharacterManaPoints.text = activePlayerUnit.GetComponent<Unit>().unitManaPoints.ToString();
        activeCharacterShieldPoints.text = activePlayerUnit.GetComponent<Unit>().unitShieldPoints.ToString();
        activeCharacterOpportunityPoints.text = activePlayerUnit.GetComponent<Unit>().unitOpportunityPoints.ToString();
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
