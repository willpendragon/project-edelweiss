using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadialMenuEntry : MonoBehaviour
{

    public enum ActionType
    {
        Move,
        Melee,
        Spell
    }

    [SerializeField] private TextMeshProUGUI _actionLabel;
    public ActionType actionType;

    public void SetLabel(string labelText)
    {
        _actionLabel.text = labelText;
    }

    public void FireAction()
    {
        FindAnyObjectByType<CursorController>().ChangeCursorMode(actionType);
    }

}
