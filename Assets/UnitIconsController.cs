using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitIconsController : MonoBehaviour
{
    public GameObject unitWaitingIcon;
    public void DisplayWaitingIcon()
    {
        Debug.Log("Display Wait Icon");
        unitWaitingIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }

    public void HideWaitingIcon()
    {
        Debug.Log("Display Wait Icon");
        unitWaitingIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
    }
}
