using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerProfileController;
using UnityEngine.UI;

public class DeityAltarController : MonoBehaviour
{
    //public GameObject playerPartyMemberProfileGO;
    public RectTransform playerPartyContainer;
    public Unit selectedPlayerUnit;
    public RectTransform capturedDeitiesContainer;
    public GameObject playerUnitImageGO;
    public GameObject deityImageGO;
    public GameObject deityLinkButtonPrefab;
    public RectTransform deityLinkMenuContainer;


    public void Start()
    {
        foreach (var playerUnit in GameManager.Instance.playerPartyMembersInstances)
        {
            Sprite playerUnitPortrait = playerUnit.GetComponent<Unit>().unitTemplate.unitPortrait;
            GameObject newPlayerUnitImage = Instantiate(playerUnitImageGO, playerPartyContainer);
            newPlayerUnitImage.GetComponent<Image>().sprite = playerUnitPortrait;
            newPlayerUnitImage.tag = "Player";
            newPlayerUnitImage.GetComponent<UnitImageController>().unitReference = playerUnit;
        }
        foreach (var capturedDeity in GameManager.Instance.capturedDeities)
        {
            Sprite deityPortrait = capturedDeity.deityPortrait;
            GameObject newDeityUnitImage = Instantiate(deityImageGO, capturedDeitiesContainer);
            newDeityUnitImage.tag = "Deity";
            newDeityUnitImage.GetComponent<Image>().sprite = deityPortrait;
            newDeityUnitImage.GetComponent<UnitImageController>().deityReference = capturedDeity;
        }
        CreateLinkButton();
    }

    public void CreateLinkButton()
    {
        GameObject deityLinkButtonInstance = Instantiate(deityLinkButtonPrefab, deityLinkMenuContainer);
        Button currentdDeityLinkButton = deityLinkButtonInstance.GetComponent<Button>();
        currentdDeityLinkButton.onClick.AddListener(() => GameManager.Instance.GetComponent<DeityLinkController>().SaveGame());
    }

    public void SetCurrentSelectedUnit(Unit unit)
    {
        selectedPlayerUnit = unit;
    }

    public void AssignDeityToUnit(Deity deity)
    {
        selectedPlayerUnit.LinkedDeityId = deity.Id; // Link the Deity ID to the Unit
        Debug.Log("Linked Player to Deity"); // Any additional logic for effects on the Unit's stats or state
        GameManager.Instance.deityLinkController.SaveGame();
    }

}
