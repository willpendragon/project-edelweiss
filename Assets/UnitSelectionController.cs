using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GridTargetingController;

public class UnitSelectionController : MonoBehaviour
{
    public enum UnitSelectionStatus
    {
        unitSelected,
        unitDeselected
    }
    public delegate void ActiveCharacterSelected();
    public static event ActiveCharacterSelected OnActiveCharacterSelected;
    public delegate void ActiveCharacterDeselected();
    public static event ActiveCharacterDeselected OnActiveCharacterDeselected;

    public UnitSelectionStatus currentUnitSelectionStatus;

    public void Start()
    {
        currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
    }
    public GameObject activeCharacterSelectorIcon;
    private void OnMouseDown()
    {
        if (currentUnitSelectionStatus == UnitSelectionStatus.unitDeselected && GridManager.Instance.currentPlayerUnit == null)
        {
            this.gameObject.tag = "ActivePlayerUnit";
            GameObject newActiveCharacterSelectorIcon = Instantiate(activeCharacterSelectorIcon, transform.localPosition + (transform.up * 3), Quaternion.identity);
            currentUnitSelectionStatus = UnitSelectionStatus.unitSelected;
            GridManager.Instance.currentPlayerUnit = this.gameObject;
            OnActiveCharacterSelected();
        }
        else if (currentUnitSelectionStatus == UnitSelectionStatus.unitSelected)
        {
            this.gameObject.tag = "Player";
            GridManager.Instance.currentPlayerUnit = null;
            currentUnitSelectionStatus = UnitSelectionStatus.unitDeselected;
            OnActiveCharacterDeselected();
            //DestroySelectedIcon
        }
    }
}
