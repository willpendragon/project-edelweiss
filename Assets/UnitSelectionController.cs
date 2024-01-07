using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridTargetingController;

public class UnitSelectionController : MonoBehaviour
{
    public GameObject activeCharacterSelectorIcon;
    private void OnMouseDown()
    {
        this.gameObject.tag = "ActivePlayerUnit";
        GameObject newActiveCharacterSelectorIcon = Instantiate(activeCharacterSelectorIcon, transform.localPosition + (transform.up * 3), Quaternion.identity);
    }
}
