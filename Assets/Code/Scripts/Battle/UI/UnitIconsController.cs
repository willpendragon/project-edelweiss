using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitIconsController : MonoBehaviour
{
    public GameObject unitWaitingIcon;
    public void DisplayWaitingIcon()
    {
        unitWaitingIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }

    public void HideWaitingIcon()
    {
        unitWaitingIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
    }
}
