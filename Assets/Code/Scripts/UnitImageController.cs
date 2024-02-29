using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitImageController : MonoBehaviour, IPointerClickHandler

{
    public enum IconSelectionStatus
    {
        basic,
        deitySelectionMode
    }

    public DeityAltarController deityAltarController;

    public IconSelectionStatus currentIconSelectionStatus;

    public void Start()
    {
        deityAltarController = GameObject.FindGameObjectWithTag("DeityAltarController").GetComponent<DeityAltarController>();
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
        if (currentIconSelectionStatus == IconSelectionStatus.basic)
        {
            deityAltarController.SetCurrentSelectedUnit(this.GetComponentInChildren<Unit>());
            currentIconSelectionStatus = IconSelectionStatus.deitySelectionMode;
        }
        else if (this.gameObject.tag == "Deity")
        {
            deityAltarController.AssignDeityToUnit(this.GetComponentInChildren<Deity>());
        }
    }

    public void HandleDeselection()
    {
        Debug.Log("Clicked with Right Button");
        deityAltarController.selectedPlayerUnit = null;
        currentIconSelectionStatus = IconSelectionStatus.basic;

    }
}