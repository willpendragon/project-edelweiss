using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMomentsController : MonoBehaviour
{
    public Image unitMoveCalloutImage;
    public void OnEnable()
    {
        AOESpellPlayerAction.OnSpellTriggeredCallout += ShowSpellCallout;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnSpellTriggeredCallout -= ShowSpellCallout;
    }

    public void ShowSpellCallout(Image spellCallout)
    {
        unitMoveCalloutImage.enabled = true;
    }


}
