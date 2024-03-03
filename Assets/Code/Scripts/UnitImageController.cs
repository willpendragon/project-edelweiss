using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitImageController : MonoBehaviour, IPointerClickHandler

{
    public enum IconSelectionStatus
    {
        playerSelectionMode,
        deitySelectionMode
    }

    public DeityAltarController deityAltarController;

    public IconSelectionStatus currentIconSelectionStatus;

    public Unit unitReference;
    public Deity deityReference;

    public void Start()
    {
        deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
        if (deityReference != null)
        {
            currentIconSelectionStatus = IconSelectionStatus.deitySelectionMode;
        }
        else
        {
            currentIconSelectionStatus = IconSelectionStatus.playerSelectionMode;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleSelection();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleDeselection();
        }
    }

    public void HandleSelection()
    {
        Debug.Log("Clicked with Left Button");

        if (currentIconSelectionStatus == IconSelectionStatus.playerSelectionMode && this.gameObject.tag == "Player")
        {
            deityAltarController.SetCurrentSelectedUnit(unitReference);
            Debug.Log("Selected Player Unit");
        }
        else if (this.gameObject.tag == "Deity" && currentIconSelectionStatus == IconSelectionStatus.deitySelectionMode)
        {
            deityAltarController.AssignDeityToUnit(this.deityReference);
        }
    }

    public void HandleDeselection()
    {
        Debug.Log("Clicked with Right Button");
        deityAltarController.selectedPlayerUnit = null;
        currentIconSelectionStatus = IconSelectionStatus.playerSelectionMode;
    }
}